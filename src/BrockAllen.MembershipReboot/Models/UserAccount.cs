﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    public class UserAccount
    {
        internal const int VerificationKeyStaleDurationDays = 1;

        internal protected UserAccount()
        {
            this.Claims = new List<UserClaim>();
        }

        internal protected UserAccount(string tenant, string username, string password, string email)
        {
            if (String.IsNullOrWhiteSpace(tenant)) throw new ArgumentException("tenant");
            if (String.IsNullOrWhiteSpace(username)) throw new ArgumentException("username");
            if (String.IsNullOrWhiteSpace(password)) throw new ArgumentException("password");
            if (String.IsNullOrWhiteSpace(email)) throw new ArgumentException("email");

            this.NameID = Guid.NewGuid();
            this.Tenant = tenant;
            this.Username = username;
            this.Email = email;
            this.Created = this.UtcNow;
            this.SetPassword(password);
            this.IsAccountVerified = !SecuritySettings.Instance.RequireAccountVerification;
            this.IsLoginAllowed = SecuritySettings.Instance.AllowLoginAfterAccountCreation;
            this.Claims = new List<UserClaim>();

            if (SecuritySettings.Instance.RequireAccountVerification)
            {
                this.SetVerificationKey(VerificationKeyPurpose.VerifyAccount);
            }
        }

        [Key]
        public virtual int ID { get; set; }
        public virtual Guid NameID { get; set; }

        [StringLength(50)]
        [Required]
        public virtual string Tenant { get; internal set; }
        [StringLength(100)]
        [Required]
        public virtual string Username { get; internal set; }
        [EmailAddress]
        [StringLength(100)]
        [Required]
        public virtual string Email { get; internal set; }

        public virtual DateTime Created { get; internal set; }
        public virtual DateTime PasswordChanged { get; internal set; }

        public virtual bool IsAccountVerified { get; internal set; }
        public virtual bool IsLoginAllowed { get; set; }
        public virtual bool IsAccountClosed { get; internal set; }
        public virtual DateTime? AccountClosed { get; internal set; }

        public virtual DateTime? LastLogin { get; internal set; }
        public virtual DateTime? LastFailedLogin { get; internal set; }
        public virtual int FailedLoginCount { get; internal set; }

        [StringLength(100)]
        public virtual string VerificationKey { get; internal set; }
        public virtual VerificationKeyPurpose? VerificationPurpose { get; set; }
        public virtual DateTime? VerificationKeySent { get; internal set; }

        [Required]
        [StringLength(200)]
        public virtual string HashedPassword { get; internal set; }

        public virtual ICollection<UserClaim> Claims { get; internal set; }

        internal void SetVerificationKey(VerificationKeyPurpose purpose, string prefix = null)
        {
            var key = prefix + StripUglyBase64(this.GenerateSalt());
            this.VerificationKey = key;
            this.VerificationPurpose = purpose;
            this.VerificationKeySent = UtcNow;
        }
        
        internal void ClearVerificationKey()
        {
            this.VerificationKey = null;
            this.VerificationPurpose = null;
            this.VerificationKeySent = null;
        }

        protected internal virtual bool VerifyAccount(string key)
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                Tracing.Verbose("[UserAccount.VerifyAccount] failed -- no key");
                return false;
            }

            if (IsAccountVerified)
            {
                Tracing.Verbose("[UserAccount.VerifyAccount] failed -- account already verified");
                return false;
            }

            if (this.VerificationPurpose != VerificationKeyPurpose.VerifyAccount)
            {
                Tracing.Verbose("[UserAccount.VerifyAccount] failed -- verification purpose invalid");
                return false;
            }

            if (this.VerificationKey != key)
            {
                Tracing.Verbose("[UserAccount.VerifyAccount] failed -- verification key doesn't match");
                return false;
            }

            this.IsAccountVerified = true;
            this.ClearVerificationKey();

            return true;
        }

        protected internal virtual bool ChangePassword(string oldPassword, string newPassword, int failedLoginCount, TimeSpan lockoutDuration)
        {
            if (Authenticate(oldPassword, failedLoginCount, lockoutDuration))
            {
                if (oldPassword == newPassword)
                {
                    Tracing.Verbose(String.Format("[UserAccount.ChangePassword] failed for tenant:user {0}:{1} -- new password same as old password", this.Tenant, this.Username));

                    throw new ValidationException("The new password must be different than the old password.");
                }

                SetPassword(newPassword);
                return true;
            }

            Tracing.Verbose(String.Format("[UserAccount.ChangePassword] failed for tentant:username {0}:{1} -- auth failed", this.Tenant, this.Username));

            return false;
        }

        protected internal virtual void SetPassword(string password)
        {
            if (String.IsNullOrWhiteSpace(password))
            {
                Tracing.Verbose("[UserAccount.SetPassword] failed -- no password provided");

                throw new ValidationException("Invalid password");
            }

            Tracing.Verbose("[UserAccount.SetPassword] setting new password hash");

            HashedPassword = HashPassword(password);
            PasswordChanged = UtcNow;
        }

        protected internal virtual bool IsVerificationKeyStale
        {
            get
            {
                if (VerificationKeySent == null)
                {
                    return true;
                }

                if (this.VerificationKeySent < UtcNow.AddDays(-VerificationKeyStaleDurationDays))
                {
                    return true;
                }

                return false;
            }
        }

        protected internal virtual bool ResetPassword()
        {
            // if they've not yet verified then don't allow changes
            if (!this.IsAccountVerified)
            {
                Tracing.Verbose("[UserAccount.ResetPassword] failed -- account not verified");

                return false;
            }

            // if there's no current key, or if there is a key but 
            // it's older than one day, create a new reset key
            if (IsVerificationKeyStale)
            {
                Tracing.Verbose("[UserAccount.ResetPassword] creating new verification keys");
                this.SetVerificationKey(VerificationKeyPurpose.ChangePassword);
            }
            else
            {
                Tracing.Verbose("[UserAccount.ResetPassword] not creating new verification keys");
            }

            return true;
        }

        protected internal virtual bool ChangePasswordFromResetKey(string key, string newPassword)
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                Tracing.Verbose("[UserAccount.ChangePasswordFromResetKey] failed -- no key");
                return false;
            }

            if (!this.IsAccountVerified)
            {
                Tracing.Verbose("[UserAccount.ChangePasswordFromResetKey] failed -- account not verified");
                return false;
            }

            // if the key is too old don't honor it
            if (IsVerificationKeyStale)
            {
                Tracing.Verbose("[UserAccount.ChangePasswordFromResetKey] failed -- verification key too old");
                return false;
            }

            if (this.VerificationPurpose != VerificationKeyPurpose.ChangePassword)
            {
                Tracing.Verbose("[UserAccount.ChangePasswordFromResetKey] failed -- invalid verification key purpose");
                return false;
            }

            // check if key matches
            if (this.VerificationKey != key)
            {
                Tracing.Verbose("[UserAccount.ChangePasswordFromResetKey] failed -- verification keys don't match");
                return false;
            }

            this.ClearVerificationKey();
            this.SetPassword(newPassword);

            return true;
        }

        protected internal virtual bool Authenticate(string password, int failedLoginCount, TimeSpan lockoutDuration)
        {
            if (failedLoginCount <= 0) throw new ArgumentException("failedLoginCount");

            if (String.IsNullOrWhiteSpace(password))
            {
                Tracing.Verbose("[UserAccount.Authenticate] failed -- no password");
                return false;
            }

            if (!IsAccountVerified)
            {
                Tracing.Verbose("[UserAccount.Authenticate] failed -- account not verified");
                return false;
            }
            if (!IsLoginAllowed)
            {
                Tracing.Verbose("[UserAccount.Authenticate] failed -- account not allowed to login");
                return false;
            }

            if (HasTooManyRecentPasswordFailures(failedLoginCount, lockoutDuration))
            {
                Tracing.Verbose("[UserAccount.Authenticate] failed -- account in lockout due to failed login attempts");

                FailedLoginCount++;
                return false;
            }

            var valid = VerifyHashedPassword(password);
            if (valid)
            {
                Tracing.Verbose("[UserAccount.Authenticate] authentication success");

                LastLogin = UtcNow;
                FailedLoginCount = 0;
            }
            else
            {
                Tracing.Verbose("[UserAccount.Authenticate] failed -- invalid password");

                LastFailedLogin = UtcNow;
                if (FailedLoginCount > 0) FailedLoginCount++;
                else FailedLoginCount = 1;
            }

            return valid;
        }

        protected internal virtual bool HasTooManyRecentPasswordFailures(int failedLoginCount, TimeSpan lockoutDuration)
        {
            if (failedLoginCount <= 0) throw new ArgumentException("failedLoginCount");

            if (failedLoginCount <= FailedLoginCount)
            {
                return LastFailedLogin >= UtcNow.Subtract(lockoutDuration);
            }

            return false;
        }

        protected internal virtual bool ChangeEmailRequest(string newEmail)
        {
            if (String.IsNullOrWhiteSpace(newEmail)) throw new ValidationException("Invalid email.");

            // if they've not yet verified then fail
            if (!this.IsAccountVerified)
            {
                Tracing.Verbose("[UserAccount.ChangeEmailRequest] failed -- account not verified");
                return false;
            }

            var lowerEmail = newEmail.ToLower(new System.Globalization.CultureInfo("tr-TR", false));
            var emailHash = StripUglyBase64(Hash(lowerEmail));

            // if there's no current key, or it's not a change email key
            // or if there is a key but it's older than one day, then create 
            // a new reset key
            if (IsVerificationKeyStale ||
                this.VerificationPurpose != VerificationKeyPurpose.ChangeEmail ||
                !this.VerificationKey.StartsWith(emailHash))
            {
                Tracing.Verbose("[UserAccount.ChangeEmailRequest] creating a new reset key");
                this.SetVerificationKey(VerificationKeyPurpose.ChangeEmail, emailHash);
            }
            else
            {
                Tracing.Verbose("[UserAccount.ChangeEmailRequest] not creating a new reset key");
            }

            return true;
        }

        protected internal virtual bool ChangeEmailFromKey(string key, string newEmail)
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                Tracing.Verbose("[UserAccount.ChangeEmailFromKey] failed -- no key");
                return false;
            }
            if (String.IsNullOrWhiteSpace(newEmail)) throw new ValidationException("Invalid email.");

            // only honor resets within the past day
            if (!IsVerificationKeyStale)
            {
                if (this.VerificationPurpose == VerificationKeyPurpose.ChangeEmail)
                {
                    if (key == this.VerificationKey)
                    {
                        var lowerEmail = newEmail.ToLower(new System.Globalization.CultureInfo("tr-TR", false));
                        var emailHash = StripUglyBase64(Hash(lowerEmail));
                        if (this.VerificationKey.StartsWith(emailHash))
                        {
                            this.Email = newEmail;
                            this.ClearVerificationKey();

                            return true;
                        }
                        else
                        {
                            Tracing.Verbose("[UserAccount.ChangeEmailFromKey] failed -- email in key doesn't match email in verification key");
                        }
                    }
                    else
                    {
                        Tracing.Verbose("[UserAccount.ChangeEmailFromKey] failed -- verification keys don't match");
                    }
                }
                else
                {
                    Tracing.Verbose("[UserAccount.ChangeEmailFromKey] failed -- invalid purpose");
                }
            }
            else
            {
                Tracing.Verbose("[UserAccount.ChangeEmailFromKey] failed -- verification key is stale");
            }

            return false;
        }

        protected internal virtual void CloseAccount()
        {
            Tracing.Verbose(String.Format("[UserAccount.CloseAccount] called on: {0}, {1}", Tenant, Username));

            this.ClearVerificationKey();
            IsLoginAllowed = false;
            IsAccountClosed = true;
            AccountClosed = UtcNow;
        }

        protected internal virtual bool IsPasswordExpired
        {
            get
            {
                var frequency = SecuritySettings.Instance.PasswordResetFrequency;
                if (frequency <= 0) return false;

                var now = this.UtcNow;
                var last = this.PasswordChanged;
                return last.AddDays(frequency) <= now;
            }
        }

        public virtual bool HasClaim(string type)
        {
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            return this.Claims.Any(x => x.Type == type);
        }

        public virtual bool HasClaim(string type, string value)
        {
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");
            if (String.IsNullOrWhiteSpace(value)) throw new ArgumentException("value");

            return this.Claims.Any(x => x.Type == type && x.Value == value);
        }

        public virtual IEnumerable<string> GetClaimValues(string type)
        {
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            var query =
                from claim in this.Claims
                where claim.Type == type
                select claim.Value;
            return query.ToArray();
        }

        public virtual string GetClaimValue(string type)
        {
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            var query =
                from claim in this.Claims
                where claim.Type == type
                select claim.Value;
            return query.SingleOrDefault();
        }

        public virtual void AddClaim(string type, string value)
        {
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");
            if (String.IsNullOrWhiteSpace(value)) throw new ArgumentException("value");

            if (!this.HasClaim(type, value))
            {
                Tracing.Verbose(String.Format("[UserAccount.AddClaim] {0}, {1}, {2}, {3}", Tenant, Username, type, value));

                this.Claims.Add(
                    new UserClaim
                    {
                        Type = type,
                        Value = value
                    });
            }
        }

        public virtual void RemoveClaim(string type)
        {
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            var claimsToRemove =
                from claim in this.Claims
                where claim.Type == type
                select claim;
            foreach (var claim in claimsToRemove.ToArray())
            {
                Tracing.Verbose(String.Format("[UserAccount.RemoveClaim] {0}, {1}, {2}, {3}", Tenant, Username, type, claim.Value));
                this.Claims.Remove(claim);
            }
        }

        public virtual void RemoveClaim(string type, string value)
        {
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");
            if (String.IsNullOrWhiteSpace(value)) throw new ArgumentException("value");

            var claimsToRemove =
                from claim in this.Claims
                where claim.Type == type && claim.Value == value
                select claim;
            foreach (var claim in claimsToRemove.ToArray())
            {
                Tracing.Verbose(String.Format("[UserAccount.RemoveClaim] {0}, {1}, {2}, {3}", Tenant, Username, type, value));
                this.Claims.Remove(claim);
            }
        }

        static readonly string[] UglyBase64 = { "+", "/", "=" };
        static string StripUglyBase64(string s)
        {
            if (s == null) return s;
            foreach (var ugly in UglyBase64)
            {
                s = s.Replace(ugly, "");
            }
            return s;
        }

        protected internal virtual string Hash(string value)
        {
            return CryptoHelper.Hash(value);
        }

        protected internal virtual string GenerateSalt()
        {
            return CryptoHelper.GenerateSalt();
        }

        protected internal virtual string HashPassword(string password)
        {
            return CryptoHelper.HashPassword(password);
        }

        protected internal virtual bool VerifyHashedPassword(string password)
        {
            return CryptoHelper.VerifyHashedPassword(HashedPassword, password);
        }

        protected internal virtual DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }
    }
}
