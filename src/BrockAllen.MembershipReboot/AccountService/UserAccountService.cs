/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace BrockAllen.MembershipReboot
{
    public class UserAccountService : IDisposable
    {
        public MembershipRebootConfiguration Configuration { get; set; }
        
        SecuritySettings SecuritySettings
        {
            get
            {
                return this.Configuration.SecuritySettings;
            }
        }

        IUserAccountRepository userRepository;

        Lazy<AggregateValidator> usernameValidator;
        Lazy<AggregateValidator> emailValidator;
        Lazy<AggregateValidator> passwordValidator;

        public UserAccountService(IUserAccountRepository userRepository)
            : this(new MembershipRebootConfiguration(), userRepository)
        {
        }

        public UserAccountService(MembershipRebootConfiguration configuration, IUserAccountRepository userRepository)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (userRepository == null) throw new ArgumentNullException("userRepository");
            
            this.Configuration = configuration;

            var validationEventBus = new EventBus();
            validationEventBus.Add(new UserAccountValidator(this));
            this.userRepository = new EventBusUserAccountRepository(userRepository,
                new AggregateEventBus { validationEventBus, configuration.ValidationBus },
                configuration.EventBus);

            this.usernameValidator = new Lazy<AggregateValidator>(()=>
            {
                var val = new AggregateValidator();
                if (!this.SecuritySettings.EmailIsUsername)
                {
                    val.Add(UserAccountValidation.UsernameDoesNotContainAtSign);
                }
                val.Add(UserAccountValidation.UsernameMustNotAlreadyExist);
                val.Add(configuration.UsernameValidator);
                return val;
            });

            this.emailValidator = new Lazy<AggregateValidator>(() =>
            {
                var val = new AggregateValidator();
                val.Add(UserAccountValidation.EmailIsValidFormat);
                val.Add(UserAccountValidation.EmailMustNotAlreadyExist);
                val.Add(configuration.EmailValidator);
                return val;
            });

            this.passwordValidator = new Lazy<AggregateValidator>(() =>
            {
                var val = new AggregateValidator();
                val.Add(UserAccountValidation.PasswordMustBeDifferentThanCurrent);
                val.Add(configuration.PasswordValidator);
                return val;
            });
        }

        internal protected void ValidateUsername(UserAccount account, string value)
        {
            var result = this.usernameValidator.Value.Validate(this, account, value);
            if (result != null && result != ValidationResult.Success)
            {
                Tracing.Error("ValidateUsername failed: " + result.ErrorMessage);
                throw new ValidationException(result.ErrorMessage);
            }
        }
        internal protected void ValidatePassword(UserAccount account, string value)
        {
            var result = this.passwordValidator.Value.Validate(this, account, value);
            if (result != null && result != ValidationResult.Success)
            {
                Tracing.Error("ValidatePassword failed: " + result.ErrorMessage);
                throw new ValidationException(result.ErrorMessage);
            }
        }
        internal protected void ValidateEmail(UserAccount account, string value)
        {
            var result = this.emailValidator.Value.Validate(this, account, value);
            if (result != null && result != ValidationResult.Success)
            {
                Tracing.Error("ValidateEmail failed: " + result.ErrorMessage);
                throw new ValidationException(result.ErrorMessage);
            }
        }

        public void Dispose()
        {
            if (this.userRepository.TryDispose())
            {
                this.userRepository = null;
            }
        }

        public virtual void Update(UserAccount account)
        {
            if (account == null)
            {
                Tracing.Error("[UserAccountService.Update] called -- failed null account");
                throw new ArgumentNullException("account");
            }

            Tracing.Information("[UserAccountService.Update] called for account: {0}", account.ID);

            account.LastUpdated = account.UtcNow;
            this.userRepository.Update(account);
        }
        
        public virtual IQueryable<UserAccount> GetAll()
        {
            return GetAll(null);
        }

        public virtual IQueryable<UserAccount> GetAll(string tenant)
        {
            if (!SecuritySettings.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.GetAll] applying default tenant");
                tenant = SecuritySettings.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) return Enumerable.Empty<UserAccount>().AsQueryable();

            return this.userRepository.GetAll().Where(x => x.Tenant == tenant && x.IsAccountClosed == false);
        }

        public virtual UserAccount GetByUsername(string username)
        {
            return GetByUsername(null, username);
        }

        public virtual UserAccount GetByUsername(string tenant, string username)
        {
            if (!SecuritySettings.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.GetByUsername] applying default tenant");
                tenant = SecuritySettings.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) return null;
            if (String.IsNullOrWhiteSpace(username)) return null;

            var account = userRepository.GetAll().Where(x => x.Tenant == tenant && x.Username == username).SingleOrDefault();
            if (account == null)
            {
                Tracing.Warning("[UserAccountService.GetByUsername] failed to locate account: {0}, {1}", tenant, username);
            }
            return account;
        }

        public virtual UserAccount GetByEmail(string email)
        {
            return GetByEmail(null, email);
        }

        public virtual UserAccount GetByEmail(string tenant, string email)
        {
            if (!SecuritySettings.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.GetByEmail] applying default tenant");
                tenant = SecuritySettings.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) return null;
            if (String.IsNullOrWhiteSpace(email)) return null;

            var account = userRepository.GetAll().Where(x => x.Tenant == tenant && x.Email == email).SingleOrDefault();
            if (account == null)
            {
                Tracing.Warning("[UserAccountService.GetByEmail] failed to locate account: {0}, {1}", tenant, email);
            }
            return account;
        }

        public virtual UserAccount GetByID(Guid id)
        {
            var account = this.userRepository.Get(id);
            if (account == null)
            {
                Tracing.Warning("[UserAccountService.GetByID] failed to locate account: {0}", id);
            }
            return account;
        }

        public virtual UserAccount GetByVerificationKey(string key)
        {
            if (String.IsNullOrWhiteSpace(key)) return null;

            var account = userRepository.GetAll().Where(x => x.VerificationKey == key).SingleOrDefault();
            if (account == null)
            {
                Tracing.Warning("[UserAccountService.GetByVerificationKey] failed to locate account: {0}", key);
            }
            return account;
        }

        public virtual UserAccount GetByLinkedAccount(string provider, string id)
        {
            return GetByLinkedAccount(null, provider, id);
        }

        public virtual UserAccount GetByLinkedAccount(string tenant, string provider, string id)
        {
            if (!SecuritySettings.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.GetByLinkedAccount] applying default tenant");
                tenant = SecuritySettings.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) return null;
            if (String.IsNullOrWhiteSpace(provider)) return null;
            if (String.IsNullOrWhiteSpace(id)) return null;

            var query =
                from u in userRepository.GetAll()
                where u.Tenant == tenant
                from l in u.LinkedAccounts
                where l.ProviderName == provider && l.ProviderAccountID == id
                select u;

            var account = query.SingleOrDefault();
            if (account == null)
            {
                Tracing.Warning("[UserAccountService.GetByLinkedAccount] failed to locate by tenant: {0}, provider: {1}, id: {2}", tenant, provider, id);
            }
            return account;
        }

        public virtual UserAccount GetByCertificate(string thumbprint)
        {
            return GetByCertificate(null, thumbprint);
        }

        public virtual UserAccount GetByCertificate(string tenant, string thumbprint)
        {
            if (!SecuritySettings.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.GetByCertificate] applying default tenant");
                tenant = SecuritySettings.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) return null;
            if (String.IsNullOrWhiteSpace(thumbprint)) return null;

            var query =
                from u in userRepository.GetAll()
                where u.Tenant == tenant
                from c in u.Certificates
                where c.Thumbprint == thumbprint
                select u;

            var account = query.SingleOrDefault();
            if (account == null)
            {
                Tracing.Warning("[UserAccountService.GetByCertificate] failed to locate by certificate thumbprint: {0}, {1}", tenant, thumbprint);
            }
            return account;
        }

        public virtual bool UsernameExists(string username)
        {
            return UsernameExists(null, username);
        }

        public virtual bool UsernameExists(string tenant, string username)
        {
            if (String.IsNullOrWhiteSpace(username)) return false;

            if (SecuritySettings.UsernamesUniqueAcrossTenants)
            {
                return this.userRepository.GetAll().Where(x => x.Username == username).Any();
            }
            else
            {
                if (!SecuritySettings.MultiTenant)
                {
                    Tracing.Verbose("[UserAccountService.UsernameExists] applying default tenant");
                    tenant = SecuritySettings.DefaultTenant;
                }

                if (String.IsNullOrWhiteSpace(tenant)) return false;

                return this.userRepository.GetAll().Where(x => x.Tenant == tenant && x.Username == username).Any();
            }
        }

        public virtual bool EmailExists(string email)
        {
            return EmailExists(null, email);
        }

        public virtual bool EmailExists(string tenant, string email)
        {
            if (!SecuritySettings.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.EmailExists] applying default tenant");
                tenant = SecuritySettings.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) return false;
            if (String.IsNullOrWhiteSpace(email)) return false;

            return this.userRepository.GetAll().Where(x => x.Tenant == tenant && x.Email == email).Any();
        }

        public virtual UserAccount CreateAccount(string username, string password, string email)
        {
            return CreateAccount(null, username, password, email);
        }

        public virtual UserAccount CreateAccount(string tenant, string username, string password, string email)
        {
            if (SecuritySettings.EmailIsUsername)
            {
                Tracing.Verbose("[UserAccountService.CreateAccount] applying email is username");
                username = email;
            }

            if (!SecuritySettings.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.CreateAccount] applying default tenant");
                tenant = SecuritySettings.DefaultTenant;
            }

            Tracing.Information("[UserAccountService.CreateAccount] called: {0}, {1}, {2}", tenant, username, email);

            var account = this.userRepository.Create();
            account.Init(tenant, username, password, email);
            
            ValidateEmail(account, email);
            ValidateUsername(account, username);
            ValidatePassword(account, password);

            Tracing.Verbose("[UserAccountService.CreateAccount] SecuritySettings.AllowLoginAfterAccountCreation is set to: {0}", SecuritySettings.AllowLoginAfterAccountCreation);
            account.IsLoginAllowed = SecuritySettings.AllowLoginAfterAccountCreation;
            
            if (!SecuritySettings.RequireAccountVerification)
            {
                Tracing.Verbose("[UserAccountService.CreateAccount] SecuritySettings.RequireAccountVerification is false, so marking account as verified");
                account.VerifyAccount(account.VerificationKey);
            }

            Tracing.Verbose("[UserAccountService.CreateAccount] success");

            this.userRepository.Add(account);
            
            return account;
        }

        public virtual bool VerifyAccount(string key)
        {
            Tracing.Information("[UserAccountService.VerifyAccount] called: {0}", key);

            var account = this.GetByVerificationKey(key);
            if (account == null) return false;

            var result = account.VerifyAccount(key);
            Update(account);

            Tracing.Verbose("[UserAccountService.VerifyAccount] result: {0}", result);

            return result;
        }

        public virtual bool CancelNewAccount(string key)
        {
            Tracing.Information("[UserAccountService.CancelNewAccount] called: {0}", key);

            var account = this.GetByVerificationKey(key);
            if (account == null) return false;

            if (account.CancelNewAccount(key))
            {
                Tracing.Verbose("[UserAccountService.CancelNewAccount] account cancelled");
                DeleteAccount(account);
                return true;
            }

            Tracing.Verbose("[UserAccountService.CancelNewAccount] account not cancelled");
            return false;
        }

        public virtual void DeleteAccount(Guid accountID)
        {
            Tracing.Information("[UserAccountService.DeleteAccount] called: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            DeleteAccount(account);
        }

        protected internal virtual void DeleteAccount(UserAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Verbose("[UserAccountService.DeleteAccount] marking account as closed: {0}", account.ID);

            account.CloseAccount();
            Update(account);

            if (SecuritySettings.AllowAccountDeletion || !account.IsAccountVerified)
            {
                Tracing.Verbose("[UserAccountService.DeleteAccount] removing account record: {0}", account.ID);
                this.userRepository.Remove(account);
            }
        }

        public virtual bool Authenticate(string username, string password)
        {
            return Authenticate(null, username, password);
        }
        public virtual bool Authenticate(string username, string password, out UserAccount account)
        {
            return Authenticate(null, username, password, out account);
        }
        
        public virtual bool Authenticate(string tenant, string username, string password)
        {
            UserAccount account;
            return Authenticate(tenant, username, password, out account);
        }
        public virtual bool Authenticate(string tenant, string username, string password, out UserAccount account)
        {
            account = null;

            if (!SecuritySettings.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.Authenticate] applying default tenant");
                tenant = SecuritySettings.DefaultTenant;
            }

            Tracing.Information("[UserAccountService.Authenticate] called: {0}, {1}", tenant, username);

            if (String.IsNullOrWhiteSpace(tenant)) return false;
            if (String.IsNullOrWhiteSpace(username)) return false;
            if (String.IsNullOrWhiteSpace(password)) return false;

            account = this.GetByUsername(tenant, username);
            if (account == null) return false;

            return Authenticate(account, password, AuthenticationPurpose.SignIn);
        }

        public virtual bool AuthenticateWithEmail(string email, string password)
        {
            return AuthenticateWithEmail(null, email, password);
        }
        public virtual bool AuthenticateWithEmail(string email, string password, out UserAccount account)
        {
            return AuthenticateWithEmail(null, email, password, out account);
        }

        public virtual bool AuthenticateWithEmail(string tenant, string email, string password)
        {
            UserAccount account;
            return AuthenticateWithEmail(null, email, password, out account);
        }
        public virtual bool AuthenticateWithEmail(string tenant, string email, string password, out UserAccount account)
        {
            account = null;

            if (!SecuritySettings.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.AuthenticateWithEmail] applying default tenant");
                tenant = SecuritySettings.DefaultTenant;
            }

            Tracing.Information("[UserAccountService.AuthenticateWithEmail] called: {0}, {1}", tenant, email);

            if (String.IsNullOrWhiteSpace(tenant)) return false;
            if (String.IsNullOrWhiteSpace(email)) return false;
            if (String.IsNullOrWhiteSpace(password)) return false;

            account = this.GetByEmail(tenant, email);
            if (account == null) return false;

            return Authenticate(account, password, AuthenticationPurpose.SignIn);
        }

        public virtual bool AuthenticateWithUsernameOrEmail(string userNameOrEmail, string password, out UserAccount account)
        {
            return AuthenticateWithUsernameOrEmail(null, userNameOrEmail, password, out account);
        }

        public virtual bool AuthenticateWithUsernameOrEmail(string tenant, string userNameOrEmail, string password, out UserAccount account)
        {
            account = null;

            if (!SecuritySettings.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.AuthenticateWithUsernameOrEmail] applying default tenant");
                tenant = SecuritySettings.DefaultTenant;
            }

            Tracing.Information("[UserAccountService.AuthenticateWithUsernameOrEmail] called {0}, {1}", tenant, userNameOrEmail);

            if (String.IsNullOrWhiteSpace(tenant)) return false;
            if (String.IsNullOrWhiteSpace(userNameOrEmail)) return false;
            if (String.IsNullOrWhiteSpace(password)) return false;

            if (!SecuritySettings.EmailIsUsername && userNameOrEmail.Contains("@"))
            {
                Tracing.Verbose("[UserAccountService.AuthenticateWithUsernameOrEmail] email detected");
                return AuthenticateWithEmail(tenant, userNameOrEmail, password, out account);
            }
            else
            {
                Tracing.Verbose("[UserAccountService.AuthenticateWithUsernameOrEmail] username detected");
                return Authenticate(tenant, userNameOrEmail, password, out account);
            }
        }
        
        protected internal virtual bool Authenticate(UserAccount account, string password, AuthenticationPurpose purpose)
        {
            Tracing.Verbose("[UserAccountService.Authenticate] for account: {0}", account.ID);
            
            int failedLoginCount = SecuritySettings.AccountLockoutFailedLoginAttempts;
            TimeSpan lockoutDuration = SecuritySettings.AccountLockoutDuration;

            var result = account.Authenticate(password, failedLoginCount, lockoutDuration);
            if (result && 
                purpose == AuthenticationPurpose.SignIn && 
                account.AccountTwoFactorAuthMode != TwoFactorAuthMode.None)
            {
                Tracing.Verbose("[UserAccountService.Authenticate] password authN successful, doing two factor auth checks: {0}, {1}", account.Tenant, account.Username);
                
                bool shouldRequestTwoFactorAuthCode = true;
                if (this.Configuration.TwoFactorAuthenticationPolicy != null)
                {
                    shouldRequestTwoFactorAuthCode = this.Configuration.TwoFactorAuthenticationPolicy.RequestRequiresTwoFactorAuth(account);
                    Tracing.Verbose("[UserAccountService.Authenticate] TwoFactorAuthenticationPolicy.RequestRequiresTwoFactorAuth called, result: {0}", shouldRequestTwoFactorAuthCode);
                }

                if (shouldRequestTwoFactorAuthCode)
                {
                    if (account.AccountTwoFactorAuthMode == TwoFactorAuthMode.Certificate)
                    {
                        Tracing.Verbose("[UserAccountService.Authenticate] requesting 2fa certificate: {0}, {1}", account.Tenant, account.Username);
                        result = account.RequestTwoFactorAuthCertificate();
                    }

                    if (account.AccountTwoFactorAuthMode == TwoFactorAuthMode.Mobile)
                    {
                        Tracing.Verbose("[UserAccountService.Authenticate] requesting 2fa mobile code: {0}, {1}", account.Tenant, account.Username);
                        result = account.RequestTwoFactorAuthCode();
                    }
                }
            }

            Update(account);

            Tracing.Verbose("[UserAccountService.Authenticate] authentication outcome: {0}", result ? "Successful Login" : "Failed Login");

            return result;
        }

        public virtual bool AuthenticateWithCode(Guid accountID, string code)
        {
            UserAccount account;
            return AuthenticateWithCode(accountID, code, out account);
        }

        public virtual bool AuthenticateWithCode(Guid accountID, string code, out UserAccount account)
        {
            Tracing.Information("[UserAccountService.AuthenticateWithCode] called {0}", accountID);

            account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            var result = account.VerifyTwoFactorAuthCode(code);
            Update(account);

            Tracing.Verbose("[UserAccountService.AuthenticateWithCode] result {0}", result);

            return result;
        }

        public virtual bool AuthenticateWithCertificate(X509Certificate2 certificate)
        {
            UserAccount account;
            return AuthenticateWithCertificate(certificate, out account);
        }

        public virtual bool AuthenticateWithCertificate(X509Certificate2 certificate, out UserAccount account)
        {
            Tracing.Information("[UserAccountService.AuthenticateWithCertificate] called");

            certificate.Validate();

            account = this.GetByCertificate(certificate.Thumbprint);
            if (account == null) return false;

            var result = account.Authenticate(certificate);
            Update(account);

            Tracing.Verbose("[UserAccountService.AuthenticateWithCertificate] result {0}", result);

            return result;
        }

        public virtual bool AuthenticateWithCertificate(Guid accountID, X509Certificate2 certificate)
        {
            UserAccount account;
            return AuthenticateWithCertificate(accountID, certificate, out account);
        }

        public virtual bool AuthenticateWithCertificate(Guid accountID, X509Certificate2 certificate, out UserAccount account)
        {
            Tracing.Information("[UserAccountService.AuthenticateWithCertificate] called for userID: {0}", accountID);
            
            certificate.Validate();

            account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            var result = account.Authenticate(certificate);
            Update(account);

            Tracing.Verbose("[UserAccountService.AuthenticateWithCertificate] result: {0}", result);
            
            return result;
        }
        
        public virtual void ConfigureTwoFactorAuthentication(Guid accountID, TwoFactorAuthMode mode)
        {
            Tracing.Information("[UserAccountService.ConfigureTwoFactorAuthentication] called: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            account.ConfigureTwoFactorAuthentication(mode);
            Update(account);
        }

        public virtual void SendTwoFactorAuthenticationCode(Guid accountID)
        {
            Tracing.Information("[UserAccountService.SendTwoFactorAuthenticationCode] called: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");
            
            account.RequestTwoFactorAuthCode();
            Update(account);
        }

        public virtual void SetPassword(Guid accountID, string newPassword)
        {
            Tracing.Information("[UserAccountService.SetPassword] called: {0}", accountID);

            if (String.IsNullOrWhiteSpace(newPassword))
            {
                Tracing.Error("[UserAccountService.SetPassword] failed -- null newPassword");
                throw new ValidationException("Invalid new password.");
            }

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            ValidatePassword(account, newPassword);

            account.SetPassword(newPassword);
            Update(account);
        }

        public virtual void ChangePassword(Guid accountID, string oldPassword, string newPassword)
        {
            Tracing.Information("[UserAccountService.ChangePassword] called: {0}", accountID);

            if (String.IsNullOrWhiteSpace(oldPassword))
            {
                Tracing.Error("[UserAccountService.ChangePassword] failed -- null oldPassword");
                throw new ValidationException("Invalid old password.");
            }
            if (String.IsNullOrWhiteSpace(newPassword))
            {
                Tracing.Error("[UserAccountService.ChangePassword] failed -- null newPassword");
                throw new ValidationException("Invalid new password.");
            }

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            ValidatePassword(account, newPassword);

            if (!Authenticate(account, oldPassword, AuthenticationPurpose.VerifyPassword))
            {
                Tracing.Error("[UserAccountService.ChangePassword] failed -- failed authN");
                throw new ValidationException("Invalid old password.");
            }

            account.SetPassword(newPassword);
            Update(account);
        }

        public virtual void ResetPassword(string email)
        {
            ResetPassword(null, email);
        }

        public virtual void ResetPassword(string tenant, string email)
        {
            if (!SecuritySettings.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.ResetPassword] applying default tenant");
                tenant = SecuritySettings.DefaultTenant;
            }

            Tracing.Information("[UserAccountService.ResetPassword] called: {0}, {1}", tenant, email);

            if (String.IsNullOrWhiteSpace(tenant))
            {
                Tracing.Error("[UserAccountService.ResetPassword] failed -- null tenant");
                throw new ValidationException("Invalid tenant.");
            }
            if (String.IsNullOrWhiteSpace(email))
            {
                Tracing.Error("[UserAccountService.ResetPassword] failed -- null email");
                throw new ValidationException("Invalid email.");
            }

            var account = this.GetByEmail(tenant, email);
            if (account == null) throw new ValidationException("Invalid email.");

            account.ResetPassword();
            Update(account);
        }

        public virtual bool ChangePasswordFromResetKey(string key, string newPassword)
        {
            Tracing.Information("[UserAccountService.ChangePasswordFromResetKey] called: {0}", key);

            if (String.IsNullOrWhiteSpace(key))
            {
                Tracing.Error("[UserAccountService.ChangePasswordFromResetKey] failed -- null key");
                return false;
            }

            var account = this.GetByVerificationKey(key);
            if (account == null) return false;

            ValidatePassword(account, newPassword);

            var result = account.ChangePasswordFromResetKey(key, newPassword);
            Update(account);

            Tracing.Verbose("[UserAccountService.ChangePasswordFromResetKey] result: {0}", result);

            return result;
        }

        public virtual void SendUsernameReminder(string email)
        {
            SendUsernameReminder(null, email);
        }

        public virtual void SendUsernameReminder(string tenant, string email)
        {
            if (!SecuritySettings.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.SendUsernameReminder] applying default tenant");
                tenant = SecuritySettings.DefaultTenant;
            }

            Tracing.Information("[UserAccountService.SendUsernameReminder] called: {0}, {1}", tenant, email);

            if (String.IsNullOrWhiteSpace(tenant))
            {
                Tracing.Error("[UserAccountService.SendUsernameReminder] failed -- null tenant");
                throw new ArgumentNullException("tenant");
            }
            if (String.IsNullOrWhiteSpace(email))
            {
                Tracing.Error("[UserAccountService.SendUsernameReminder] failed -- null email");
                throw new ValidationException("Invalid email.");
            }

            var account = this.GetByEmail(tenant, email);
            if (account == null) throw new ValidationException("Invalid email.");

            account.SendAccountNameReminder();
            Update(account);
        }

        public virtual void ChangeUsername(Guid accountID, string newUsername)
        {
            Tracing.Information("[UserAccountService.ChangeUsername] called: {0}", accountID);

            if (SecuritySettings.EmailIsUsername)
            {
                Tracing.Error("[UserAccountService.ChangeUsername] failed -- SecuritySettings.EmailIsUsername is true, use ChangeEmail API instead");
                throw new Exception("EmailIsUsername is enabled in SecuritySettings -- use ChangeEmail APIs instead.");
            }

            if (String.IsNullOrWhiteSpace(newUsername))
            {
                Tracing.Error("[UserAccountService.ChangeUsername] failed -- null newUsername");
                throw new ValidationException("Invalid username.");
            }

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            ValidateUsername(account, newUsername);

            account.ChangeUsername(newUsername);
            Update(account);
        }

        public virtual void ChangeEmailRequest(Guid accountID, string newEmail)
        {
            Tracing.Information("[UserAccountService.ChangeEmailRequest] called: {0}, {1}", accountID, newEmail);

            if (String.IsNullOrWhiteSpace(newEmail))
            {
                Tracing.Error("[UserAccountService.ChangeEmailRequest] failed -- null newEmail");
                throw new ValidationException("Invalid email.");
            }

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            ValidateEmail(account, newEmail);

            account.ChangeEmailRequest(newEmail);
            Update(account);
        }

        public virtual bool ChangeEmailFromKey(Guid accountID, string password, string key, string newEmail)
        {
            Tracing.Information("[UserAccountService.ChangeEmailFromKey] called: {0}, {1}", accountID, newEmail);

            if (String.IsNullOrWhiteSpace(password))
            {
                Tracing.Error("[UserAccountService.ChangeEmailFromKey] failed -- null password");
                throw new ValidationException("Invalid password.");
            }
            if (String.IsNullOrWhiteSpace(key))
            {
                Tracing.Error("[UserAccountService.ChangeEmailFromKey] failed -- null key");
                throw new ValidationException("Invalid key.");
            }
            if (String.IsNullOrWhiteSpace(newEmail))
            {
                Tracing.Error("[UserAccountService.ChangeEmailFromKey] failed -- null newEmail");
                throw new ValidationException("Invalid email.");
            }

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            if (!Authenticate(account, password, AuthenticationPurpose.VerifyPassword))
            {
                Tracing.Error("[UserAccountService.ChangeEmailFromKey] failed -- authN failed");
                throw new ValidationException("Invalid password.");
            }

            ValidateEmail(account, newEmail);

            var result = account.ChangeEmailFromKey(key, newEmail);
            if (result && SecuritySettings.EmailIsUsername)
            {
                Tracing.Verbose("[UserAccountService.ChangeEmailFromKey] security setting EmailIsUsername is true and AllowEmailChangeWhenEmailIsUsername is true, so changing username: {0}, to: {1}", account.Username, newEmail);
                account.Username = newEmail;
            }
            
            Update(account);

            Tracing.Verbose("[UserAccountService.ChangeEmailFromKey] result: {0}", result);

            return result;
        }

        public virtual void RemoveMobilePhone(Guid accountID)
        {
            Tracing.Information("[UserAccountService.RemoveMobilePhone] called: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            account.ClearMobilePhoneNumber();
            Update(account);
        }

        public virtual void ChangeMobilePhoneRequest(Guid accountID, string newMobilePhoneNumber)
        {
            Tracing.Information("[UserAccountService.ChangeMobilePhoneRequest] called: {0}, {1}", accountID, newMobilePhoneNumber);

            if (String.IsNullOrWhiteSpace(newMobilePhoneNumber))
            {
                Tracing.Error("[UserAccountService.ChangeMobilePhoneRequest] failed -- null newMobilePhoneNumber");
                throw new ValidationException("Invalid Phone Number.");
            }

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            account.RequestChangeMobilePhoneNumber(newMobilePhoneNumber);
            Update(account);
        }

        public virtual bool ChangeMobilePhoneFromCode(Guid accountID, string code)
        {
            Tracing.Information("[UserAccountService.ChangeMobileFromCode] called: {0}", accountID);

            if (String.IsNullOrWhiteSpace(code))
            {
                Tracing.Error("[UserAccountService.ChangeMobileFromCode] failed -- null code");
                return false;
            }

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            var result = account.ConfirmMobilePhoneNumberFromCode(code);
            Update(account);

            Tracing.Verbose("[UserAccountService.ChangeMobileFromCode] result: {0}", result);
            
            return result;
        }

        public virtual bool IsPasswordExpired(Guid accountID)
        {
            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            return IsPasswordExpired(account);
        }
        
        public virtual bool IsPasswordExpired(UserAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");

            return account.GetIsPasswordExpired(SecuritySettings.PasswordResetFrequency);
        }
    }
}
