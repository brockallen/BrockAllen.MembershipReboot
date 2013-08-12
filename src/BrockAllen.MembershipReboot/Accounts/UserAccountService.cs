/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.ComponentModel;
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

            this.userRepository =
                new EventBusUserAccountRepository(
                    userRepository,
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
                tenant = SecuritySettings.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) return null;
            if (String.IsNullOrWhiteSpace(username)) return null;

            var account = userRepository.GetAll().Where(x => x.Tenant == tenant && x.Username == username).SingleOrDefault();
            if (account == null)
            {
                Tracing.Verbose(String.Format("[UserAccountService.GetByUsername] failed to locate account: {0}, {1}", tenant, username));
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
                tenant = SecuritySettings.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) return null;
            if (String.IsNullOrWhiteSpace(email)) return null;

            var account = userRepository.GetAll().Where(x => x.Tenant == tenant && x.Email == email).SingleOrDefault();
            if (account == null)
            {
                Tracing.Verbose(String.Format("[UserAccountService.GetByEmail] failed to locate account: {0}, {1}", tenant, email));
            }
            return account;
        }

        public virtual UserAccount GetByID(Guid id)
        {
            var account = this.userRepository.Get(id);
            if (account == null)
            {
                Tracing.Verbose(String.Format("[UserAccountService.GetByID] failed to locate account: {0}", id));
            }
            return account;
        }

        public virtual UserAccount GetByVerificationKey(string key)
        {
            if (String.IsNullOrWhiteSpace(key)) return null;

            var account = userRepository.GetAll().Where(x => x.VerificationKey == key).SingleOrDefault();
            if (account == null)
            {
                Tracing.Verbose(String.Format("[UserAccountService.GetByVerificationKey] failed to locate account: {0}", key));
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
                Tracing.Verbose(String.Format("[UserAccountService.GetByLinkedAccount] failed to locate by provider: {0}, id: {1}", provider, id));
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
                Tracing.Verbose(String.Format("[UserAccountService.GetByCertificate] failed to locate by thumbprint: {0}, {1}", tenant, thumbprint));
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
            Tracing.Information(String.Format("[UserAccountService.CreateAccount] called: {0}, {1}, {2}", tenant, username, email));

            if (SecuritySettings.EmailIsUsername)
            {
                username = email;
            }

            if (!SecuritySettings.MultiTenant)
            {
                tenant = SecuritySettings.DefaultTenant;
            }

            var account = this.userRepository.Create();
            account.Init(tenant, username, password, email);
            
            ValidateEmail(account, email);
            ValidateUsername(account, username);
            ValidatePassword(account, password);

            account.IsLoginAllowed = SecuritySettings.AllowLoginAfterAccountCreation;
            if (!SecuritySettings.RequireAccountVerification)
            {
                account.VerifyAccount(account.VerificationKey);
            }

            this.userRepository.Add(account);
            
            return account;
        }

        public virtual bool VerifyAccount(string key)
        {
            Tracing.Information(String.Format("[UserAccountService.VerifyAccount] called: {0}", key));

            var account = this.GetByVerificationKey(key);
            if (account == null) return false;

            Tracing.Verbose(String.Format("[UserAccountService.VerifyAccount] account located: {0}, {1}", account.Tenant, account.Username));

            var result = account.VerifyAccount(key);
            Update(account);
            
            return result;
        }

        public virtual bool CancelNewAccount(string key)
        {
            Tracing.Information(String.Format("[UserAccountService.CancelNewAccount] called: {0}", key));

            var account = this.GetByVerificationKey(key);
            if (account == null) return false;

            Tracing.Verbose(String.Format("[UserAccountService.CancelNewAccount] account located: {0}, {1}", account.Tenant, account.Username));

            if (account.CancelNewAccount(key))
            {
                Tracing.Verbose(String.Format("[UserAccountService.CancelNewAccount] account cancelled: {0}, {1}", account.Tenant, account.Username));

                DeleteAccount(account);

                return true;
            }

            return false;
        }

        public virtual void DeleteAccount(Guid accountID)
        {
            Tracing.Information(String.Format("[UserAccountService.DeleteAccount] called: {0}", accountID));

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            DeleteAccount(account);
        }

        protected internal virtual void DeleteAccount(UserAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Verbose(String.Format("[UserAccountService.DeleteAccount] marking account as closed: {0}, {1}", account.Tenant, account.Username));

            account.CloseAccount();
            Update(account);

            if (SecuritySettings.AllowAccountDeletion || !account.IsAccountVerified)
            {
                Tracing.Verbose(String.Format("[UserAccountService.DeleteAccount] removing account record: {0}, {1}", account.Tenant, account.Username));
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

            Tracing.Information(String.Format("[UserAccountService.Authenticate] called: {0}, {1}", tenant, username));

            if (!SecuritySettings.MultiTenant)
            {
                tenant = SecuritySettings.DefaultTenant;
            }
            
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

            Tracing.Information(String.Format("[UserAccountService.AuthenticateWithEmail] called: {0}, {1}", tenant, email));

            if (!SecuritySettings.MultiTenant)
            {
                tenant = SecuritySettings.DefaultTenant;
            }

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

            Tracing.Verbose(String.Format("[UserAccountService.AuthenticateWithUsernameOrEmail]: {0}, {1}", tenant, userNameOrEmail));

            if (!SecuritySettings.MultiTenant)
            {
                tenant = SecuritySettings.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) return false;
            if (String.IsNullOrWhiteSpace(userNameOrEmail)) return false;
            if (String.IsNullOrWhiteSpace(password)) return false;

            if (!SecuritySettings.EmailIsUsername && userNameOrEmail.Contains("@"))
            {
                return AuthenticateWithEmail(tenant, userNameOrEmail, password, out account);
            }
            else
            {
                return Authenticate(tenant, userNameOrEmail, password, out account);
            }
        }
        
        protected internal virtual bool Authenticate(UserAccount account, string password, AuthenticationPurpose purpose)
        {
            int failedLoginCount = SecuritySettings.AccountLockoutFailedLoginAttempts;
            TimeSpan lockoutDuration = SecuritySettings.AccountLockoutDuration;

            var result = account.Authenticate(password, failedLoginCount, lockoutDuration);
            if (result && 
                purpose == AuthenticationPurpose.SignIn && 
                account.UseTwoFactorAuth)
            {
                bool shouldRequestTwoFactorAuthCode = true;
                if (this.Configuration.TwoFactorAuthenticationPolicy != null)
                {
                    shouldRequestTwoFactorAuthCode = this.Configuration.TwoFactorAuthenticationPolicy.RequestRequiresTwoFactorAuth(account);
                }

                if (shouldRequestTwoFactorAuthCode)
                {
                    Tracing.Verbose(String.Format("[UserAccountService.Authenticate] requesting 2fa code: {0}, {1}", account.Tenant, account.Username));
                    result = account.RequestTwoFactorAuthCode();
                }
            }
            Update(account);

            Tracing.Verbose(String.Format("[UserAccountService.Authenticate] authentication outcome: {0}, {1}, {2}", account.Tenant, account.Username, result ? "Successful Login" : "Failed Login"));

            return result;
        }

        public virtual bool AuthenticateWithCode(Guid accountID, string code)
        {
            UserAccount account;
            return AuthenticateWithCode(accountID, code, out account);
        }

        public virtual bool AuthenticateWithCode(Guid accountID, string code, out UserAccount account)
        {
            account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            var result = account.VerifyTwoFactorAuthCode(code);
            Update(account);

            return result;
        }
        
        public virtual bool AuthenticateWithCertificate(X509Certificate2 certificate)
        {
            UserAccount account;
            return AuthenticateWithCertificate(certificate, out account);
        }

        public virtual bool AuthenticateWithCertificate(X509Certificate2 certificate, out UserAccount account)
        {
            certificate.Validate();

            account = this.GetByCertificate(certificate.Thumbprint);
            if (account == null) return false;

            var result = account.Authenticate(certificate);
            Update(account);

            return result;
        }
        
        public virtual void EnableTwoFactorAuthentication(Guid accountID)
        {
            Tracing.Information(String.Format("[UserAccountService.EnableTwoFactorAuthentication] called: {0}", accountID));

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");
            
            account.EnableTwoFactorAuthentication();
            Update(account);
        }

        public virtual void DisableTwoFactorAuthentication(Guid accountID)
        {
            Tracing.Information(String.Format("[UserAccountService.DisableTwoFactorAuthentication] called: {0}", accountID));

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            account.DisableTwoFactorAuthentication();
            Update(account);
        }

        public virtual void SendTwoFactorAuthenticationCode(Guid accountID)
        {
            Tracing.Information(String.Format("[UserAccountService.SendTwoFactorAuthenticationCode] called: {0}", accountID));

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");
            
            account.RequestTwoFactorAuthCode();
            Update(account);
        }

        public virtual void SetPassword(Guid accountID, string newPassword)
        {
            Tracing.Information(String.Format("[UserAccountService.SetPassword] called: {0}", accountID));

            if (String.IsNullOrWhiteSpace(newPassword)) throw new ValidationException("Invalid newPassword.");

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            ValidatePassword(account, newPassword);

            Tracing.Information(String.Format("[UserAccountService.SetPassword] setting new password for: {0}", accountID));

            account.SetPassword(newPassword);
            Update(account);
        }

        public virtual void ChangePassword(Guid accountID, string oldPassword, string newPassword)
        {
            Tracing.Information(String.Format("[UserAccountService.ChangePassword] called: {0}", accountID));

            if (String.IsNullOrWhiteSpace(oldPassword)) throw new ValidationException("Invalid old password.");
            if (String.IsNullOrWhiteSpace(newPassword)) throw new ValidationException("Invalid new password.");

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            ValidatePassword(account, newPassword);

            if (!Authenticate(account, oldPassword, AuthenticationPurpose.VerifyPassword))
            {
                Tracing.Verbose(String.Format("[UserAccountService.ChangePassword] password change failed: {0}, {1}", account.Tenant, account.Username));
                throw new ValidationException("Invalid old password.");
            }

            account.SetPassword(newPassword);
            Update(account);
            
            Tracing.Verbose(String.Format("[UserAccountService.ChangePassword] password change successful: {0}, {1}", account.Tenant, account.Username));
        }

        public virtual void ResetPassword(string email)
        {
            ResetPassword(null, email);
        }

        public virtual void ResetPassword(string tenant, string email)
        {
            Tracing.Information(String.Format("[UserAccountService.ResetPassword] called: {0}, {1}", tenant, email));

            if (!SecuritySettings.MultiTenant)
            {
                tenant = SecuritySettings.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) throw new ValidationException("Invalid tenant.");
            if (String.IsNullOrWhiteSpace(email)) throw new ValidationException("Invalid email.");

            var account = this.GetByEmail(tenant, email);
            if (account == null) throw new ValidationException("Invalid email.");

            account.ResetPassword();
            Update(account);
        }

        public virtual bool ChangePasswordFromResetKey(string key, string newPassword)
        {
            Tracing.Information(String.Format("[UserAccountService.ChangePasswordFromResetKey] called: {0}", key));

            if (String.IsNullOrWhiteSpace(key)) return false;

            var account = this.GetByVerificationKey(key);
            if (account == null) return false;

            Tracing.Verbose(String.Format("[UserAccountService.ChangePasswordFromResetKey] account located: {0}, {1}", account.Tenant, account.Username));

            ValidatePassword(account, newPassword);

            var result = account.ChangePasswordFromResetKey(key, newPassword);
            Update(account);

            Tracing.Verbose(String.Format("[UserAccountService.ChangePasswordFromResetKey] change password outcome: {0}, {1}, {2}", account.Tenant, account.Username, result ? "Successful" : "Failed"));

            return result;
        }

        public virtual void SendUsernameReminder(string email)
        {
            SendUsernameReminder(null, email);
        }

        public virtual void SendUsernameReminder(string tenant, string email)
        {
            Tracing.Information(String.Format("[UserAccountService.SendUsernameReminder] called: {0}, {1}", tenant, email));

            if (!SecuritySettings.MultiTenant)
            {
                tenant = SecuritySettings.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) throw new ArgumentNullException("tenant");
            if (String.IsNullOrWhiteSpace(email)) throw new ValidationException("Invalid email.");

            var account = this.GetByEmail(tenant, email);
            if (account == null) throw new ValidationException("Invalid email.");

            Tracing.Verbose(String.Format("[UserAccountService.SendUsernameReminder] account located: {0}, {1}", account.Tenant, account.Username));

            account.SendAccountNameReminder();
            Update(account);
        }

        public virtual void ChangeUsername(Guid accountID, string newUsername)
        {
            if (SecuritySettings.EmailIsUsername)
            {
                throw new Exception("EmailIsUsername is enabled in SecuritySettings -- use ChangeEmail APIs instead.");
            }

            Tracing.Information(String.Format("[UserAccountService.ChangeUsername] called: {0}, {1}", accountID, newUsername));

            if (String.IsNullOrWhiteSpace(newUsername)) throw new ValidationException("Invalid username.");

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            ValidateUsername(account, newUsername);

            Tracing.Information(String.Format("[UserAccountService.ChangeUsername] changing username: {0}, {1}", accountID, newUsername));

            account.ChangeUsername(newUsername);
            Update(account);
        }

        public virtual void ChangeEmailRequest(Guid accountID, string newEmail)
        {
            Tracing.Information(String.Format("[UserAccountService.ChangeEmailRequest] called: {0}, {1}", accountID, newEmail));

            if (String.IsNullOrWhiteSpace(newEmail)) throw new ValidationException("Invalid email.");

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            Tracing.Verbose(String.Format("[UserAccountService.ChangeEmailRequest] account located: {0}, {1}", account.Tenant, account.Username));

            ValidateEmail(account, newEmail);

            account.ChangeEmailRequest(newEmail);
            Update(account);

            Tracing.Verbose(String.Format("[UserAccountService.ChangeEmailRequest] change request successful: {0}, {1}", account.Tenant, account.Username));
        }

        public virtual bool ChangeEmailFromKey(Guid accountID, string password, string key, string newEmail)
        {
            Tracing.Information(String.Format("[UserAccountService.ChangeEmailFromKey] called: {0}, {1}, {2}", accountID, key, newEmail));

            if (String.IsNullOrWhiteSpace(password)) throw new ValidationException("Invalid password.");
            if (String.IsNullOrWhiteSpace(key)) throw new ValidationException("Invalid key.");
            if (String.IsNullOrWhiteSpace(newEmail)) throw new ValidationException("Invalid email.");

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            Tracing.Verbose(String.Format("[UserAccountService.ChangeEmailFromKey] account located: {0}, {1}", account.Tenant, account.Username));

            if (!Authenticate(account, password, AuthenticationPurpose.VerifyPassword))
            {
                throw new ValidationException("Invalid password.");
            }

            ValidateEmail(account, newEmail);

            var result = account.ChangeEmailFromKey(key, newEmail);

            if (result && SecuritySettings.EmailIsUsername)
            {
                Tracing.Warning(String.Format("[UserAccountService.ChangeEmailFromKey] security setting EmailIsUsername is true and AllowEmailChangeWhenEmailIsUsername is true, so changing username: {0}, to: {1}", account.Username, newEmail));
                account.Username = newEmail;
            }
            
            Update(account);

            Tracing.Verbose(String.Format("[UserAccountService.ChangeEmailFromKey] change email outcome: {0}, {1}, {2}", account.Tenant, account.Username, result ? "Successful" : "Failed"));

            return result;
        }

        public virtual void RemoveMobilePhone(Guid accountID)
        {
            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            account.ClearMobilePhoneNumber();
            Update(account);
        }

        public virtual void ChangeMobilePhoneRequest(Guid accountID, string newMobilePhoneNumber)
        {
            Tracing.Information(String.Format("[UserAccountService.ChangeMobilePhoneRequest] called: {0}, {1}", accountID, newMobilePhoneNumber));

            if (String.IsNullOrWhiteSpace(newMobilePhoneNumber)) throw new ValidationException("Invalid Phone Number.");

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            Tracing.Verbose(String.Format("[UserAccountService.ChangeMobilePhoneRequest] account located: {0}, {1}", account.Tenant, account.Username));

            account.RequestChangeMobilePhoneNumber(newMobilePhoneNumber);
            Update(account);

            Tracing.Verbose(String.Format("[UserAccountService.ChangeMobilePhoneRequest] change request successful: {0}, {1}", account.Tenant, account.Username));
        }

        public virtual bool ChangeMobilePhoneFromCode(Guid accountID, string code)
        {
            Tracing.Information(String.Format("[UserAccountService.ChangeMobileFromCode] called: {0}", accountID));

            if (String.IsNullOrWhiteSpace(code)) return false;

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            Tracing.Verbose(String.Format("[UserAccountService.ChangeMobileFromCode] account located: {0}, {1}", account.Tenant, account.Username));

            var result = account.ConfirmMobilePhoneNumberFromCode(code);
            Update(account);

            Tracing.Warning(String.Format("[UserAccountService.ChangeMobileFromCode] outcome: {0}, {1}, {2}", account.Tenant, account.Username, result));
            
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
