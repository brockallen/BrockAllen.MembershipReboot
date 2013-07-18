/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Transactions;

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

        public UserAccountService(MembershipRebootConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            
            this.Configuration = configuration;

            this.userRepository =
                new EventBusUserAccountRepository(
                    configuration.CreateUserAccountRepository(),
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        public UserAccountService(IUserAccountRepository userAccountRepository, INotificationService notificationService, IPasswordPolicy passwordPolicy)
            : this(ConfigFromDeprecatedInterfaces(userAccountRepository, notificationService, passwordPolicy))
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public UserAccountService(IUserAccountRepository userAccountRepository)
            : this(userAccountRepository, null, null)
        {
        }

        static MembershipRebootConfiguration ConfigFromDeprecatedInterfaces(
            IUserAccountRepository userAccountRepository,
            INotificationService notificationService,
            IPasswordPolicy passwordPolicy)
        {
            if (userAccountRepository == null) throw new ArgumentNullException("userAccountRepository");

            var config = new MembershipRebootConfiguration(SecuritySettings.Instance, new DelegateFactory(() => userAccountRepository));
            config.FromLegacy(notificationService, passwordPolicy);
            return config;
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

        public virtual UserAccount GetByID(string id)
        {
            Guid guid;
            if (Guid.TryParse(id, out guid))
            {
                return GetByID(guid);
            }

            Tracing.Verbose(String.Format("[UserAccountService.GetByID] failed to parse string into guid: {0}", id));

            return null;
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
            this.userRepository.Update(account);
            
            return result;
        }

        public virtual bool CancelNewAccount(string key)
        {
            Tracing.Information(String.Format("[UserAccountService.CancelNewAccount] called: {0}", key));

            var account = this.GetByVerificationKey(key);
            if (account == null) return false;

            Tracing.Verbose(String.Format("[UserAccountService.CancelNewAccount] account located: {0}, {1}", account.Tenant, account.Username));

            if (account.IsAccountVerified) return false;
            if (account.VerificationPurpose != VerificationKeyPurpose.VerifyAccount) return false;
            if (account.VerificationKey != key) return false;

            Tracing.Verbose(String.Format("[UserAccountService.CancelNewAccount] deleting account: {0}, {1}", account.Tenant, account.Username));

            DeleteAccount(account);

            return true;
        }

        public virtual bool DeleteAccount(string username)
        {
            return DeleteAccount(null, username);
        }

        public virtual bool DeleteAccount(string tenant, string username)
        {
            Tracing.Information(String.Format("[UserAccountService.DeleteAccount] called: {0}, {1}", tenant, username));

            if (!SecuritySettings.MultiTenant)
            {
                tenant = SecuritySettings.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) return false;
            if (String.IsNullOrWhiteSpace(username)) return false;

            var account = this.GetByUsername(tenant, username);
            if (account == null) return false;

            DeleteAccount(account);

            return true;
        }

        protected internal virtual void DeleteAccount(UserAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");

            account.CloseAccount();

            if (SecuritySettings.AllowAccountDeletion || !account.IsAccountVerified)
            {
                Tracing.Verbose(String.Format("[UserAccountService.DeleteAccount] removing account record: {0}, {1}", account.Tenant, account.Username));
                this.userRepository.Remove(account);
            }
            else
            {
                Tracing.Verbose(String.Format("[UserAccountService.DeleteAccount] marking account closed: {0}, {1}", account.Tenant, account.Username));
                this.userRepository.Update(account);
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

            return Authenticate(account, password);
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

            return Authenticate(account, password);
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
        
        protected internal virtual bool Authenticate(UserAccount account, string password)
        {
            int failedLoginCount = SecuritySettings.AccountLockoutFailedLoginAttempts;
            TimeSpan lockoutDuration = SecuritySettings.AccountLockoutDuration;

            var result = account.Authenticate(password, failedLoginCount, lockoutDuration);
            this.userRepository.Update(account);

            Tracing.Verbose(String.Format("[UserAccountService.Authenticate] authentication outcome: {0}, {1}, {2}", account.Tenant, account.Username, result ? "Successful Login" : "Failed Login"));

            return result;
        }

        public virtual void SetPassword(string username, string newPassword)
        {
            SetPassword(null, username, newPassword);
        }

        public virtual void SetPassword(string tenant, string username, string newPassword)
        {
            Tracing.Information(String.Format("[UserAccountService.SetPassword] called: {0}, {1}", tenant, username));

            if (!SecuritySettings.MultiTenant)
            {
                tenant = SecuritySettings.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) throw new ValidationException("Invalid tenant.");
            if (String.IsNullOrWhiteSpace(username)) throw new ValidationException("Invalid username.");
            if (String.IsNullOrWhiteSpace(newPassword)) throw new ValidationException("Invalid newPassword.");

            //ValidatePassword(tenant, username, newPassword);
            var account = this.GetByUsername(tenant, username);
            if (account == null) throw new ValidationException("Invalid username.");

            ValidatePassword(account, newPassword);

            Tracing.Information(String.Format("[UserAccountService.SetPassword] setting new password for: {0}, {1}", tenant, username));

            account.SetPassword(newPassword);
            this.userRepository.Update(account);
        }

        public virtual bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            return ChangePassword(null, username, oldPassword, newPassword);
        }

        public virtual bool ChangePassword(
            string tenant, string username,
            string oldPassword, string newPassword)
        {
            Tracing.Information(String.Format("[UserAccountService.ChangePassword] called: {0}, {1}", tenant, username));

            if (!SecuritySettings.MultiTenant)
            {
                tenant = SecuritySettings.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) return false;
            if (String.IsNullOrWhiteSpace(username)) return false;
            if (String.IsNullOrWhiteSpace(oldPassword)) return false;
            if (String.IsNullOrWhiteSpace(newPassword)) return false;

            //ValidatePassword(tenant, username, newPassword);

            var account = this.GetByUsername(tenant, username);
            if (account == null) return false;

            ValidatePassword(account, newPassword);

            if (!Authenticate(account, oldPassword))
            {
                Tracing.Verbose(String.Format("[UserAccountService.ChangePassword] password change failed: {0}, {1}", account.Tenant, account.Username));
                return false;
            }

            account.SetPassword(newPassword);
            this.userRepository.Update(account);
            
            Tracing.Verbose(String.Format("[UserAccountService.ChangePassword] password change successful: {0}, {1}", account.Tenant, account.Username));

            return true;
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
            if (account == null) throw new ValidationException("Invalid account.");

            account.ResetPassword();
            this.userRepository.Update(account);
        }

        public virtual bool ChangePasswordFromResetKey(string key, string newPassword)
        {
            Tracing.Information(String.Format("[UserAccountService.ChangePasswordFromResetKey] called: {0}", key));

            if (String.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            var account = this.GetByVerificationKey(key);
            if (account == null) return false;

            Tracing.Verbose(String.Format("[UserAccountService.ChangePasswordFromResetKey] account located: {0}, {1}", account.Tenant, account.Username));

            //ValidatePassword(account.Tenant, account.Username, newPassword);
            ValidatePassword(account, newPassword);

            var result = account.ChangePasswordFromResetKey(key, newPassword);
            this.userRepository.Update(account);

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

            if (String.IsNullOrWhiteSpace(tenant)) throw new ArgumentException("tenant");
            if (String.IsNullOrWhiteSpace(email)) throw new ValidationException("Invalid email.");

            var account = this.GetByEmail(tenant, email);
            if (account == null) throw new ValidationException("Invalid email.");

            Tracing.Verbose(String.Format("[UserAccountService.SendUsernameReminder] account located: {0}, {1}", account.Tenant, account.Username));

            account.SendAccountNameReminder();
            this.userRepository.Update(account);
        }

        public virtual void ChangeUsername(string username, string newUsername)
        {
            ChangeUsername(null, username, newUsername);
        }

        public virtual void ChangeUsername(string tenant, string username, string newUsername)
        {
            if (SecuritySettings.EmailIsUsername)
            {
                throw new Exception("EmailIsUsername is enabled in SecuritySettings -- use ChangeEmail APIs instead.");
            }

            Tracing.Information(String.Format("[UserAccountService.ChangeUsername] called: {0}, {1}, {2}", tenant, username, newUsername));

            if (!SecuritySettings.MultiTenant)
            {
                tenant = SecuritySettings.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) throw new ArgumentException("tenant");
            if (String.IsNullOrWhiteSpace(username)) throw new ArgumentException("username");
            if (String.IsNullOrWhiteSpace(newUsername)) throw new ArgumentException("newUsername");

            var account = GetByUsername(tenant, username);
            if (account == null) throw new ValidationException("Invalid account");

            ValidateUsername(account, newUsername);
            //if (UsernameExists(tenant, newUsername))
            //{
            //    Tracing.Information(String.Format("[UserAccountService.ChangeUsername] failed because new username already in use: {0}, {1}, {2}", tenant, username, newUsername));
            //    throw new ValidationException("Username is already in use.");
            //}

            Tracing.Information(String.Format("[UserAccountService.ChangeUsername] changing username: {0}, {1}, {2}", tenant, username, newUsername));

            account.ChangeUsername(newUsername);
            this.userRepository.Update(account);
        }

        public virtual bool ChangeEmailRequest(string username, string newEmail)
        {
            return ChangeEmailRequest(null, username, newEmail);
        }

        public virtual bool ChangeEmailRequest(string tenant, string username, string newEmail)
        {
            Tracing.Information(String.Format("[UserAccountService.ChangeEmailRequest] called: {0}, {1}, {2}", tenant, username, newEmail));

            if (!SecuritySettings.MultiTenant)
            {
                tenant = SecuritySettings.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) return false;
            if (String.IsNullOrWhiteSpace(username)) return false;
            if (String.IsNullOrWhiteSpace(newEmail)) return false;

            //EmailAddressAttribute validator = new EmailAddressAttribute();
            //if (!validator.IsValid(newEmail))
            //{
            //    Tracing.Verbose(String.Format("[UserAccountService.ChangeEmailRequest] email validation failed: {0}, {1}, {2}", tenant, username, newEmail));

            //    throw new ValidationException("Email is invalid.");
            //}

            var account = this.GetByUsername(tenant, username);
            if (account == null) return false;

            Tracing.Verbose(String.Format("[UserAccountService.ChangeEmailRequest] account located: {0}, {1}", account.Tenant, account.Username));

            ValidateEmail(account, newEmail);

            //if (EmailExists(tenant, newEmail))
            //{
            //    Tracing.Verbose(String.Format("[UserAccountService.ChangeEmailRequest] Email already exists: {0}, {1}, new email: {2}", tenant, username, newEmail));

            //    throw new ValidationException("Email already in use.");
            //}

            var result = account.ChangeEmailRequest(newEmail);
            this.userRepository.Update(account);

            Tracing.Verbose(String.Format("[UserAccountService.ChangeEmailRequest] change request outcome: {0}, {1}, {2}", account.Tenant, account.Username, result ? "Successful" : "Failed"));

            return result;
        }

        public virtual bool ChangeEmailFromKey(string password, string key, string newEmail)
        {
            Tracing.Information(String.Format("[UserAccountService.ChangeEmailFromKey] called: {0}, {1}", key, newEmail));

            if (String.IsNullOrWhiteSpace(password)) return false;
            if (String.IsNullOrWhiteSpace(key)) return false;
            if (String.IsNullOrWhiteSpace(newEmail)) return false;

            var account = this.GetByVerificationKey(key);
            if (account == null) return false;

            Tracing.Verbose(String.Format("[UserAccountService.ChangeEmailFromKey] account located: {0}, {1}", account.Tenant, account.Username));

            if (!Authenticate(account, password))
            {
                return false;
            }

            ValidateEmail(account, newEmail);

            var result = account.ChangeEmailFromKey(key, newEmail);

            if (result && SecuritySettings.EmailIsUsername)
            {
                Tracing.Warning(String.Format("[UserAccountService.ChangeEmailFromKey] security setting EmailIsUsername is true and AllowEmailChangeWhenEmailIsUsername is true, so changing username: {0}, to: {1}", account.Username, newEmail));
                account.Username = newEmail;
            }
            
            this.userRepository.Update(account);

            Tracing.Verbose(String.Format("[UserAccountService.ChangeEmailFromKey] change email outcome: {0}, {1}, {2}", account.Tenant, account.Username, result ? "Successful" : "Failed"));

            return result;
        }

        public virtual bool IsPasswordExpired(string username)
        {
            return IsPasswordExpired(null, username);
        }

        public virtual bool IsPasswordExpired(string tenant, string username)
        {
            if (!SecuritySettings.MultiTenant)
            {
                tenant = SecuritySettings.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) return false;
            if (String.IsNullOrWhiteSpace(username)) return false;

            var account = this.GetByUsername(tenant, username);
            if (account == null) return false;

            return account.GetIsPasswordExpired(SecuritySettings.PasswordResetFrequency);
        }
    }
}
