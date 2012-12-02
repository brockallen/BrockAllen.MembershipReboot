using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace BrockAllen.MembershipReboot
{
    public class UserAccount
    {
        public static UserAccount Create(string username, string password, string email)
        {
            if (SecuritySettings.Instance.EmailIsUsername && username != email)
            {
                throw new ValidationException("Username must be the same as the Email");
            }

            UserAccount account = new UserAccount
            {
                Username = username,
                Email = email,
                Created = DateTime.UtcNow,
                IsAccountVerified = !SecuritySettings.Instance.RequireAccountVerification,
                IsLoginAllowed = SecuritySettings.Instance.AllowLoginAfterAccountCreation,
                Claims = new List<UserClaim>()
            };

            account.SetPassword(password);
            if (SecuritySettings.Instance.RequireAccountVerification)
            {
                account.VerificationKey = StripUglyBase64(Crypto.GenerateSalt());
                account.VerificationKeySent = DateTime.UtcNow;
            }

            return account;
        }

        [Key]
        public virtual string Username { get; private set; }
        [EmailAddress]
        public virtual string Email { get; private set; }

        public virtual DateTime Created { get; private set; }
        public virtual DateTime PasswordChanged { get; private set; }

        public virtual bool IsAccountVerified { get; private set; }
        public virtual bool IsLoginAllowed { get; set; }
        public virtual bool IsAccountClosed { get; set; }

        public virtual DateTime? LastLogin { get; private set; }
        public virtual DateTime? LastFailedLogin { get; private set; }
        public virtual int FailedLoginCount { get; private set; }

        public string VerificationKey { get; private set; }
        public DateTime? VerificationKeySent { get; private set; }

        [Required]
        public virtual string HashedPassword { get; private set; }

        public virtual ICollection<UserClaim> Claims { get; private set; }

        public bool VerifyAccount(string key)
        {
            if (String.IsNullOrWhiteSpace(key)) return false;
            if (IsAccountVerified) return false;
            if (this.VerificationKey != key) return false;

            this.IsAccountVerified = true;
            this.VerificationKey = null;
            this.VerificationKeySent = null;

            return true;
        }

        public bool ChangePassword(string oldPassword, string newPassword, int failedLoginCount, TimeSpan lockoutDuration)
        {
            if (Authenticate(oldPassword, failedLoginCount, lockoutDuration))
            {
                SetPassword(newPassword);
                return true;
            }

            return false;
        }

        public void SetPassword(string password)
        {
            if (String.IsNullOrWhiteSpace(password))
            {
                throw new ValidationException("Invalid password");
            }
            
            HashedPassword = Crypto.HashPassword(password);
            PasswordChanged = DateTime.UtcNow;
        }

        public virtual bool ResetPassword(string email)
        {
            if (!Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)) return false;
            
            // if they've not yet verified, then just use the current
            // verification key
            if (!this.IsAccountVerified) return true;

            // if there's no current key, or if there is a key but 
            // it's older than one day, create a new reset key
            if (this.VerificationKeySent == null ||
                this.VerificationKeySent < DateTime.UtcNow.AddDays(1))
            {
                this.VerificationKey = StripUglyBase64(Crypto.GenerateSalt());
                this.VerificationKeySent = DateTime.UtcNow;
            }

            return true;
        }

        public virtual bool ChangePasswordFromResetKey(string key, string newPassword)
        {
            if (String.IsNullOrWhiteSpace(key)) return false;
            if (!this.IsAccountVerified) return false;

            // if the key was sent more than a day, don't honor it
            if (TimeSpan.FromDays(1) < DateTime.UtcNow.Subtract(this.VerificationKeySent.Value))
            {
                return false;
            }

            // check if key matches
            if (this.VerificationKey != key) return false;

            this.VerificationKey = null;
            this.VerificationKeySent = null;
            this.SetPassword(newPassword);

            return true;
        }

        public bool Authenticate(string password, int failedLoginCount, TimeSpan lockoutDuration)
        {
            if (failedLoginCount <= 0) throw new ArgumentException("failedLoginCount");

            if (String.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            if (!IsAccountVerified) return false;
            if (!IsLoginAllowed) return false;

            if (failedLoginCount <= FailedLoginCount &&
                LastFailedLogin <= DateTime.UtcNow.Add(lockoutDuration))
            {
                FailedLoginCount++;
                return false;
            }

            var valid = Crypto.VerifyHashedPassword(HashedPassword, password);
            if (valid)
            {
                LastLogin = DateTime.UtcNow;
                FailedLoginCount = 0;
            }
            else
            {
                LastFailedLogin = DateTime.UtcNow;
                if (FailedLoginCount > 0) FailedLoginCount++;
                else FailedLoginCount = 1;
            }

            return valid;
        }

        public void AddClaim(string type, string value)
        {
            RemoveClaim(type, value);

            this.Claims.Add(
                new UserClaim
                {
                    Type = type,
                    Value = value
                });
        }

        public void RemoveClaim(string username, string type)
        {
            var claimsToRemove =
                from claim in this.Claims
                where claim.Type == type
                select claim;
            foreach (var claim in claimsToRemove)
            {
                this.Claims.Remove(claim);
            }
        }

        public void RemoveClaim(string username, string type, string value)
        {
            var claimsToRemove =
                from claim in this.Claims
                where claim.Type == type && claim.Value == value
                select claim;
            foreach (var claim in claimsToRemove)
            {
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
    }
}
