/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace BrockAllen.MembershipReboot
{
    public class UserAccount : IEventSource
    {
        public UserAccount()
        {
            this.Claims = new HashSet<UserClaim>();
            this.LinkedAccounts = new HashSet<LinkedAccount>();
            this.Certificates = new HashSet<UserCertificate>();
        }

        [Key]
        public virtual Guid ID { get; internal set; }

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
        public virtual DateTime LastUpdated { get; internal set; }
        public virtual DateTime PasswordChanged { get; internal set; }
        public virtual bool RequiresPasswordReset { get; set; }

        public virtual string MobileCode { get; internal set; }
        public virtual DateTime? MobileCodeSent { get; internal set; }
        public virtual string MobilePhoneNumber { get; internal set; }

        public virtual TwoFactorAuthMode AccountTwoFactorAuthMode { get; internal set; }
        public virtual TwoFactorAuthMode CurrentTwoFactorAuthStatus { get; internal set; }

        public virtual bool IsAccountVerified { get; internal set; }
        public virtual bool IsLoginAllowed { get; set; }
        public virtual bool IsAccountClosed { get; internal set; }
        public virtual DateTime? AccountClosed { get; internal set; }

        public virtual DateTime? LastLogin { get; internal set; }
        public virtual DateTime? LastFailedLogin { get; internal set; }
        public virtual int FailedLoginCount { get; internal set; }

        [StringLength(100)]
        public virtual string VerificationKey { get; internal set; }
        public virtual VerificationKeyPurpose? VerificationPurpose { get; internal set; }
        public virtual DateTime? VerificationKeySent { get; internal set; }
        [StringLength(100)]
        public virtual string VerificationStorage { get; internal set; }

        [Required]
        [StringLength(200)]
        public virtual string HashedPassword { get; internal set; }

        public virtual ICollection<UserClaim> Claims { get; internal set; }
        public virtual ICollection<LinkedAccount> LinkedAccounts { get; internal set; }
        public virtual ICollection<UserCertificate> Certificates { get; internal set; }
        public virtual ICollection<TwoFactorAuthToken> TwoFactorAuthTokens { get; internal set; }

        List<IEvent> events = new List<IEvent>();
        IEnumerable<IEvent> IEventSource.GetEvents()
        {
            return events;
        }
        void IEventSource.Clear()
        {
            events.Clear();
        }
        protected internal void AddEvent<E>(E evt) where E : IEvent
        {
            if (evt is IAllowMultiple ||
                !events.Any(x => x.GetType() == evt.GetType()))
            {
                events.Add(evt);
            }
        }

        internal protected virtual void Init(string tenant, string username, string password, string email)
        {
            Tracing.Information("[UserAccount.Init] called");

            if (String.IsNullOrWhiteSpace(tenant))
            {
                Tracing.Error("[UserAccount.Init] failed -- no tenant");
                throw new ArgumentNullException("tenant");
            }
            if (String.IsNullOrWhiteSpace(username))
            {
                Tracing.Error("[UserAccount.Init] failed -- no username");
                throw new ValidationException("Username is required.");
            }
            if (String.IsNullOrWhiteSpace(password))
            {
                Tracing.Error("[UserAccount.Init] failed -- no password");
                throw new ValidationException("Password is required.");
            }
            if (String.IsNullOrWhiteSpace(email))
            {
                Tracing.Error("[UserAccount.Init] failed -- no email");
                throw new ValidationException("Email is required.");
            }

            if (this.ID != Guid.Empty)
            {
                Tracing.Error("[UserAccount.Init] failed -- ID already assigned");
                throw new Exception("Can't call Init if UserAccount is already assigned an ID");
            }

            this.ID = Guid.NewGuid();
            this.Tenant = tenant;
            this.Username = username;
            this.Email = email;
            this.Created = this.UtcNow;
            this.LastUpdated = this.Created;
            this.HashedPassword = HashPassword(password);
            this.PasswordChanged = this.Created;
            this.IsAccountVerified = false;
            this.IsLoginAllowed = false;
            this.AccountTwoFactorAuthMode = TwoFactorAuthMode.None;
            this.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.None;
            var key = this.SetVerificationKey(VerificationKeyPurpose.VerifyAccount);

            this.AddEvent(new AccountCreatedEvent { Account = this, VerificationKey = key });
        }

        protected internal virtual bool IsVerificationKeyStale
        {
            get
            {
                if (VerificationKeySent == null)
                {
                    return true;
                }

                if (this.VerificationKeySent < UtcNow.AddMinutes(-MembershipRebootConstants.UserAccount.VerificationKeyStaleDurationMinutes))
                {
                    return true;
                }

                return false;
            }
        }

        internal string SetVerificationKey(VerificationKeyPurpose purpose, string key = null, string state = null)
        {
            if (key == null) key = StripUglyBase64(this.GenerateSalt());

            this.VerificationKey = CryptoHelper.Hash(key);
            this.VerificationPurpose = purpose;
            this.VerificationKeySent = UtcNow;
            this.VerificationStorage = state;

            return key;
        }

        internal bool IsVerificationKeyValid(VerificationKeyPurpose purpose, string key)
        {
            if (!IsVerificationPurposeValid(purpose))
            {
                return false;
            }

            var hashedKey = CryptoHelper.Hash(key);
            var result = SlowEquals(this.VerificationKey, hashedKey);
            if (!result)
            {
                Tracing.Error("[UserAccount.IsVerificationKeyValid] failed -- verification key doesn't match");
                return false;
            }

            Tracing.Verbose("[UserAccount.IsVerificationKeyValid] success -- verification key valid");
            return true;
        }
        
        internal bool IsVerificationPurposeValid(VerificationKeyPurpose purpose)
        {
            if (this.VerificationPurpose != purpose)
            {
                Tracing.Error("[UserAccount.IsVerificationPurposeValid] failed -- verification purpose invalid");
                return false;
            }

            if (IsVerificationKeyStale)
            {
                Tracing.Error("[UserAccount.IsVerificationPurposeValid] failed -- verification key stale");
                return false;
            }

            Tracing.Verbose("[UserAccount.IsVerificationPurposeValid] success -- verification purpose valid");
            return true;
        }

        internal void ClearVerificationKey()
        {
            this.VerificationKey = null;
            this.VerificationPurpose = null;
            this.VerificationKeySent = null;
            this.VerificationStorage = null;
        }

        protected internal virtual bool VerifyAccount(string key, string password)
        {
            Tracing.Information("[UserAccount.VerifyAccount] called for accountID: {0}", this.ID);

            if (String.IsNullOrWhiteSpace(key))
            {
                Tracing.Error("[UserAccount.VerifyAccount] failed -- no key");
                return false;
            }
            
            if (String.IsNullOrWhiteSpace(password))
            {
                Tracing.Error("[UserAccount.VerifyAccount] failed -- no password");
                return false;
            }

            if (IsAccountVerified)
            {
                Tracing.Error("[UserAccount.VerifyAccount] failed -- account already verified");
                return false;
            }

            if (!IsVerificationKeyValid(VerificationKeyPurpose.VerifyAccount, key))
            {
                Tracing.Error("[UserAccount.VerifyAccount] failed -- key verification failed");
                return false;
            }

            if (!VerifyHashedPassword(password))
            {
                Tracing.Error("[UserAccount.VerifyAccount] failed -- invalid password");
                return false;
            }

            Tracing.Verbose("[UserAccount.VerifyAccount] succeeded");

            this.VerifyAccount();

            return true;
        }

        protected internal virtual void VerifyAccount()
        {
            this.IsAccountVerified = true;
            this.ClearVerificationKey();
            this.AddEvent(new AccountVerifiedEvent { Account = this });
        }

        protected internal virtual bool CancelNewAccount(string key)
        {
            Tracing.Information("[UserAccount.CancelNewAccount] called for accountID: {0}", this.ID);

            if (this.IsAccountVerified)
            {
                Tracing.Error("[UserAccount.CancelNewAccount] failed -- account already verified");
                return false;
            }

            if (!IsVerificationKeyValid(VerificationKeyPurpose.VerifyAccount, key))
            {
                Tracing.Error("[UserAccount.CancelNewAccount] failed -- key verification failed");
                return false;
            } 
            
            Tracing.Verbose("[UserAccount.CancelNewAccount] succeeded (closing account)");
            
            this.CloseAccount();

            return true;
        }

        protected internal virtual void SetPassword(string password)
        {
            Tracing.Information("[UserAccount.SetPassword] called for accountID: {0}", this.ID);

            if (String.IsNullOrWhiteSpace(password))
            {
                Tracing.Error("[UserAccount.SetPassword] failed -- no password provided");
                throw new ValidationException("Invalid password.");
            }

            Tracing.Verbose("[UserAccount.SetPassword] setting new password hash");

            HashedPassword = HashPassword(password);
            PasswordChanged = UtcNow;
            RequiresPasswordReset = false;

            this.AddEvent(new PasswordChangedEvent { Account = this, NewPassword = password });
        }

        protected internal virtual void ResetPassword()
        {
            Tracing.Information("[UserAccount.ResetPassword] called for accountID: {0}", this.ID);

            if (!this.IsAccountVerified)
            {
                Tracing.Verbose("[UserAccount.ResetPassword] creating new verification key because existing one is stale");
                var key = this.SetVerificationKey(VerificationKeyPurpose.VerifyAccount);
                
                // if they've not yet verified then don't allow changes
                // instead raise an event as if the account was just created to 
                // the user re-recieves their notification
                Tracing.Verbose("[UserAccount.ResetPassword] account not verified -- raising account create to resend notification");
                this.AddEvent(new AccountCreatedEvent { Account = this, VerificationKey = key });
            }
            else
            {
                Tracing.Verbose("[UserAccount.ResetPassword] creating new verification keys");
                var key = this.SetVerificationKey(VerificationKeyPurpose.ResetPassword);
                
                Tracing.Verbose("[UserAccount.ResetPassword] account verified -- raising event to send reset notification");
                
                this.AddEvent(new PasswordResetRequestedEvent { Account = this, VerificationKey = key });
            }
        }

        protected internal virtual bool ChangePasswordFromResetKey(string key, string newPassword)
        {
            Tracing.Information("[UserAccount.ChangePasswordFromResetKey] called for accountID: {0}", this.ID);

            if (String.IsNullOrWhiteSpace(key))
            {
                Tracing.Error("[UserAccount.ChangePasswordFromResetKey] failed -- no key");
                return false;
            }

            if (!this.IsAccountVerified)
            {
                Tracing.Error("[UserAccount.ChangePasswordFromResetKey] failed -- account not verified");
                return false;
            }

            if (!IsVerificationKeyValid(VerificationKeyPurpose.ResetPassword, key))
            {
                Tracing.Error("[UserAccount.ChangePasswordFromResetKey] failed -- key verification failed");
                return false;
            }

            Tracing.Verbose("[UserAccount.ChangePasswordFromResetKey] success");

            this.ClearVerificationKey();
            this.SetPassword(newPassword);

            return true;
        }

        protected internal virtual bool Authenticate(string password, int failedLoginCount, TimeSpan lockoutDuration)
        {
            Tracing.Information("[UserAccount.Authenticate] called for accountID: {0}", this.ID);

            if (failedLoginCount <= 0) throw new ArgumentException("failedLoginCount");

            if (String.IsNullOrWhiteSpace(password))
            {
                Tracing.Error("[UserAccount.Authenticate] failed -- no password");
                return false;
            }

            if (!IsAccountVerified)
            {
                Tracing.Error("[UserAccount.Authenticate] failed -- account not verified");
                this.AddEvent(new AccountNotVerifiedEvent { Account = this });
                return false;
            }

            if (IsAccountClosed)
            {
                Tracing.Error("[UserAccount.Authenticate] failed -- account closed");
                return false;
            }
            
            if (!IsLoginAllowed)
            {
                Tracing.Error("[UserAccount.Authenticate] failed -- account not allowed to login");
                this.AddEvent(new AccountLockedEvent { Account = this });
                return false;
            }

            if (HasTooManyRecentPasswordFailures(failedLoginCount, lockoutDuration))
            {
                Tracing.Error("[UserAccount.Authenticate] failed -- account in lockout due to failed login attempts");

                FailedLoginCount++;
                this.AddEvent(new TooManyRecentPasswordFailuresEvent { Account = this });

                return false;
            }

            var valid = VerifyHashedPassword(password);
            if (valid)
            {
                Tracing.Verbose("[UserAccount.Authenticate] authentication success");

                LastLogin = UtcNow;
                FailedLoginCount = 0;

                this.AddEvent(new SuccessfulPasswordLoginEvent { Account = this });
            }
            else
            {
                Tracing.Error("[UserAccount.Authenticate] failed -- invalid password");

                LastFailedLogin = UtcNow;
                if (FailedLoginCount > 0) FailedLoginCount++;
                else FailedLoginCount = 1;

                this.AddEvent(new InvalidPasswordEvent { Account = this });
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

        protected internal virtual bool Authenticate(X509Certificate2 certificate)
        {
            Tracing.Information("[UserAccount.Authenticate] certificate auth called for account ID: {0}", this.ID);
            
            certificate.Validate();

            Tracing.Verbose("[UserAccount.Authenticate] cert: {0}", certificate.Thumbprint);

            if (!(certificate.NotBefore < UtcNow && UtcNow < certificate.NotAfter))
            {
                Tracing.Error("[UserAccount.Authenticate] failed -- invalid certificate dates");
                this.AddEvent(new InvalidCertificateEvent { Account = this, Certificate = certificate });
                return false;
            }

            var match = this.Certificates.FirstOrDefault(x => x.Thumbprint.Equals(certificate.Thumbprint, StringComparison.OrdinalIgnoreCase));
            if (match == null)
            {
                Tracing.Error("[UserAccount.Authenticate] failed -- no certificate thumbprint match");
                this.AddEvent(new InvalidCertificateEvent { Account = this, Certificate = certificate });
                return false;
            }

            Tracing.Verbose("[UserAccount.Authenticate] success");

            this.LastLogin = UtcNow;
            this.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.None;

            this.AddEvent(new SuccessfulCertificateLoginEvent { Account = this, UserCertificate = match, Certificate = certificate });

            return true;
        }

        string IssueMobileCode()
        {
            string code = CryptoHelper.GenerateNumericCode(MembershipRebootConstants.UserAccount.MobileCodeLength);
            this.MobileCode = CryptoHelper.HashPassword(code);
            this.MobileCodeSent = UtcNow;
            return code;
        }

        bool VerifyMobileCode(string code)
        {
            if (IsMobileCodeStale)
            {
                Tracing.Error("[UserAccount.VerifyMobileCode] failed -- mobile code stale");
                return false;
            }

            var result = CryptoHelper.VerifyHashedPassword(this.MobileCode, code);
            if (!result)
            {
                Tracing.Error("[UserAccount.VerifyMobileCode] failed -- mobile code invalid");
                return false;
            }

            Tracing.Verbose("[UserAccount.VerifyMobileCode] success -- mobile code valid");
            return true;
        }

        void ClearMobileAuthCode()
        {
            this.MobileCode = null;
            this.MobileCodeSent = null;
        }

        protected virtual bool IsMobileCodeStale
        {
            get
            {
                if (this.MobileCodeSent == null || String.IsNullOrWhiteSpace(this.MobileCode))
                {
                    return true;
                }

                if (this.MobileCodeSent < UtcNow.AddMinutes(-MembershipRebootConstants.UserAccount.MobileCodeStaleDurationMinutes))
                {
                    return true;
                }

                return false;
            }
        }

        protected internal virtual void RequestChangeMobilePhoneNumber(string newMobilePhoneNumber)
        {
            Tracing.Information("[UserAccount.RequestChangeMobilePhoneNumber] called for accountID: {0}", this.ID);

            if (String.IsNullOrWhiteSpace(newMobilePhoneNumber))
            {
                Tracing.Error("[UserAccount.RequestChangeMobilePhoneNumber] invalid mobile phone");
                throw new ValidationException("Mobile Phone Number required.");
            }

            if (this.MobilePhoneNumber == newMobilePhoneNumber)
            {
                Tracing.Error("[UserAccount.RequestChangeMobilePhoneNumber] mobile phone same as current");
                throw new ValidationException("Mobile phone number must be different then the current.");
            }

            this.SetVerificationKey(VerificationKeyPurpose.ChangeMobile, state: newMobilePhoneNumber);
            var code = this.IssueMobileCode();

            Tracing.Verbose("[UserAccount.RequestChangeMobilePhoneNumber] success");

            this.AddEvent(new MobilePhoneChangeRequestedEvent { Account = this, NewMobilePhoneNumber = newMobilePhoneNumber, Code = code });
        }

        protected internal virtual bool ConfirmMobilePhoneNumberFromCode(string code)
        {
            Tracing.Information("[UserAccount.ConfirmMobilePhoneNumberFromCode] called for accountID: {0}", this.ID);
            
            if (String.IsNullOrWhiteSpace(code))
            {
                Tracing.Error("[UserAccount.ConfirmMobilePhoneNumberFromCode] failed -- no code");
                return false;
            }

            if (this.VerificationPurpose != VerificationKeyPurpose.ChangeMobile)
            {
                Tracing.Error("[UserAccount.ConfirmMobilePhoneNumberFromCode] failed -- invalid verification key purpose");
                return false;
            }

            if (!VerifyMobileCode(code))
            {
                Tracing.Error("[UserAccount.ConfirmMobilePhoneNumberFromCode] failed -- mobile code failed to verify");
                return false;
            }

            Tracing.Verbose("[UserAccount.ConfirmMobilePhoneNumberFromCode] success");
            
            this.MobilePhoneNumber = this.VerificationStorage;

            this.ClearVerificationKey();
            this.ClearMobileAuthCode();

            this.AddEvent(new MobilePhoneChangedEvent { Account = this });

            return true;
        }

        protected internal virtual void ClearMobilePhoneNumber()
        {
            Tracing.Information("[UserAccount.ClearMobilePhoneNumber] called for accountID: {0}", this.ID);

            if (this.AccountTwoFactorAuthMode == TwoFactorAuthMode.Mobile)
            {
                Tracing.Verbose("[UserAccount.ClearMobilePhoneNumber] disabling two factor auth");
                this.ConfigureTwoFactorAuthentication(TwoFactorAuthMode.None);
            }

            if (String.IsNullOrWhiteSpace(MobilePhoneNumber))
            {
                Tracing.Warning("[UserAccount.ClearMobilePhoneNumber] nothing to do -- no mobile associated with account");
                return;
            }

            Tracing.Verbose("[UserAccount.ClearMobilePhoneNumber] success");

            this.ClearMobileAuthCode();
            this.MobilePhoneNumber = null;

            this.AddEvent(new MobilePhoneRemovedEvent { Account = this });
        }

        protected internal virtual void ConfigureTwoFactorAuthentication(TwoFactorAuthMode mode)
        {
            Tracing.Information("[UserAccount.ConfigureTwoFactorAuthentication] called for accountID: {0}, mode: {1}", this.ID, mode);

            if (this.AccountTwoFactorAuthMode == mode)
            {
                Tracing.Warning("[UserAccount.ConfigureTwoFactorAuthentication] nothing to do -- mode is same as current value");
                return;
            }

            if (mode == TwoFactorAuthMode.Mobile &&
                String.IsNullOrWhiteSpace(this.MobilePhoneNumber))
            {
                Tracing.Error("[UserAccount.ConfigureTwoFactorAuthentication] failed -- mobile requested but no mobile phone for account");
                throw new ValidationException("Register a mobile phone number to enable mobile two factor authentication.");
            }

            if (mode == TwoFactorAuthMode.Certificate &&
                !this.Certificates.Any())
            {
                Tracing.Error("[UserAccount.ConfigureTwoFactorAuthentication] failed -- certificate requested but no certificates for account");
                throw new ValidationException("Add a client certificate to enable certificate two factor authentication.");
            }

            this.ClearMobileAuthCode();
            this.AccountTwoFactorAuthMode = mode;

            if (mode == TwoFactorAuthMode.None)
            {
                Tracing.Verbose("[UserAccount.ConfigureTwoFactorAuthentication] success -- two factor auth disabled");
                this.AddEvent(new TwoFactorAuthenticationDisabledEvent { Account = this });
            }
            else
            {
                Tracing.Verbose("[UserAccount.ConfigureTwoFactorAuthentication] success -- two factor auth enabled, mode: {0}", mode);
                this.AddEvent(new TwoFactorAuthenticationEnabledEvent { Account = this, Mode = mode });
            }
        }

        public bool RequiresTwoFactorAuthToSignIn
        {
            get
            {
                return this.CurrentTwoFactorAuthStatus != TwoFactorAuthMode.None;
            }
        }

        public bool RequiresTwoFactorCertificateToSignIn
        {
            get
            {
                return
                    this.AccountTwoFactorAuthMode == TwoFactorAuthMode.Certificate &&
                    this.CurrentTwoFactorAuthStatus == TwoFactorAuthMode.Certificate;
            }
        }

        protected internal virtual bool RequestTwoFactorAuthCertificate()
        {
            Tracing.Information("[UserAccount.RequestTwoFactorAuthCertificate] called for accountID: {0}", this.ID);

            if (!this.IsAccountVerified)
            {
                Tracing.Error("[UserAccount.RequestTwoFactorAuthCertificate] failed -- account not verified");
                return false;
            }

            if (this.IsAccountClosed)
            {
                Tracing.Error("[UserAccount.RequestTwoFactorAuthCertificate] failed -- account closed");
                return false;
            }

            if (!this.IsLoginAllowed)
            {
                Tracing.Error("[UserAccount.RequestTwoFactorAuthCertificate] failed -- login not allowed");
                return false;
            }

            if (this.AccountTwoFactorAuthMode != TwoFactorAuthMode.Certificate)
            {
                Tracing.Error("[UserAccount.RequestTwoFactorAuthCertificate] failed -- current auth mode is not certificate");
                return false;
            }

            if (!this.Certificates.Any())
            {
                Tracing.Error("[UserAccount.RequestTwoFactorAuthCertificate] failed -- no certificates");
                return false;
            }
            
            Tracing.Verbose("[UserAccount.RequestTwoFactorAuthCertificate] success");

            this.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.Certificate;

            return true;
        }

        public bool RequiresTwoFactorAuthCodeToSignIn
        {
            get
            {
                return
                    this.AccountTwoFactorAuthMode == TwoFactorAuthMode.Mobile &&
                    this.CurrentTwoFactorAuthStatus == TwoFactorAuthMode.Mobile;
            }
        }

        protected internal virtual bool RequestTwoFactorAuthCode()
        {
            Tracing.Information("[UserAccount.RequestTwoFactorAuthCode] called for accountID: {0}", this.ID);

            if (!this.IsAccountVerified)
            {
                Tracing.Error("[UserAccount.RequestTwoFactorAuthCode] failed -- account not verified");
                return false;
            }

            if (this.IsAccountClosed)
            {
                Tracing.Error("[UserAccount.RequestTwoFactorAuthCode] failed -- account closed");
                return false;
            }

            if (!this.IsLoginAllowed)
            {
                Tracing.Error("[UserAccount.RequestTwoFactorAuthCode] failed -- login not allowed");
                return false;
            } 
            
            if (this.AccountTwoFactorAuthMode != TwoFactorAuthMode.Mobile)
            {
                Tracing.Error("[UserAccount.RequestTwoFactorAuthCode] failed -- AccountTwoFactorAuthMode not mobile");
                return false;
            }

            if (String.IsNullOrWhiteSpace(MobilePhoneNumber))
            {
                Tracing.Error("[UserAccount.RequestTwoFactorAuthCode] failed -- empty MobilePhoneNumber");
                return false;
            }

            Tracing.Verbose("[UserAccount.RequestTwoFactorAuthCode] new mobile code issued");
            var code = this.IssueMobileCode();

            Tracing.Verbose("[UserAccount.RequestTwoFactorAuthCode] success");

            this.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.Mobile;

            this.AddEvent(new TwoFactorAuthenticationCodeNotificationEvent { Account = this, Code = code });

            return true;
        }

        protected internal virtual bool VerifyTwoFactorAuthCode(string code)
        {
            Tracing.Information("[UserAccount.VerifyTwoFactorAuthCode] called for accountID: {0}", this.ID);

            if (code == null)
            {
                Tracing.Error("[UserAccount.VerifyTwoFactorAuthCode] failed - null code");
                return false;
            }

            if (!this.IsAccountVerified)
            {
                Tracing.Error("[UserAccount.VerifyTwoFactorAuthCode] failed -- account not verified");
                return false;
            }

            if (this.IsAccountClosed)
            {
                Tracing.Error("[UserAccount.VerifyTwoFactorAuthCode] failed -- account closed");
                return false;
            }

            if (!this.IsLoginAllowed)
            {
                Tracing.Error("[UserAccount.VerifyTwoFactorAuthCode] failed -- login not allowed");
                return false;
            }
            
            if (this.AccountTwoFactorAuthMode != TwoFactorAuthMode.Mobile)
            {
                Tracing.Error("[UserAccount.VerifyTwoFactorAuthCode] failed -- two factor auth mode not mobile");
                return false;
            }

            if (this.CurrentTwoFactorAuthStatus != TwoFactorAuthMode.Mobile)
            {
                Tracing.Error("[UserAccount.VerifyTwoFactorAuthCode] failed -- current auth status not mobile");
                return false;
            }

            if (!VerifyMobileCode(code))
            {
                Tracing.Error("[UserAccount.VerifyTwoFactorAuthCode] failed -- mobile code failed to verify");
                return false;
            }

            Tracing.Verbose("[UserAccount.VerifyTwoFactorAuthCode] success");

            this.LastLogin = UtcNow;
            this.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.None;
            this.ClearMobileAuthCode();

            this.AddEvent(new SuccessfulTwoFactorAuthCodeLoginEvent { Account = this });

            return true;
        }

        protected internal virtual void SendAccountNameReminder()
        {
            Tracing.Information("[UserAccount.SendAccountNameReminder] called for accountID: {0}", this.ID);

            this.AddEvent(new UsernameReminderRequestedEvent { Account = this });
        }

        protected internal virtual void ChangeUsername(string newUsername)
        {
            Tracing.Information("[UserAccount.ChangeUsername] called for accountID: {0}", this.ID);

            if (String.IsNullOrWhiteSpace(newUsername))
            {
                Tracing.Error("[UserAccount.ChangeUsername] failed -- invalid newUsername");
                throw new ArgumentNullException(newUsername);
            }
            
            Tracing.Verbose("[UserAccount.ChangeUsername] success");

            this.Username = newUsername;

            this.AddEvent(new UsernameChangedEvent { Account = this });
        }

        protected internal virtual void ChangeEmailRequest(string newEmail)
        {
            Tracing.Information("[UserAccount.ChangeEmailRequest] called for accountID: {0}", this.ID);

            if (String.IsNullOrWhiteSpace(newEmail))
            {
                Tracing.Error("[UserAccount.ChangeEmailRequest] failed -- invalid newEmail");
                throw new ValidationException("Invalid email.");
            }

            // if they've not yet verified then fail
            if (!this.IsAccountVerified)
            {
                Tracing.Error("[UserAccount.ChangeEmailRequest] failed -- account not verified");
                throw new Exception("Account not verified");
            }

            Tracing.Verbose("[UserAccount.ChangeEmailRequest] creating a new reset key");
            var key = this.SetVerificationKey(VerificationKeyPurpose.ChangeEmail, state:newEmail);

            Tracing.Verbose("[UserAccount.ChangeEmailRequest] success");
            
            this.AddEvent(new EmailChangeRequestedEvent { Account = this, NewEmail = newEmail, VerificationKey = key });
        }

        protected internal virtual bool ChangeEmailFromKey(string key)
        {
            Tracing.Information("[UserAccount.ChangeEmailFromKey] called for accountID: {0}", this.ID);

            if (String.IsNullOrWhiteSpace(key))
            {
                Tracing.Error("[UserAccount.ChangeEmailFromKey] failed -- invalid key");
                throw new ValidationException("Invalid key.");
            }

            if (!IsVerificationKeyValid(VerificationKeyPurpose.ChangeEmail, key))
            {
                Tracing.Error("[UserAccount.ChangeEmailFromKey] failed -- key verification failed");
                return false;
            }
            
            if (String.IsNullOrWhiteSpace(this.VerificationStorage))
            {
                Tracing.Verbose("[UserAccount.ChangeEmailFromKey] failed -- verification storage empty");
                return false;
            }
            
            Tracing.Verbose("[UserAccount.ChangeEmailFromKey] success");

            var oldEmail = this.Email;
            this.Email = this.VerificationStorage;
            
            this.ClearVerificationKey();

            this.AddEvent(new EmailChangedEvent { Account = this, OldEmail = oldEmail });

            return true;
        }

        protected internal virtual void CloseAccount()
        {
            Tracing.Information("[UserAccount.CloseAccount] called for accountID: {0}", this.ID);

            this.ClearVerificationKey();
            this.ClearMobileAuthCode();

            IsLoginAllowed = false;
            CurrentTwoFactorAuthStatus = TwoFactorAuthMode.None;

            if (!IsAccountClosed)
            {
                Tracing.Verbose("[UserAccount.CloseAccount] success");

                IsAccountClosed = true;
                AccountClosed = UtcNow;

                this.AddEvent(new AccountClosedEvent { Account = this });
            }
            else
            {
                Tracing.Warning("[UserAccount.CloseAccount] account already closed");
            }
        }

        protected internal virtual bool GetIsPasswordExpired(int passwordResetFrequency)
        {
            if (this.RequiresPasswordReset) return true;

            if (passwordResetFrequency <= 0) return false;

            var now = this.UtcNow;
            var last = this.PasswordChanged;
            return last.AddDays(passwordResetFrequency) <= now;
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
            Tracing.Information("[UserAccount.AddClaim] called for accountID: {0}", this.ID);

            if (String.IsNullOrWhiteSpace(type))
            {
                Tracing.Error("[UserAccount.AddClaim] failed -- null type");
                throw new ArgumentException("type");
            }
            if (String.IsNullOrWhiteSpace(value))
            {
                Tracing.Error("[UserAccount.AddClaim] failed -- null value");
                throw new ArgumentException("value");
            }

            if (!this.HasClaim(type, value))
            {
                var claim = new UserClaim
                {
                    Type = type,
                    Value = value
                };
                this.Claims.Add(claim);
                this.AddEvent(new ClaimAddedEvent { Account = this, Claim = claim });
            }
        }

        public virtual void RemoveClaim(string type)
        {
            Tracing.Information("[UserAccount.RemoveClaim] called for accountID: {0}", this.ID);

            if (String.IsNullOrWhiteSpace(type))
            {
                Tracing.Error("[UserAccount.RemoveClaim] failed -- null type");
                throw new ArgumentException("type");
            }

            var claimsToRemove =
                from claim in this.Claims
                where claim.Type == type
                select claim;
            foreach (var claim in claimsToRemove.ToArray())
            {
                this.Claims.Remove(claim);
                this.AddEvent(new ClaimRemovedEvent { Account = this, Claim = claim });
            }
        }

        public virtual void RemoveClaim(string type, string value)
        {
            Tracing.Information("[UserAccount.RemoveClaim] called for accountID: {0}", this.ID);

            if (String.IsNullOrWhiteSpace(type))
            {
                Tracing.Error("[UserAccount.RemoveClaim] failed -- null type");
                throw new ArgumentException("type");
            }
            if (String.IsNullOrWhiteSpace(value))
            {
                Tracing.Error("[UserAccount.RemoveClaim] failed -- null value");
                throw new ArgumentException("value");
            }

            var claimsToRemove =
                from claim in this.Claims
                where claim.Type == type && claim.Value == value
                select claim;
            foreach (var claim in claimsToRemove.ToArray())
            {
                this.Claims.Remove(claim);
                this.AddEvent(new ClaimRemovedEvent { Account = this, Claim = claim });
            }
        }

        public virtual LinkedAccount GetLinkedAccount(string provider, string id)
        {
            return this.LinkedAccounts.Where(x => x.ProviderName == provider && x.ProviderAccountID == id).SingleOrDefault();
        }

        public virtual void AddOrUpdateLinkedAccount(string provider, string id, IEnumerable<Claim> claims = null)
        {
            Tracing.Information("[UserAccount.AddOrUpdateLinkedAccount] called for accountID: {0}", this.ID);

            if (String.IsNullOrWhiteSpace(provider))
            {
                Tracing.Error("[UserAccount.AddOrUpdateLinkedAccount] failed -- null provider");
                throw new ArgumentNullException("provider");
            }
            if (String.IsNullOrWhiteSpace(id))
            {
                Tracing.Error("[UserAccount.AddOrUpdateLinkedAccount] failed -- null id");
                throw new ArgumentNullException("id");
            }

            var linked = GetLinkedAccount(provider, id);
            if (linked == null)
            {
                linked = new LinkedAccount
                {
                    ProviderName = provider,
                    ProviderAccountID = id
                };
                this.LinkedAccounts.Add(linked);
                this.AddEvent(new LinkedAccountAddedEvent { Account = this, LinkedAccount = linked });

                Tracing.Verbose("[UserAccount.AddOrUpdateLinkedAccount] linked account added");
            }
            UpdateLinkedAccount(linked, claims);
        }

        protected virtual void UpdateLinkedAccount(LinkedAccount account, IEnumerable<Claim> claims = null)
        {
            Tracing.Information("[UserAccount.UpdateLinkedAccount] called for accountID: {0}", this.ID);

            if (account == null)
            {
                Tracing.Error("[UserAccount.UpdateLinkedAccount] failed -- null account");
                throw new ArgumentNullException("account");
            }

            account.LastLogin = UtcNow;
            account.UpdateClaims(claims);
        }

        public virtual void RemoveLinkedAccount(string provider)
        {
            Tracing.Information("[UserAccount.RemoveLinkedAccount] called for accountID: {0}", this.ID);

            var linked = this.LinkedAccounts.Where(x => x.ProviderName == provider);
            foreach(var item in linked)
            {
                this.LinkedAccounts.Remove(item);
                this.AddEvent(new LinkedAccountRemovedEvent { Account = this, LinkedAccount = item });
            }
        }

        public virtual void RemoveLinkedAccount(string provider, string id)
        {
            Tracing.Information("[UserAccount.RemoveLinkedAccount] called for accountID: {0}", this.ID);

            var linked = GetLinkedAccount(provider, id);
            if (linked != null)
            {
                this.LinkedAccounts.Remove(linked);
                this.AddEvent(new LinkedAccountRemovedEvent { Account = this, LinkedAccount = linked });
            }
        }

        public virtual void AddCertificate(X509Certificate2 certificate)
        {
            Tracing.Information("[UserAccount.AddCertificate] called for accountID: {0}", this.ID);

            certificate.Validate();
            RemoveCertificate(certificate);
            AddCertificate(certificate.Thumbprint, certificate.Subject);
        }
        public virtual void AddCertificate(string thumbprint, string subject)
        {
            Tracing.Information("[UserAccount.AddCertificate] called for accountID: {0}", this.ID);

            if (String.IsNullOrWhiteSpace(thumbprint))
            {
                Tracing.Error("[UserAccount.AddCertificate] failed -- null thumbprint");
                throw new ArgumentNullException("thumbprint");
            }
            if (String.IsNullOrWhiteSpace(subject))
            {
                Tracing.Error("[UserAccount.AddCertificate] failed -- null subject");
                throw new ArgumentNullException("subject");
            }

            var cert = new UserCertificate { User = this, Thumbprint = thumbprint, Subject = subject };
            this.Certificates.Add(cert);
            
            this.AddEvent(new CertificateAddedEvent { Account = this, Certificate = cert });
        }

        public virtual void RemoveCertificate(X509Certificate2 certificate)
        {
            Tracing.Information("[UserAccount.RemoveCertificate] called for accountID: {0}", this.ID);

            if (certificate == null)
            {
                Tracing.Error("[UserAccount.RemoveCertificate] failed -- null certificate");
                throw new ArgumentNullException("certificate");
            }
            if (certificate.Handle == IntPtr.Zero)
            {
                Tracing.Error("[UserAccount.RemoveCertificate] failed -- invalid certificate handle");
                throw new ArgumentException("Invalid certificate");
            }

            RemoveCertificate(certificate.Thumbprint);
        }
        public virtual void RemoveCertificate(string thumbprint)
        {
            Tracing.Information("[UserAccount.RemoveCertificate] called for accountID: {0}", this.ID);

            if (String.IsNullOrWhiteSpace(thumbprint))
            {
                Tracing.Error("[UserAccount.RemoveCertificate] failed -- no thumbprint");
                throw new ArgumentNullException("thumbprint");
            }

            var certs = this.Certificates.Where(x => x.Thumbprint.Equals(thumbprint, StringComparison.OrdinalIgnoreCase)).ToArray();
            foreach (var cert in certs)
            {
                this.AddEvent(new CertificateRemovedEvent { Account = this, Certificate = cert });
                this.Certificates.Remove(cert);
            }

            if (!this.Certificates.Any() &&
                this.AccountTwoFactorAuthMode == TwoFactorAuthMode.Certificate)
            {
                Tracing.Verbose("[UserAccount.RemoveCertificate] last cert removed, disabling two factor auth");
                this.ConfigureTwoFactorAuthentication(TwoFactorAuthMode.None);
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

        protected internal bool SlowEquals(string a, string b)
        {
            return CryptoHelper.SlowEquals(a, b);
        }

        protected internal virtual DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }

        internal string CreateTwoFactorAuthToken()
        {
            var value = CryptoHelper.GenerateSalt();

            var item = new TwoFactorAuthToken
            {
                Token = CryptoHelper.Hash(value),
                Issued = this.UtcNow
            };
            this.TwoFactorAuthTokens.Add(item);
            
            return value;
        }

        internal bool VerifyTwoFactorAuthToken(string token)
        {
            if (String.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            token = CryptoHelper.Hash(token);

            var expiration = UtcNow.AddDays(-MembershipRebootConstants.UserAccount.TwoFactorAuthTokenDurationDays);
            var query = 
                from t in this.TwoFactorAuthTokens
                where 
                    t.Issued < this.PasswordChanged || 
                    t.Issued < expiration
                select t;
            foreach(var item in query.ToArray())
            {
                this.TwoFactorAuthTokens.Remove(item);
            }

            query = 
                from t in this.TwoFactorAuthTokens.ToArray()
                where SlowEquals(t.Token, token)
                select t;
            
            return query.Any();
        }
    }
}
