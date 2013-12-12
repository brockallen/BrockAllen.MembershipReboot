/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace BrockAllen.MembershipReboot
{
    public class UserAccountService<TAccount> : IEventSource
        where TAccount : UserAccount
    {
        public MembershipRebootConfiguration<TAccount> Configuration { get; set; }

        IUserAccountRepository<TAccount> userRepository;

        Lazy<AggregateValidator<TAccount>> usernameValidator;
        Lazy<AggregateValidator<TAccount>> emailValidator;
        Lazy<AggregateValidator<TAccount>> passwordValidator;

        public UserAccountService(IUserAccountRepository<TAccount> userRepository)
            : this(new MembershipRebootConfiguration<TAccount>(), userRepository)
        {
        }

        public UserAccountService(MembershipRebootConfiguration<TAccount> configuration, IUserAccountRepository<TAccount> userRepository)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (userRepository == null) throw new ArgumentNullException("userRepository");

            this.Configuration = configuration;

            var validationEventBus = new EventBus();
            validationEventBus.Add(new UserAccountValidator<TAccount>(this));
            this.userRepository = new EventBusUserAccountRepository<TAccount>(this, userRepository,
                new AggregateEventBus { validationEventBus, configuration.ValidationBus },
                configuration.EventBus);

            this.usernameValidator = new Lazy<AggregateValidator<TAccount>>(() =>
            {
                var val = new AggregateValidator<TAccount>();
                if (!this.Configuration.EmailIsUsername)
                {
                    val.Add(UserAccountValidation<TAccount>.UsernameDoesNotContainAtSign);
                    val.Add(UserAccountValidation<TAccount>.UsernameOnlyContainsLettersAndDigits);
                }
                val.Add(UserAccountValidation<TAccount>.UsernameMustNotAlreadyExist);
                val.Add(configuration.UsernameValidator);
                return val;
            });

            this.emailValidator = new Lazy<AggregateValidator<TAccount>>(() =>
            {
                var val = new AggregateValidator<TAccount>();
                val.Add(UserAccountValidation<TAccount>.EmailIsRequiredIfRequireAccountVerificationEnabled);
                val.Add(UserAccountValidation<TAccount>.EmailIsValidFormat);
                val.Add(UserAccountValidation<TAccount>.EmailMustNotAlreadyExist);
                val.Add(configuration.EmailValidator);
                return val;
            });

            this.passwordValidator = new Lazy<AggregateValidator<TAccount>>(() =>
            {
                var val = new AggregateValidator<TAccount>();
                val.Add(UserAccountValidation<TAccount>.PasswordMustBeDifferentThanCurrent);
                val.Add(configuration.PasswordValidator);
                return val;
            });
        }

        protected void ValidateUsername(TAccount account, string value)
        {
            var result = this.usernameValidator.Value.Validate(this, account, value);
            if (result != null && result != ValidationResult.Success)
            {
                Tracing.Error("ValidateUsername failed: " + result.ErrorMessage);
                throw new ValidationException(result.ErrorMessage);
            }
        }
        protected void ValidatePassword(TAccount account, string value)
        {
            var result = this.passwordValidator.Value.Validate(this, account, value);
            if (result != null && result != ValidationResult.Success)
            {
                Tracing.Error("ValidatePassword failed: " + result.ErrorMessage);
                throw new ValidationException(result.ErrorMessage);
            }
        }
        protected void ValidateEmail(TAccount account, string value)
        {
            var result = this.emailValidator.Value.Validate(this, account, value);
            if (result != null && result != ValidationResult.Success)
            {
                Tracing.Error("ValidateEmail failed: " + result.ErrorMessage);
                throw new ValidationException(result.ErrorMessage);
            }
        }

        List<IEvent> events = new List<IEvent>();
        IEnumerable<IEvent> IEventSource.GetEvents()
        {
            return events;
        }
        void IEventSource.Clear()
        {
            events.Clear();
        }
        protected void AddEvent<E>(E evt) where E : IEvent
        {
            if (evt is IAllowMultiple ||
                !events.Any(x => x.GetType() == evt.GetType()))
            {
                events.Add(evt);
            }
        }

        public virtual IQueryable<TAccount> GetAll()
        {
            return GetAll(null);
        }

        public virtual void Update(TAccount account)
        {
            if (account == null)
            {
                Tracing.Error("[UserAccountService.Update] called -- failed null account");
                throw new ArgumentNullException("account");
            }

            Tracing.Information("[UserAccountService.Update] called for account: {0}", account.ID);

            account.LastUpdated = UtcNow;
            this.userRepository.Update(account);
        }

        public virtual IQueryable<TAccount> GetAll(string tenant)
        {
            if (!Configuration.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.GetAll] applying default tenant");
                tenant = Configuration.DefaultTenant;
            }

            Tracing.Information("[UserAccountService.GetAll] called for tenant: {0}", tenant);
            
            if (String.IsNullOrWhiteSpace(tenant)) return Enumerable.Empty<TAccount>().AsQueryable();

            return this.userRepository.GetAll().Where(x => x.Tenant == tenant && x.IsAccountClosed == false);
        }

        public virtual TAccount GetByUsername(string username)
        {
            return GetByUsername(null, username);
        }

        public virtual TAccount GetByUsername(string tenant, string username)
        {
            if (!Configuration.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.GetByUsername] applying default tenant");
                tenant = Configuration.DefaultTenant;
            }

            Tracing.Information("[UserAccountService.GetByUsername] called for tenant: {0}, username: {1}", tenant, username);

            if (!Configuration.UsernamesUniqueAcrossTenants && String.IsNullOrWhiteSpace(tenant)) return null;
            if (String.IsNullOrWhiteSpace(username)) return null;

            var query = userRepository.GetAll().Where(x => x.Username == username);
            if (!Configuration.UsernamesUniqueAcrossTenants)
            {
                query = query.Where(x => x.Tenant == tenant);
            }

            var account = query.SingleOrDefault();
            if (account == null)
            {
                Tracing.Warning("[UserAccountService.GetByUsername] failed to locate account: {0}, {1}", tenant, username);
            }
            return account;
        }

        public virtual TAccount GetByEmail(string email)
        {
            return GetByEmail(null, email);
        }

        public virtual TAccount GetByEmail(string tenant, string email)
        {
            if (!Configuration.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.GetByEmail] applying default tenant");
                tenant = Configuration.DefaultTenant;
            }

            Tracing.Information("[UserAccountService.GetByEmail] called for tenant: {0}, email: {1}", tenant, email);
            
            if (String.IsNullOrWhiteSpace(tenant)) return null;
            if (String.IsNullOrWhiteSpace(email)) return null;

            var account = userRepository.GetAll().Where(x => x.Tenant == tenant && x.Email == email).SingleOrDefault();
            if (account == null)
            {
                Tracing.Warning("[UserAccountService.GetByEmail] failed to locate account: {0}, {1}", tenant, email);
            }
            return account;
        }

        public virtual TAccount GetByID(Guid id)
        {
            Tracing.Information("[UserAccountService.GetByID] called for id: {0}", id);

            var account = this.userRepository.Get(id);
            if (account == null)
            {
                Tracing.Warning("[UserAccountService.GetByID] failed to locate account: {0}", id);
            }
            return account;
        }

        public virtual TAccount GetByVerificationKey(string key)
        {
            Tracing.Information("[UserAccountService.GetByVerificationKey] called for key: {0}", key);

            if (String.IsNullOrWhiteSpace(key)) return null;

            key =  this.Configuration.Crypto.Hash(key);

            var account = userRepository.GetAll().Where(x => x.VerificationKey == key).SingleOrDefault();
            if (account == null)
            {
                Tracing.Warning("[UserAccountService.GetByVerificationKey] failed to locate account: {0}", key);
            }
            return account;
        }

        public virtual TAccount GetByLinkedAccount(string provider, string id)
        {
            return GetByLinkedAccount(null, provider, id);
        }

        public virtual TAccount GetByLinkedAccount(string tenant, string provider, string id)
        {
            if (!Configuration.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.GetByLinkedAccount] applying default tenant");
                tenant = Configuration.DefaultTenant;
            }

            Tracing.Information("[UserAccountService.GetByLinkedAccount] called for tenant: {0}, provider; {1}, id: {2}", tenant, provider, id);

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

        public virtual TAccount GetByCertificate(string thumbprint)
        {
            return GetByCertificate(null, thumbprint);
        }

        public virtual TAccount GetByCertificate(string tenant, string thumbprint)
        {
            if (!Configuration.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.GetByCertificate] applying default tenant");
                tenant = Configuration.DefaultTenant;
            }

            Tracing.Information("[UserAccountService.GetByCertificate] called for tenant: {0}, thumbprint; {1}", tenant, thumbprint);

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
            Tracing.Information("[UserAccountService.UsernameExists] called for tenant: {0}, username; {1}", tenant, username);

            if (String.IsNullOrWhiteSpace(username)) return false;

            if (Configuration.UsernamesUniqueAcrossTenants)
            {
                return this.userRepository.GetAll().Where(x => x.Username == username).Any();
            }
            else
            {
                if (!Configuration.MultiTenant)
                {
                    Tracing.Verbose("[UserAccountService.UsernameExists] applying default tenant");
                    tenant = Configuration.DefaultTenant;
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
            if (!Configuration.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.EmailExists] applying default tenant");
                tenant = Configuration.DefaultTenant;
            }

            Tracing.Information("[UserAccountService.EmailExists] called for tenant: {0}, email; {1}", tenant, email);

            if (String.IsNullOrWhiteSpace(tenant)) return false;
            if (String.IsNullOrWhiteSpace(email)) return false;

            return this.userRepository.GetAll().Where(x => x.Tenant == tenant && x.Email == email).Any();
        }
        
        protected internal bool EmailExistsOtherThan(TAccount account, string email)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Information("[UserAccountService.EmailExistsOtherThan] called for account id: {0}, email; {1}", account.ID, email);
            
            if (String.IsNullOrWhiteSpace(email)) return false;

            return this.userRepository.GetAll().Where(x => x.Tenant == account.Tenant && x.Email == email && x.ID != account.ID).Any();
        }

        public virtual TAccount CreateAccount(string username, string password, string email)
        {
            return CreateAccount(null, username, password, email);
        }

        public virtual TAccount CreateAccount(string tenant, string username, string password, string email)
        {
            if (Configuration.EmailIsUsername)
            {
                Tracing.Verbose("[UserAccountService.CreateAccount] applying email is username");
                username = email;
            }

            if (!Configuration.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.CreateAccount] applying default tenant");
                tenant = Configuration.DefaultTenant;
            }

            Tracing.Information("[UserAccountService.CreateAccount] called: {0}, {1}, {2}", tenant, username, email);

            var account = this.userRepository.Create();
            Init(account, tenant, username, password, email);

            ValidateEmail(account, email);
            ValidateUsername(account, username);
            ValidatePassword(account, password);

            Tracing.Verbose("[UserAccountService.CreateAccount] success");

            this.userRepository.Add(account);

            return account;
        }

        protected void Init(TAccount account, string tenant, string username, string password, string email)
        {
            Tracing.Information("[UserAccountService.Init] called");

            if (String.IsNullOrWhiteSpace(tenant))
            {
                Tracing.Error("[UserAccountService.Init] failed -- no tenant");
                throw new ArgumentNullException("tenant");
            }
            
            if (String.IsNullOrWhiteSpace(username))
            {
                Tracing.Error("[UserAccountService.Init] failed -- no username");
                throw new ValidationException(Resources.ValidationMessages.UsernameRequired);
            }

            if (password != null && String.IsNullOrWhiteSpace(password.Trim()))
            {
                Tracing.Error("[UserAccountService.Init] failed -- no password");
                throw new ValidationException(Resources.ValidationMessages.PasswordRequired);
            }

            if (account.ID != Guid.Empty)
            {
                Tracing.Error("[UserAccountService.Init] failed -- ID already assigned");
                throw new Exception("Can't call Init if UserAccount is already assigned an ID");
            }

            account.ID = Guid.NewGuid();
            account.Tenant = tenant;
            account.Username = username;
            account.Email = email;
            account.Created = UtcNow;
            account.LastUpdated = account.Created;
            account.HashedPassword = password != null ?
                Configuration.Crypto.HashPassword(password, this.Configuration.PasswordHashingIterationCount) : null;
            account.PasswordChanged = password != null ? account.Created : (DateTime?)null;
            account.IsAccountVerified = false;
            account.AccountTwoFactorAuthMode = TwoFactorAuthMode.None;
            account.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.None;

            account.IsLoginAllowed = Configuration.AllowLoginAfterAccountCreation;
            Tracing.Verbose("[UserAccountService.CreateAccount] SecuritySettings.AllowLoginAfterAccountCreation is set to: {0}", account.IsLoginAllowed);

            string key = null;
            if (!String.IsNullOrWhiteSpace(account.Email))
            {
                Tracing.Verbose("[UserAccountService.CreateAccount] Email was provided, so creating email verification request");
                key = SetVerificationKey(account, VerificationKeyPurpose.ChangeEmail, state: account.Email);
            }

            this.AddEvent(new AccountCreatedEvent<TAccount> { Account = account, InitialPassword = password, VerificationKey = key });
        }

        public virtual void RequestAccountVerification(Guid accountID)
        {
            Tracing.Information("[UserAccountService.RequestAccountVerification] called for account id: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null)
            {
                Tracing.Error("[UserAccountService.RequestAccountVerification] invalid account id");
                throw new Exception("Invalid Account ID");
            }

            if (String.IsNullOrWhiteSpace(account.Email))
            {
                Tracing.Error("[UserAccountService.RequestAccountVerification] email empty");
                throw new ValidationException(Resources.ValidationMessages.EmailRequired);
            }

            if (account.IsAccountVerified)
            {
                Tracing.Error("[UserAccountService.RequestAccountVerification] account already verified");
                throw new ValidationException(Resources.ValidationMessages.AccountAlreadyVerified);
            }

            Tracing.Verbose("[UserAccountService.RequestAccountVerification] creating a new verification key");
            var key = SetVerificationKey(account, VerificationKeyPurpose.ChangeEmail, state: account.Email);
            this.AddEvent(new EmailChangeRequestedEvent<TAccount> { Account = account, NewEmail = account.Email, VerificationKey = key });

            Update(account);
        }

        public virtual void CancelVerification(string key)
        {
            bool closed;
            CancelVerification(key, out closed);
        }

        public virtual void CancelVerification(string key, out bool accountClosed)
        {
            Tracing.Information("[UserAccountService.CancelVerification] called: {0}", key);

            accountClosed = false;

            var account = this.GetByVerificationKey(key);
            if (account == null)
            {
                Tracing.Error("[UserAccountService.CancelVerification] failed -- account not found from key");
                throw new ValidationException(Resources.ValidationMessages.InvalidKey);
            }

            if (account.VerificationPurpose == null)
            {
                Tracing.Error("[UserAccountService.CancelVerification] failed -- no purpose");
                throw new ValidationException(Resources.ValidationMessages.InvalidKey);
            }

            var hashedKey = Configuration.Crypto.Hash(key);
            var result = Configuration.Crypto.SlowEquals(account.VerificationKey, hashedKey);
            if (!result)
            {
                Tracing.Error("[UserAccountService.CancelVerification] failed -- key verification failed");
                throw new ValidationException(Resources.ValidationMessages.InvalidKey);
            }

            if (account.VerificationPurpose == VerificationKeyPurpose.ChangeEmail && 
                account.IsNew())
            {
                // if last login is null then they've never logged in so we can delete the account
                Tracing.Verbose("[UserAccountService.CancelVerification] succeeded (deleting account)");
                DeleteAccount(account);
                accountClosed = true;
            }
            else
            {
                Tracing.Verbose("[UserAccountService.CancelVerification] succeeded");
                ClearVerificationKey(account);
                Update(account);
            }
        }

        public virtual void DeleteAccount(Guid accountID)
        {
            Tracing.Information("[UserAccountService.DeleteAccount] called: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            DeleteAccount(account);
        }

        protected virtual void DeleteAccount(TAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Verbose("[UserAccountService.DeleteAccount] marking account as closed: {0}", account.ID);
            
            CloseAccount(account);
            Update(account);

            if (Configuration.AllowAccountDeletion || account.IsNew())
            {
                Tracing.Verbose("[UserAccountService.DeleteAccount] removing account record: {0}", account.ID);
                this.userRepository.Remove(account);
            }
        }

        public virtual bool Authenticate(string username, string password)
        {
            return Authenticate(null, username, password);
        }
        public virtual bool Authenticate(string username, string password, out TAccount account)
        {
            return Authenticate(null, username, password, out account);
        }

        public virtual bool Authenticate(string tenant, string username, string password)
        {
            TAccount account;
            return Authenticate(tenant, username, password, out account);
        }
        public virtual bool Authenticate(string tenant, string username, string password, out TAccount account)
        {
            account = null;

            if (!Configuration.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.Authenticate] applying default tenant");
                tenant = Configuration.DefaultTenant;
            }

            Tracing.Information("[UserAccountService.Authenticate] called: {0}, {1}", tenant, username);

            if (!Configuration.UsernamesUniqueAcrossTenants && String.IsNullOrWhiteSpace(tenant)) return false;
            if (String.IsNullOrWhiteSpace(username)) return false;
            if (String.IsNullOrWhiteSpace(password))
            {
                Tracing.Error("[UserAccountService.CancelVerification] failed -- empty password");
                return false;
            }

            account = this.GetByUsername(tenant, username);
            if (account == null) return false;

            return Authenticate(account, password, AuthenticationPurpose.SignIn);
        }

        public virtual bool AuthenticateWithEmail(string email, string password)
        {
            return AuthenticateWithEmail(null, email, password);
        }
        public virtual bool AuthenticateWithEmail(string email, string password, out TAccount account)
        {
            return AuthenticateWithEmail(null, email, password, out account);
        }

        public virtual bool AuthenticateWithEmail(string tenant, string email, string password)
        {
            TAccount account;
            return AuthenticateWithEmail(null, email, password, out account);
        }
        public virtual bool AuthenticateWithEmail(string tenant, string email, string password, out TAccount account)
        {
            account = null;

            if (!Configuration.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.AuthenticateWithEmail] applying default tenant");
                tenant = Configuration.DefaultTenant;
            }

            Tracing.Information("[UserAccountService.AuthenticateWithEmail] called: {0}, {1}", tenant, email);

            if (String.IsNullOrWhiteSpace(tenant)) return false;
            if (String.IsNullOrWhiteSpace(email)) return false;
            if (String.IsNullOrWhiteSpace(password))
            {
                Tracing.Error("[UserAccountService.AuthenticateWithEmail] failed -- empty password");
                return false;
            }

            account = this.GetByEmail(tenant, email);
            if (account == null) return false;

            return Authenticate(account, password, AuthenticationPurpose.SignIn);
        }

        public virtual bool AuthenticateWithUsernameOrEmail(string userNameOrEmail, string password, out TAccount account)
        {
            return AuthenticateWithUsernameOrEmail(null, userNameOrEmail, password, out account);
        }

        public virtual bool AuthenticateWithUsernameOrEmail(string tenant, string userNameOrEmail, string password, out TAccount account)
        {
            account = null;

            if (!Configuration.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.AuthenticateWithUsernameOrEmail] applying default tenant");
                tenant = Configuration.DefaultTenant;
            }

            Tracing.Information("[UserAccountService.AuthenticateWithUsernameOrEmail] called {0}, {1}", tenant, userNameOrEmail);

            if (String.IsNullOrWhiteSpace(tenant)) return false;
            if (String.IsNullOrWhiteSpace(userNameOrEmail)) return false;
            if (String.IsNullOrWhiteSpace(password))
            {
                Tracing.Error("[UserAccountService.AuthenticateWithUsernameOrEmail] failed -- empty password");
                return false;
            }

            if (!Configuration.EmailIsUsername && userNameOrEmail.Contains("@"))
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

        protected virtual bool Authenticate(TAccount account, string password, AuthenticationPurpose purpose)
        {
            Tracing.Verbose("[UserAccountService.Authenticate] for account: {0}", account.ID);

            var result = Authenticate(account, password);

            if (purpose == AuthenticationPurpose.SignIn && 
                Configuration.RequireAccountVerification && 
                !account.IsAccountVerified)
            {
                Tracing.Error("[UserAccountService.Authenticate] failed -- account not verified");
                this.AddEvent(new AccountNotVerifiedEvent<TAccount>() { Account = account });
                result = false;
            }

            if (result &&
                purpose == AuthenticationPurpose.SignIn &&
                account.AccountTwoFactorAuthMode != TwoFactorAuthMode.None)
            {
                Tracing.Verbose("[UserAccountService.Authenticate] password authN successful, doing two factor auth checks: {0}, {1}", account.Tenant, account.Username);

                bool shouldRequestTwoFactorAuthCode = true;
                if (this.Configuration.TwoFactorAuthenticationPolicy != null)
                {
                    Tracing.Verbose("[UserAccountService.Authenticate] TwoFactorAuthenticationPolicy configured");

                    var token = this.Configuration.TwoFactorAuthenticationPolicy.GetTwoFactorAuthToken(account);
                    if (!String.IsNullOrWhiteSpace(token))
                    {
                        shouldRequestTwoFactorAuthCode = !VerifyTwoFactorAuthToken(account, token);
                        Tracing.Verbose("[UserAccountService.Authenticate] TwoFactorAuthenticationPolicy token found, was verified: {0}", shouldRequestTwoFactorAuthCode);
                    }
                    else
                    {
                        Tracing.Verbose("[UserAccountService.Authenticate] TwoFactorAuthenticationPolicy no token present");
                    }
                }

                if (shouldRequestTwoFactorAuthCode)
                {
                    if (account.AccountTwoFactorAuthMode == TwoFactorAuthMode.Certificate)
                    {
                        Tracing.Verbose("[UserAccountService.Authenticate] requesting 2fa certificate: {0}, {1}", account.Tenant, account.Username);
                        result = RequestTwoFactorAuthCertificate(account);
                    }

                    if (account.AccountTwoFactorAuthMode == TwoFactorAuthMode.Mobile)
                    {
                        Tracing.Verbose("[UserAccountService.Authenticate] requesting 2fa mobile code: {0}, {1}", account.Tenant, account.Username);
                        result = RequestTwoFactorAuthCode(account);
                    }
                }
                else
                {
                    Tracing.Verbose("[UserAccountService.Authenticate] setting two factor auth status to None");
                    account.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.None;
                }
            }

            Update(account);

            Tracing.Verbose("[UserAccountService.Authenticate] authentication outcome: {0}", result ? "Successful Login" : "Failed Login");

            return result;
        }
        
        protected virtual bool Authenticate(TAccount account, string password)
        {
            Tracing.Information("[UserAccountService.Authenticate] called for accountID: {0}", account.ID);

            if (String.IsNullOrWhiteSpace(password))
            {
                Tracing.Error("[UserAccountService.Authenticate] failed -- no password");
                return false;
            }

            if (!account.HasPassword())
            {
                Tracing.Error("[UserAccountService.Authenticate] failed -- account does not have a password");
                return false;
            }
            
            if (account.IsAccountClosed)
            {
                Tracing.Error("[UserAccountService.Authenticate] failed -- account closed");
                this.AddEvent(new InvalidAccountEvent<TAccount> { Account = account });
                return false;
            }

            if (!account.IsLoginAllowed)
            {
                Tracing.Error("[UserAccountService.Authenticate] failed -- account not allowed to login");
                this.AddEvent(new AccountLockedEvent<TAccount> { Account = account });
                return false;
            }

            if (HasTooManyRecentPasswordFailures(account))
            {
                Tracing.Error("[UserAccountService.Authenticate] failed -- account in lockout due to failed login attempts");
                this.AddEvent(new TooManyRecentPasswordFailuresEvent<TAccount> { Account = account });
                return false;
            }

            var valid = VerifyHashedPassword(account, password);
            if (valid)
            {
                Tracing.Verbose("[UserAccountService.Authenticate] authentication success");
                RecordSuccessfulLogin(account);
                this.AddEvent(new SuccessfulPasswordLoginEvent<TAccount> { Account = account });
            }
            else
            {
                Tracing.Error("[UserAccountService.Authenticate] failed -- invalid password");
                RecordInvalidLoginAttempt(account);
                this.AddEvent(new InvalidPasswordEvent<TAccount> { Account = account });
            }

            return valid;
        }

        protected internal bool VerifyHashedPassword(TAccount account, string password)
        {
            return Configuration.Crypto.VerifyHashedPassword(account.HashedPassword, password);
        }

        protected virtual bool HasTooManyRecentPasswordFailures(TAccount account)
        {
            var result = false;
            if (Configuration.AccountLockoutFailedLoginAttempts <= account.FailedLoginCount)
            {
                result = account.LastFailedLogin >= UtcNow.Subtract(Configuration.AccountLockoutDuration);
            }

            if (result)
            {
                account.FailedLoginCount++;
            }

            return result;
        }

        protected virtual void RecordSuccessfulLogin(TAccount account)
        {
            account.LastLogin = UtcNow;
            account.FailedLoginCount = 0;
        }

        protected virtual void RecordInvalidLoginAttempt(TAccount account)
        {
            account.LastFailedLogin = UtcNow;
            if (account.FailedLoginCount <= 0)
            {
                account.FailedLoginCount = 1;
            }
            else
            {
                account.FailedLoginCount++;
            }
        }

        public virtual bool AuthenticateWithCode(Guid accountID, string code)
        {
            TAccount account;
            return AuthenticateWithCode(accountID, code, out account);
        }

        public virtual bool AuthenticateWithCode(Guid accountID, string code, out TAccount account)
        {
            Tracing.Information("[UserAccountService.AuthenticateWithCode] called {0}", accountID);

            account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            Tracing.Information("[UserAccountService.AuthenticateWithCode] called for accountID: {0}", account.ID);

            if (code == null)
            {
                Tracing.Error("[UserAccountService.AuthenticateWithCode] failed - null code");
                return false;
            }

            if (account.IsAccountClosed)
            {
                Tracing.Error("[UserAccountService.AuthenticateWithCode] failed -- account closed");
                return false;
            }

            if (!account.IsLoginAllowed)
            {
                Tracing.Error("[UserAccountService.AuthenticateWithCode] failed -- login not allowed");
                return false;
            }

            if (account.AccountTwoFactorAuthMode != TwoFactorAuthMode.Mobile)
            {
                Tracing.Error("[UserAccountService.AuthenticateWithCode] failed -- two factor auth mode not mobile");
                return false;
            }

            if (account.CurrentTwoFactorAuthStatus != TwoFactorAuthMode.Mobile)
            {
                Tracing.Error("[UserAccountService.AuthenticateWithCode] failed -- current auth status not mobile");
                return false;
            }

            if (!VerifyMobileCode(account, code))
            {
                Tracing.Error("[UserAccountService.AuthenticateWithCode] failed -- mobile code failed to verify");
                return false;
            }

            ClearMobileAuthCode(account);

            account.LastLogin = UtcNow;
            account.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.None;

            this.AddEvent(new SuccessfulTwoFactorAuthCodeLoginEvent<TAccount> { Account = account });

            Tracing.Verbose("[UserAccountService.AuthenticateWithCode] success ");

            if (this.Configuration.TwoFactorAuthenticationPolicy != null)
            {
                CreateTwoFactorAuthToken(account);
                Tracing.Verbose("[UserAccountService.AuthenticateWithCode] TwoFactorAuthenticationPolicy issuing a new two factor auth token");
            };

            Update(account);

            return true;
        }

        public virtual bool AuthenticateWithCertificate(X509Certificate2 certificate)
        {
            TAccount account;
            return AuthenticateWithCertificate(certificate, out account);
        }

        public virtual bool AuthenticateWithCertificate(X509Certificate2 certificate, out TAccount account)
        {
            Tracing.Information("[UserAccountService.AuthenticateWithCertificate] called");

            if (!certificate.Validate())
            {
                Tracing.Error("[UserAccountService.AuthenticateWithCertificate] failed -- cert failed to validate");
                account = null;
                return false;
            }

            account = this.GetByCertificate(certificate.Thumbprint);
            if (account == null) return false;

            var result = Authenticate(account, certificate);
            Update(account);

            Tracing.Verbose("[UserAccountService.AuthenticateWithCertificate] result {0}", result);

            return result;
        }

        public virtual bool AuthenticateWithCertificate(Guid accountID, X509Certificate2 certificate)
        {
            TAccount account;
            return AuthenticateWithCertificate(accountID, certificate, out account);
        }

        public virtual bool AuthenticateWithCertificate(Guid accountID, X509Certificate2 certificate, out TAccount account)
        {
            Tracing.Information("[UserAccountService.AuthenticateWithCertificate] called for userID: {0}", accountID);

            if (!certificate.Validate())
            {
                Tracing.Error("[UserAccountService.AuthenticateWithCertificate] failed -- cert failed to validate");
                account = null;
                return false;
            }

            account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            var result = Authenticate(account, certificate);
            Update(account);

            Tracing.Verbose("[UserAccountService.AuthenticateWithCertificate] result: {0}", result);

            return result;
        }

        protected virtual bool Authenticate(TAccount account, X509Certificate2 certificate)
        {
            Tracing.Information("[UserAccountService.Authenticate] certificate auth called for account ID: {0}", account.ID);

            if (!certificate.Validate())
            {
                Tracing.Error("[UserAccountService.Authenticate] failed -- cert failed to validate");
                return false;
            }

            Tracing.Verbose("[UserAccountService.Authenticate] cert: {0}", certificate.Thumbprint);

            if (!(certificate.NotBefore < UtcNow && UtcNow < certificate.NotAfter))
            {
                Tracing.Error("[UserAccountService.Authenticate] failed -- invalid certificate dates");
                this.AddEvent(new InvalidCertificateEvent<TAccount> { Account = account, Certificate = certificate });
                return false;
            }

            var match = account.Certificates.FirstOrDefault(x => x.Thumbprint.Equals(certificate.Thumbprint, StringComparison.OrdinalIgnoreCase));
            if (match == null)
            {
                Tracing.Error("[UserAccountService.Authenticate] failed -- no certificate thumbprint match");
                this.AddEvent(new InvalidCertificateEvent<TAccount> { Account = account, Certificate = certificate });
                return false;
            }

            Tracing.Verbose("[UserAccountService.Authenticate] success");

            account.LastLogin = UtcNow;
            account.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.None;

            this.AddEvent(new SuccessfulCertificateLoginEvent<TAccount> { Account = account, UserCertificate = match, Certificate = certificate });

            return true;
        }

        public virtual void SetIsLoginAllowed(Guid accountID, bool isLoginAllowed)
        {
            Tracing.Information("[UserAccountService.SetIsLoginAllowed] called: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            account.IsLoginAllowed = isLoginAllowed;

            Tracing.Verbose("[UserAccountService.SetIsLoginAllowed] success");

            Update(account);
        }

        public virtual void SetRequiresPasswordReset(Guid accountID, bool requiresPasswordReset)
        {
            Tracing.Information("[UserAccountService.SetRequiresPasswordReset] called: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            account.RequiresPasswordReset = requiresPasswordReset;

            Tracing.Verbose("[UserAccountService.SetRequiresPasswordReset] success");

            Update(account);
        }

        public virtual void SetPassword(Guid accountID, string newPassword)
        {
            Tracing.Information("[UserAccountService.SetPassword] called: {0}", accountID);

            if (String.IsNullOrWhiteSpace(newPassword))
            {
                Tracing.Error("[UserAccountService.SetPassword] failed -- null newPassword");
                throw new ValidationException(Resources.ValidationMessages.InvalidNewPassword);
            }

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            ValidatePassword(account, newPassword);

            SetPassword(account, newPassword);
            Update(account);
            
            Tracing.Verbose("[UserAccountService.SetPassword] success");
        }

        public virtual void ChangePassword(Guid accountID, string oldPassword, string newPassword)
        {
            Tracing.Information("[UserAccountService.ChangePassword] called: {0}", accountID);

            if (String.IsNullOrWhiteSpace(oldPassword))
            {
                Tracing.Error("[UserAccountService.ChangePassword] failed -- null oldPassword");
                throw new ValidationException(Resources.ValidationMessages.InvalidOldPassword);
            }
            if (String.IsNullOrWhiteSpace(newPassword))
            {
                Tracing.Error("[UserAccountService.ChangePassword] failed -- null newPassword");
                throw new ValidationException(Resources.ValidationMessages.InvalidNewPassword);
            }

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            ValidatePassword(account, newPassword);

            if (!Authenticate(account, oldPassword, AuthenticationPurpose.VerifyPassword))
            {
                Tracing.Error("[UserAccountService.ChangePassword] failed -- failed authN");
                throw new ValidationException(Resources.ValidationMessages.InvalidOldPassword);
            }

            Tracing.Verbose("[UserAccountService.ChangePassword] success");

            SetPassword(account, newPassword);
            Update(account);
        }

        public virtual void ResetPassword(string email)
        {
            ResetPassword(null, email);
        }

        public virtual void ResetPassword(string tenant, string email)
        {
            if (!Configuration.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.ResetPassword] applying default tenant");
                tenant = Configuration.DefaultTenant;
            }

            Tracing.Information("[UserAccountService.ResetPassword] called: {0}, {1}", tenant, email);

            if (String.IsNullOrWhiteSpace(tenant))
            {
                Tracing.Error("[UserAccountService.ResetPassword] failed -- null tenant");
                throw new ValidationException(Resources.ValidationMessages.InvalidTenant);
            }
            if (String.IsNullOrWhiteSpace(email))
            {
                Tracing.Error("[UserAccountService.ResetPassword] failed -- null email");
                throw new ValidationException(Resources.ValidationMessages.InvalidEmail);
            }

            var account = this.GetByEmail(tenant, email);
            if (account == null) throw new ValidationException(Resources.ValidationMessages.InvalidEmail);

            if (account.PasswordResetSecrets.Count > 0)
            {
                Tracing.Error("[UserAccountService.ResetPassword] failed -- account configured for secret question/answer");
                throw new ValidationException(Resources.ValidationMessages.AccountPasswordResetRequiresSecretQuestion);
            }

            Tracing.Verbose("[UserAccountService.ResetPassword] success");
            
            ResetPassword(account);
            Update(account);
        }

        public virtual bool ChangePasswordFromResetKey(string key, string newPassword)
        {
            TAccount account;
            return ChangePasswordFromResetKey(key, newPassword, out account);
        }

        public virtual bool ChangePasswordFromResetKey(string key, string newPassword, out TAccount account)
        {
            Tracing.Information("[UserAccountService.ChangePasswordFromResetKey] called: {0}", key);

            if (String.IsNullOrWhiteSpace(key))
            {
                Tracing.Error("[UserAccountService.ChangePasswordFromResetKey] failed -- null key");
                account = null;
                return false;
            }

            account = this.GetByVerificationKey(key);
            if (account == null) return false;

            ValidatePassword(account, newPassword);

            if (!account.IsAccountVerified)
            {
                Tracing.Error("[UserAccountService.ChangePasswordFromResetKey] failed -- account not verified");
                return false;
            }

            if (!IsVerificationKeyValid(account, VerificationKeyPurpose.ResetPassword, key))
            {
                Tracing.Error("[UserAccountService.ChangePasswordFromResetKey] failed -- key verification failed");
                return false;
            }

            Tracing.Verbose("[UserAccountService.ChangePasswordFromResetKey] success");

            ClearVerificationKey(account);
            SetPassword(account, newPassword);
            Update(account);

            return true;
        }

        public virtual void AddPasswordResetSecret(Guid accountID, string password, string question, string answer)
        {
            Tracing.Information("[UserAccountService.AddPasswordResetSecret] called: {0}", accountID);

            if (String.IsNullOrWhiteSpace(password))
            {
                Tracing.Error("[UserAccountService.AddPasswordResetSecret] failed -- null oldPassword");
                throw new ValidationException(Resources.ValidationMessages.InvalidPassword);
            }
            if (String.IsNullOrWhiteSpace(question))
            {
                Tracing.Error("[UserAccountService.AddPasswordResetSecret] failed -- null question");
                throw new ValidationException(Resources.ValidationMessages.SecretQuestionRequired);
            }
            if (String.IsNullOrWhiteSpace(answer))
            {
                Tracing.Error("[UserAccountService.AddPasswordResetSecret] failed -- null answer");
                throw new ValidationException(Resources.ValidationMessages.SecretAnswerRequired);
            }

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            if (!Authenticate(account, password, AuthenticationPurpose.VerifyPassword))
            {
                Tracing.Error("[UserAccountService.AddPasswordResetSecret] failed -- failed authN");
                throw new ValidationException(Resources.ValidationMessages.InvalidPassword);
            }

            if (account.PasswordResetSecrets.Any(x => x.Question == question))
            {
                Tracing.Error("[UserAccountService.AddPasswordResetSecret] failed -- question already exists");
                throw new ValidationException(Resources.ValidationMessages.SecretQuestionAlreadyInUse);
            }

            Tracing.Verbose("[UserAccountService.AddPasswordResetSecret] success");

            var secret = new PasswordResetSecret();
            secret.PasswordResetSecretID = Guid.NewGuid();
            secret.UserAccountID = account.ID;
            secret.Question = question;
            secret.Answer = this.Configuration.Crypto.Hash(answer);
            account.PasswordResetSecrets.Add(secret);

            this.AddEvent(new PasswordResetSecretAddedEvent<TAccount> { Account = account, Secret = secret });

            Update(account);
        }

        public virtual void RemovePasswordResetSecret(Guid accountID, Guid questionID)
        {
            Tracing.Information("[UserAccountService.RemovePasswordResetSecret] called: Acct: {0}, Question: {1}", accountID, questionID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            var item = account.PasswordResetSecrets.SingleOrDefault(x => x.PasswordResetSecretID == questionID);
            if (item != null)
            {
                Tracing.Verbose("[UserAccountService.RemovePasswordResetSecret] success -- item removed");

                account.PasswordResetSecrets.Remove(item);
                this.AddEvent(new PasswordResetSecretRemovedEvent<TAccount> { Account = account, Secret = item });
                Update(account);
            }
            else
            {
                Tracing.Verbose("[UserAccountService.RemovePasswordResetSecret] no matching item found");
            }
        }

        public virtual void ResetPasswordFromSecretQuestionAndAnswer(Guid accountID, PasswordResetQuestionAnswer[] answers)
        {
            Tracing.Information("[UserAccountService.ResetPasswordFromSecretQuestionAndAnswer] called: {0}", accountID);

            if (answers == null || answers.Length == 0 || answers.Any(x => String.IsNullOrWhiteSpace(x.Answer)))
            {
                Tracing.Error("[UserAccountService.ResetPasswordFromSecretQuestionAndAnswer] failed -- no answers");
                throw new ValidationException(Resources.ValidationMessages.SecretAnswerRequired);
            }

            var account = this.GetByID(accountID);
            if (account == null)
            {
                Tracing.Error("[UserAccountService.ResetPasswordFromSecretQuestionAndAnswer] failed -- invalid account id");
                throw new Exception("Invalid Account ID");
            }

            if (String.IsNullOrWhiteSpace(account.Email))
            {
                Tracing.Error("[UserAccountService.ResetPasswordFromSecretQuestionAndAnswer] no email to use for password reset");
                throw new ValidationException(Resources.ValidationMessages.PasswordResetErrorNoEmail);
            }

            if (account.PasswordResetSecrets.Count == 0)
            {
                Tracing.Error("[UserAccountService.ResetPasswordFromSecretQuestionAndAnswer] failed -- account not configured for secret question/answer");
                throw new ValidationException(Resources.ValidationMessages.AccountNotConfiguredWithSecretQuestion);
            }

            if (account.FailedPasswordResetCount >= Configuration.AccountLockoutFailedLoginAttempts &&
                account.LastFailedPasswordReset >= UtcNow.Subtract(Configuration.AccountLockoutDuration))
            {
                account.FailedPasswordResetCount++;

                this.AddEvent(new PasswordResetFailedEvent<TAccount> { Account = account });

                Update(account);

                Tracing.Error("[UserAccountService.ResetPasswordFromSecretQuestionAndAnswer] failed -- too many failed password reset attempts");
                throw new ValidationException(Resources.ValidationMessages.InvalidQuestionOrAnswer);
            }

            var secrets = account.PasswordResetSecrets.ToArray();
            var failed = false;
            foreach (var answer in answers)
            {
                var secret = secrets.SingleOrDefault(x => x.PasswordResetSecretID == answer.QuestionID);
                if (secret == null ||
                    !this.Configuration.Crypto.SlowEquals(secret.Answer, this.Configuration.Crypto.Hash(answer.Answer)))
                {
                    Tracing.Error("[UserAccountService.ResetPasswordFromSecretQuestionAndAnswer] failed on question id: {0}", answer.QuestionID);
                    failed = true;
                }
            }

            if (failed)
            {
                account.LastFailedPasswordReset = UtcNow;
                if (account.FailedPasswordResetCount <= 0)
                {
                    account.FailedPasswordResetCount = 1;
                }
                else
                {
                    account.FailedPasswordResetCount++;
                }
                this.AddEvent(new PasswordResetFailedEvent<TAccount> { Account = account });
            }
            else
            {
                Tracing.Verbose("[UserAccountService.ResetPasswordFromSecretQuestionAndAnswer] success");
                
                account.LastFailedPasswordReset = null;
                account.FailedPasswordResetCount = 0;
                ResetPassword(account);
            }

            Update(account);

            if (failed)
            {
                throw new ValidationException(Resources.ValidationMessages.InvalidQuestionOrAnswer);
            }
        }

        public virtual void SendUsernameReminder(string email)
        {
            SendUsernameReminder(null, email);
        }

        public virtual void SendUsernameReminder(string tenant, string email)
        {
            if (!Configuration.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.SendUsernameReminder] applying default tenant");
                tenant = Configuration.DefaultTenant;
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
                throw new ValidationException(Resources.ValidationMessages.InvalidEmail);
            }

            var account = this.GetByEmail(tenant, email);
            if (account == null) throw new ValidationException(Resources.ValidationMessages.InvalidEmail);

            if (!account.IsAccountVerified)
            {
                Tracing.Error("[UserAccountService.SendUsernameReminder] failed -- account not verified");
                throw new ValidationException(Resources.ValidationMessages.AccountNotVerified);
            }

            Tracing.Verbose("[UserAccountService.SendUsernameReminder] success");

            this.AddEvent(new UsernameReminderRequestedEvent<TAccount> { Account = account });
            
            Update(account);
        }

        public virtual void ChangeUsername(Guid accountID, string newUsername)
        {
            Tracing.Information("[UserAccountService.ChangeUsername] called account id: {0}, new username: {1}", accountID, newUsername);

            if (Configuration.EmailIsUsername)
            {
                Tracing.Error("[UserAccountService.ChangeUsername] failed -- SecuritySettings.EmailIsUsername is true, use ChangeEmail API instead");
                throw new Exception("EmailIsUsername is enabled in SecuritySettings -- use ChangeEmail APIs instead.");
            }

            if (String.IsNullOrWhiteSpace(newUsername))
            {
                Tracing.Error("[UserAccountService.ChangeUsername] failed -- null newUsername");
                throw new ValidationException(Resources.ValidationMessages.InvalidUsername);
            }

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            ValidateUsername(account, newUsername);

            Tracing.Verbose("[UserAccountService.ChangeUsername] success");

            account.Username = newUsername;

            this.AddEvent(new UsernameChangedEvent<TAccount> { Account = account }); 
            
            Update(account);
        }

        public virtual void ChangeEmailRequest(Guid accountID, string newEmail)
        {
            Tracing.Information("[UserAccountService.ChangeEmailRequest] called: {0}, {1}", accountID, newEmail);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            ValidateEmail(account, newEmail);

            var oldEmail = account.Email;

            Tracing.Verbose("[UserAccountService.ChangeEmailRequest] creating a new reset key");
            var key = SetVerificationKey(account, VerificationKeyPurpose.ChangeEmail, state: newEmail);
            
            if (!Configuration.RequireAccountVerification)
            {
                Tracing.Verbose("[UserAccountService.ChangeEmailRequest] RequireAccountVerification false, changing email");
                account.IsAccountVerified = false;
                account.Email = newEmail;
                this.AddEvent(new EmailChangedEvent<TAccount> { Account = account, OldEmail = oldEmail, VerificationKey = key });
            }
            else
            {
                Tracing.Verbose("[UserAccountService.ChangeEmailRequest] RequireAccountVerification true, sending changing email");
                this.AddEvent(new EmailChangeRequestedEvent<TAccount> { Account = account, OldEmail = oldEmail, NewEmail = newEmail, VerificationKey = key });
            }

            Tracing.Verbose("[UserAccountService.ChangeEmailRequest] success");
            
            Update(account);
        }

        public virtual void VerifyEmailFromKey(string key)
        {
            TAccount account;
            VerifyEmailFromKey(key, out account);
        }
        
        public virtual void VerifyEmailFromKey(string key, out TAccount account)
        {
            VerifyEmailFromKey(key, null, out account);
        }

        public virtual void VerifyEmailFromKey(string key, string password)
        {
            TAccount account;
            VerifyEmailFromKey(key, password, out account);
        }
        
        public virtual void VerifyEmailFromKey(string key, string password, out TAccount account)
        {
            Tracing.Information("[UserAccountService.VerifyEmailFromKey] called");
            
            if (String.IsNullOrWhiteSpace(key))
            {
                Tracing.Error("[UserAccountService.VerifyEmailFromKey] failed -- null key");
                account = null;
                throw new ValidationException(Resources.ValidationMessages.InvalidKey);
            }

            account = this.GetByVerificationKey(key);
            if (account == null)
            {
                Tracing.Error("[UserAccountService.VerifyEmailFromKey] failed -- invalid key");
                throw new ValidationException(Resources.ValidationMessages.InvalidKey);
            }
            
            Tracing.Information("[UserAccountService.VerifyEmailFromKey] account located: id: {0}", account.ID);

            if (account.HasPassword() && String.IsNullOrWhiteSpace(password))
            {
                Tracing.Error("[UserAccountService.VerifyEmailFromKey] failed -- null password");
                account = null;
                throw new ValidationException(Resources.ValidationMessages.InvalidPassword);
            }

            if (!IsVerificationKeyValid(account, VerificationKeyPurpose.ChangeEmail, key))
            {
                Tracing.Error("[UserAccountService.VerifyEmailFromKey] failed -- key verification failed");
                throw new ValidationException(Resources.ValidationMessages.InvalidKey);
            }

            if (account.HasPassword() && !Authenticate(account, password, AuthenticationPurpose.VerifyPassword))
            {
                Tracing.Error("[UserAccountService.VerifyEmailFromKey] failed -- authN failed");
                throw new ValidationException(Resources.ValidationMessages.InvalidPassword);
            } 
            
            if (String.IsNullOrWhiteSpace(account.VerificationStorage))
            {
                Tracing.Verbose("[UserAccountService.VerifyEmailFromKey] failed -- verification storage empty");
                throw new ValidationException(Resources.ValidationMessages.InvalidKey);
            }

            // one last check
            ValidateEmail(account, account.VerificationStorage);

            account.Email = account.VerificationStorage;
            account.IsAccountVerified = true;

            ClearVerificationKey(account);

            this.AddEvent(new EmailVerifiedEvent<TAccount> { Account = account });

            if (Configuration.EmailIsUsername)
            {
                Tracing.Verbose("[UserAccountService.VerifyEmailFromKey] security setting EmailIsUsername is true and AllowEmailChangeWhenEmailIsUsername is true, so changing username: {0}, to: {1}", account.Username, account.Email);
                account.Username = account.Email;
            }

            Update(account);

            Tracing.Verbose("[UserAccountService.VerifyEmailFromKey] success");
        }

        public virtual void RemoveMobilePhone(Guid accountID)
        {
            Tracing.Information("[UserAccountService.RemoveMobilePhone] called: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            Tracing.Information("[UserAccountService.ClearMobilePhoneNumber] called for accountID: {0}", account.ID);

            if (account.AccountTwoFactorAuthMode == TwoFactorAuthMode.Mobile)
            {
                Tracing.Verbose("[UserAccountService.ClearMobilePhoneNumber] disabling two factor auth");
                ConfigureTwoFactorAuthentication(account, TwoFactorAuthMode.None);
            }

            if (String.IsNullOrWhiteSpace(account.MobilePhoneNumber))
            {
                Tracing.Warning("[UserAccountService.ClearMobilePhoneNumber] nothing to do -- no mobile associated with account");
                return;
            }

            Tracing.Verbose("[UserAccountService.ClearMobilePhoneNumber] success");

            ClearMobileAuthCode(account);

            account.MobilePhoneNumber = null;
            account.MobilePhoneNumberChanged = UtcNow;

            this.AddEvent(new MobilePhoneRemovedEvent<TAccount> { Account = account });

            Update(account);
        }

        public virtual void ChangeMobilePhoneRequest(Guid accountID, string newMobilePhoneNumber)
        {
            Tracing.Information("[UserAccountService.ChangeMobilePhoneRequest] called: {0}, {1}", accountID, newMobilePhoneNumber);

            if (String.IsNullOrWhiteSpace(newMobilePhoneNumber))
            {
                Tracing.Error("[UserAccountService.ChangeMobilePhoneRequest] failed -- null newMobilePhoneNumber");
                throw new ValidationException(Resources.ValidationMessages.InvalidPhone);
            }

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            if (account.MobilePhoneNumber == newMobilePhoneNumber)
            {
                Tracing.Error("[UserAccountService.RequestChangeMobilePhoneNumber] mobile phone same as current");
                throw new ValidationException(Resources.ValidationMessages.MobilePhoneMustBeDifferent);
            }

            if (!IsVerificationPurposeValid(account, VerificationKeyPurpose.ChangeMobile) ||
                CanResendMobileCode(account) ||
                newMobilePhoneNumber != account.VerificationStorage ||
                account.CurrentTwoFactorAuthStatus == TwoFactorAuthMode.Mobile)
            {
                ClearMobileAuthCode(account);

                SetVerificationKey(account, VerificationKeyPurpose.ChangeMobile, state: newMobilePhoneNumber);
                var code = IssueMobileCode(account);

                Tracing.Verbose("[UserAccountService.RequestChangeMobilePhoneNumber] success");

                this.AddEvent(new MobilePhoneChangeRequestedEvent<TAccount> { Account = account, NewMobilePhoneNumber = newMobilePhoneNumber, Code = code });
            }
            else
            {
                Tracing.Verbose("[UserAccountService.RequestChangeMobilePhoneNumber] complete, but not issuing a new code");
            }

            Update(account);
        }

        public virtual bool ChangeMobilePhoneFromCode(Guid accountID, string code)
        {
            Tracing.Information("[UserAccountService.ChangeMobileFromCode] called: {0}", accountID);

            if (String.IsNullOrWhiteSpace(code))
            {
                Tracing.Error("[UserAccountService.ChangeMobileFromCode] failed -- null code");
                throw new ValidationException(Resources.ValidationMessages.CodeRequired);
            }

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            Tracing.Information("[UserAccountService.ConfirmMobilePhoneNumberFromCode] called for accountID: {0}", account.ID);

            if (account.VerificationPurpose != VerificationKeyPurpose.ChangeMobile)
            {
                Tracing.Error("[UserAccountService.ConfirmMobilePhoneNumberFromCode] failed -- invalid verification key purpose");
                return false;
            }

            if (!VerifyMobileCode(account, code))
            {
                Tracing.Error("[UserAccountService.ConfirmMobilePhoneNumberFromCode] failed -- mobile code failed to verify");
                return false;
            }

            Tracing.Verbose("[UserAccountService.ConfirmMobilePhoneNumberFromCode] success");

            account.MobilePhoneNumber = account.VerificationStorage;
            account.MobilePhoneNumberChanged = UtcNow;

            ClearVerificationKey(account);
            ClearMobileAuthCode(account);

            this.AddEvent(new MobilePhoneChangedEvent<TAccount> { Account = account });

            Update(account);

            return true;
        }

        public virtual bool IsPasswordExpired(Guid accountID)
        {
            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            return IsPasswordExpired(account);
        }

        public virtual bool IsPasswordExpired(TAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Information("[UserAccountService.IsPasswordExpired] called: {0}", account.ID);

            if (account.RequiresPasswordReset)
            {
                Tracing.Verbose("[UserAccountService.IsPasswordExpired] RequiresPasswordReset set, returning true");
                return true;
            }

            if (Configuration.PasswordResetFrequency <= 0)
            {
                Tracing.Verbose("[UserAccountService.PasswordResetFrequency ] PasswordResetFrequency not set, returning false");
                return false;
            }

            if (String.IsNullOrWhiteSpace(account.HashedPassword))
            {
                Tracing.Verbose("[UserAccountService.PasswordResetFrequency ] HashedPassword is null, returning false");
                return false;
            }

            if (account.PasswordChanged == null)
            {
                Tracing.Warning("[UserAccountService.PasswordResetFrequency ] PasswordChanged is null, returning false");
                return false;
            }

            var now = UtcNow;
            var last = account.PasswordChanged.Value;
            var result = last.AddDays(Configuration.PasswordResetFrequency) <= now;

            Tracing.Verbose("[UserAccountService.PasswordResetFrequency ] result: {0}", result);
            
            return result;
        }

        protected virtual string SetVerificationKey(TAccount account, VerificationKeyPurpose purpose, string key = null, string state = null)
        {
            if (key == null) key = StripUglyBase64(Configuration.Crypto.GenerateSalt());

            account.VerificationKey = this.Configuration.Crypto.Hash(key);
            account.VerificationPurpose = purpose;
            account.VerificationKeySent = UtcNow;
            account.VerificationStorage = state;

            return key;
        }

        protected virtual bool IsVerificationKeyValid(TAccount account, VerificationKeyPurpose purpose, string key)
        {
            if (!IsVerificationPurposeValid(account, purpose))
            {
                return false;
            }

            var hashedKey = Configuration.Crypto.Hash(key);
            var result = Configuration.Crypto.SlowEquals(account.VerificationKey, hashedKey);
            if (!result)
            {
                Tracing.Error("[UserAccountService.IsVerificationKeyValid] failed -- verification key doesn't match");
                return false;
            }

            Tracing.Verbose("[UserAccountService.IsVerificationKeyValid] success -- verification key valid");
            return true;
        }

        protected virtual bool IsVerificationPurposeValid(TAccount account, VerificationKeyPurpose purpose)
        {
            if (account.VerificationPurpose != purpose)
            {
                Tracing.Error("[UserAccountService.IsVerificationPurposeValid] failed -- verification purpose invalid");
                return false;
            }

            if (IsVerificationKeyStale(account))
            {
                Tracing.Error("[UserAccountService.IsVerificationPurposeValid] failed -- verification key stale");
                return false;
            }

            Tracing.Verbose("[UserAccountService.IsVerificationPurposeValid] success -- verification purpose valid");
            return true;
        }

        protected virtual bool IsVerificationKeyStale(TAccount account)
        {
            if (account.VerificationKeySent == null)
            {
                return true;
            }

            if (account.VerificationKeySent < UtcNow.AddMinutes(-MembershipRebootConstants.UserAccount.VerificationKeyStaleDurationMinutes))
            {
                return true;
            }

            return false;
        }

        protected virtual void ClearVerificationKey(TAccount account)
        {
            account.VerificationKey = null;
            account.VerificationPurpose = null;
            account.VerificationKeySent = null;
            account.VerificationStorage = null;
        }

        protected virtual void SetPassword(TAccount account, string password)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Information("[UserAccountService.SetPassword] called for accountID: {0}", account.ID);

            if (String.IsNullOrWhiteSpace(password))
            {
                Tracing.Error("[UserAccountService.SetPassword] failed -- no password provided");
                throw new ValidationException(Resources.ValidationMessages.InvalidPassword);
            }

            Tracing.Verbose("[UserAccountService.SetPassword] setting new password hash");

            account.HashedPassword = Configuration.Crypto.HashPassword(password, this.Configuration.PasswordHashingIterationCount);
            account.PasswordChanged = UtcNow;
            account.RequiresPasswordReset = false;

            this.AddEvent(new PasswordChangedEvent<TAccount> { Account = account, NewPassword = password });
        }

        protected virtual void ResetPassword(TAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Information("[UserAccountService.ResetPassword] called for accountID: {0}", account.ID);

            if (String.IsNullOrWhiteSpace(account.Email))
            {
                Tracing.Error("[UserAccountService.ResetPassword] no email to use for password reset");
                throw new ValidationException(Resources.ValidationMessages.PasswordResetErrorNoEmail);
            }

            if (!account.IsAccountVerified)
            {
                // if they've not yet verified then don't allow password reset
                // instead request an initial account verification
                if (account.IsNew())
                {
                    Tracing.Verbose("[UserAccountService.ResetPassword] account not verified -- raising account created email event to resend initial email");
                    var key = SetVerificationKey(account, VerificationKeyPurpose.ChangeEmail, state: account.Email);
                    this.AddEvent(new AccountCreatedEvent<TAccount> { Account = account, VerificationKey = key });
                }
                else
                {
                    Tracing.Verbose("[UserAccountService.ResetPassword] account not verified -- raising change email event to resend email verification");
                    var key = SetVerificationKey(account, VerificationKeyPurpose.ChangeEmail, state: account.Email);
                    this.AddEvent(new EmailChangeRequestedEvent<TAccount> { Account = account, NewEmail = account.Email, VerificationKey = key });
                }
            }
            else
            {
                Tracing.Verbose("[UserAccountService.ResetPassword] creating new verification keys");
                var key = SetVerificationKey(account, VerificationKeyPurpose.ResetPassword);

                Tracing.Verbose("[UserAccountService.ResetPassword] account verified -- raising event to send reset notification");
                this.AddEvent(new PasswordResetRequestedEvent<TAccount> { Account = account, VerificationKey = key });
            }
        }

        protected virtual string IssueMobileCode(TAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");

            string code = this.Configuration.Crypto.GenerateNumericCode(MembershipRebootConstants.UserAccount.MobileCodeLength);
            account.MobileCode = this.Configuration.Crypto.HashPassword(code, this.Configuration.PasswordHashingIterationCount);
            account.MobileCodeSent = UtcNow;

            return code;
        }

        protected virtual bool VerifyMobileCode(TAccount account, string code)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (String.IsNullOrWhiteSpace(code)) return false;

            if (IsMobileCodeExpired(account))
            {
                Tracing.Error("[UserAccountService.VerifyMobileCode] failed -- mobile code stale");
                return false;
            }

            if (HasTooManyRecentPasswordFailures(account))
            {
                Tracing.Error("[UserAccountService.VerifyMobileCode] failed -- TooManyRecentPasswordFailures");
                return false;
            }

            var result = this.Configuration.Crypto.VerifyHashedPassword(account.MobileCode, code);
            if (!result)
            {
                RecordInvalidLoginAttempt(account);
                Tracing.Error("[UserAccountService.VerifyMobileCode] failed -- mobile code invalid");
                return false;
            }

            RecordSuccessfulLogin(account);

            Tracing.Verbose("[UserAccountService.VerifyMobileCode] success -- mobile code valid");
            return true;
        }

        protected virtual void ClearMobileAuthCode(TAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Verbose("[UserAccountService.ClearMobileAuthCode] called for account id {0}", account.ID);

            account.MobileCode = null;
            account.MobileCodeSent = null;
            if (account.CurrentTwoFactorAuthStatus == TwoFactorAuthMode.Mobile)
            {
                account.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.None;
            }
            if (account.VerificationPurpose == VerificationKeyPurpose.ChangeMobile)
            {
                ClearVerificationKey(account);
            }
        }

        protected virtual bool IsMobileCodeOlderThan(TAccount account, int duration)
        {
            if (account == null) throw new ArgumentNullException("account");

            if (account.MobileCodeSent == null || String.IsNullOrWhiteSpace(account.MobileCode))
            {
                return true;
            }

            if (account.MobileCodeSent < UtcNow.AddMinutes(-duration))
            {
                return true;
            }

            return false;
        }

        protected virtual bool IsMobileCodeExpired(TAccount account)
        {
            return IsMobileCodeOlderThan(account, MembershipRebootConstants.UserAccount.MobileCodeStaleDurationMinutes);
        }
        
        protected virtual bool CanResendMobileCode(TAccount account)
        {
            return IsMobileCodeOlderThan(account, MembershipRebootConstants.UserAccount.MobileCodeResendDelayMinutes);
        }

        public virtual void ConfigureTwoFactorAuthentication(Guid accountID, TwoFactorAuthMode mode)
        {
            Tracing.Information("[UserAccountService.ConfigureTwoFactorAuthentication] called: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            ConfigureTwoFactorAuthentication(account, mode);
            Update(account);
            
            Tracing.Verbose("[UserAccountService.ConfigureTwoFactorAuthentication] success");
        }

        protected virtual void ConfigureTwoFactorAuthentication(TAccount account, TwoFactorAuthMode mode)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Information("[UserAccountService.ConfigureTwoFactorAuthentication] called for accountID: {0}, mode: {1}", account.ID, mode);

            if (account.AccountTwoFactorAuthMode == mode)
            {
                Tracing.Warning("[UserAccountService.ConfigureTwoFactorAuthentication] nothing to do -- mode is same as current value");
                return;
            }

            if (mode == TwoFactorAuthMode.Mobile &&
                String.IsNullOrWhiteSpace(account.MobilePhoneNumber))
            {
                Tracing.Error("[UserAccountService.ConfigureTwoFactorAuthentication] failed -- mobile requested but no mobile phone for account");
                throw new ValidationException(Resources.ValidationMessages.RegisterMobileForTwoFactor);
            }

            if (mode == TwoFactorAuthMode.Certificate &&
                !account.Certificates.Any())
            {
                Tracing.Error("[UserAccountService.ConfigureTwoFactorAuthentication] failed -- certificate requested but no certificates for account");
                throw new ValidationException(Resources.ValidationMessages.AddClientCertForTwoFactor);
            }

            ClearMobileAuthCode(account);

            account.AccountTwoFactorAuthMode = mode;
            account.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.None;

            if (mode == TwoFactorAuthMode.None)
            {
                RemoveTwoFactorAuthTokens(account);

                Tracing.Verbose("[UserAccountService.ConfigureTwoFactorAuthentication] success -- two factor auth disabled");
                this.AddEvent(new TwoFactorAuthenticationDisabledEvent<TAccount> { Account = account });
            }
            else
            {
                Tracing.Verbose("[UserAccountService.ConfigureTwoFactorAuthentication] success -- two factor auth enabled, mode: {0}", mode);
                this.AddEvent(new TwoFactorAuthenticationEnabledEvent<TAccount> { Account = account, Mode = mode });
            }
        }
        
        protected virtual bool RequestTwoFactorAuthCertificate(TAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Information("[UserAccountService.RequestTwoFactorAuthCertificate] called for accountID: {0}", account.ID);

            if (account.IsAccountClosed)
            {
                Tracing.Error("[UserAccountService.RequestTwoFactorAuthCertificate] failed -- account closed");
                return false;
            }

            if (!account.IsLoginAllowed)
            {
                Tracing.Error("[UserAccountService.RequestTwoFactorAuthCertificate] failed -- login not allowed");
                return false;
            }

            if (account.AccountTwoFactorAuthMode != TwoFactorAuthMode.Certificate)
            {
                Tracing.Error("[UserAccountService.RequestTwoFactorAuthCertificate] failed -- current auth mode is not certificate");
                return false;
            }

            if (!account.Certificates.Any())
            {
                Tracing.Error("[UserAccountService.RequestTwoFactorAuthCertificate] failed -- no certificates");
                return false;
            }

            Tracing.Verbose("[UserAccountService.RequestTwoFactorAuthCertificate] success");

            account.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.Certificate;

            return true;
        }

        protected virtual bool RequestTwoFactorAuthCode(TAccount account, bool force = false)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Information("[UserAccountService.RequestTwoFactorAuthCode] called for accountID: {0}", account.ID);

            if (account.IsAccountClosed)
            {
                Tracing.Error("[UserAccountService.RequestTwoFactorAuthCode] failed -- account closed");
                return false;
            }

            if (!account.IsLoginAllowed)
            {
                Tracing.Error("[UserAccountService.RequestTwoFactorAuthCode] failed -- login not allowed");
                return false;
            }

            if (account.AccountTwoFactorAuthMode != TwoFactorAuthMode.Mobile)
            {
                Tracing.Error("[UserAccountService.RequestTwoFactorAuthCode] failed -- AccountTwoFactorAuthMode not mobile");
                return false;
            }

            if (String.IsNullOrWhiteSpace(account.MobilePhoneNumber))
            {
                Tracing.Error("[UserAccountService.RequestTwoFactorAuthCode] failed -- empty MobilePhoneNumber");
                return false;
            }

            if (CanResendMobileCode(account) || 
                account.CurrentTwoFactorAuthStatus != TwoFactorAuthMode.Mobile)
            {
                ClearMobileAuthCode(account);

                Tracing.Verbose("[UserAccountService.RequestTwoFactorAuthCode] new mobile code issued");
                var code = IssueMobileCode(account);
                account.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.Mobile;

                Tracing.Verbose("[UserAccountService.RequestTwoFactorAuthCode] success");

                this.AddEvent(new TwoFactorAuthenticationCodeNotificationEvent<TAccount> { Account = account, Code = code });
            }
            else
            {
                Tracing.Verbose("[UserAccountService.RequestTwoFactorAuthCode] success, but not issing a new code");
            }

            return true;
        }

        public virtual void SendTwoFactorAuthenticationCode(Guid accountID)
        {
            Tracing.Information("[UserAccountService.SendTwoFactorAuthenticationCode] called: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            RequestTwoFactorAuthCode(account, true);
            Update(account);
        }
        
        protected virtual void CloseAccount(TAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Information("[UserAccountService.CloseAccount] called for accountID: {0}", account.ID);

            ClearVerificationKey(account);
            ClearMobileAuthCode(account);
            ConfigureTwoFactorAuthentication(account, TwoFactorAuthMode.None);

            account.IsLoginAllowed = false;

            if (!account.IsAccountClosed)
            {
                Tracing.Verbose("[UserAccountService.CloseAccount] success");

                account.IsAccountClosed = true;
                account.AccountClosed = UtcNow;

                this.AddEvent(new AccountClosedEvent<TAccount> { Account = account });
            }
            else
            {
                Tracing.Warning("[UserAccountService.CloseAccount] account already closed");
            }
        }

        public virtual void AddClaim(Guid accountID, string type, string value)
        {
            Tracing.Information("[UserAccountService.AddClaim] called for accountID: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            if (String.IsNullOrWhiteSpace(type))
            {
                Tracing.Error("[UserAccountService.AddClaim] failed -- null type");
                throw new ArgumentException("type");
            }

            if (String.IsNullOrWhiteSpace(value))
            {
                Tracing.Error("[UserAccountService.AddClaim] failed -- null value");
                throw new ArgumentException("value");
            }

            if (!account.HasClaim(type, value))
            {
                var claim = new UserClaim();
                claim.UserAccountID = account.ID;
                claim.Type = type;
                claim.Value = value;
                account.Claims.Add(claim);
                this.AddEvent(new ClaimAddedEvent<TAccount> { Account = account, Claim = claim });

                Tracing.Verbose("[UserAccountService.AddClaim] claim added");
            } 
            
            Update(account);
        }

        public virtual void RemoveClaim(Guid accountID, string type)
        {
            Tracing.Information("[UserAccountService.RemoveClaim] called for accountID: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            if (String.IsNullOrWhiteSpace(type))
            {
                Tracing.Error("[UserAccountService.RemoveClaim] failed -- null type");
                throw new ArgumentException("type");
            }

            var claimsToRemove =
                from claim in account.Claims
                where claim.Type == type
                select claim;
            foreach (var claim in claimsToRemove.ToArray())
            {
                account.Claims.Remove(claim);
                this.AddEvent(new ClaimRemovedEvent<TAccount> { Account = account, Claim = claim });
                Tracing.Verbose("[UserAccountService.RemoveClaim] claim removed");
            } 
            
            Update(account);
        }

        public virtual void RemoveClaim(Guid accountID, string type, string value)
        {
            Tracing.Information("[UserAccountService.RemoveClaim] called for accountID: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            if (String.IsNullOrWhiteSpace(type))
            {
                Tracing.Error("[UserAccountService.RemoveClaim] failed -- null type");
                throw new ArgumentException("type");
            }
            if (String.IsNullOrWhiteSpace(value))
            {
                Tracing.Error("[UserAccountService.RemoveClaim] failed -- null value");
                throw new ArgumentException("value");
            }

            var claimsToRemove =
                from claim in account.Claims
                where claim.Type == type && claim.Value == value
                select claim;
            foreach (var claim in claimsToRemove.ToArray())
            {
                account.Claims.Remove(claim);
                this.AddEvent(new ClaimRemovedEvent<TAccount> { Account = account, Claim = claim });
                Tracing.Verbose("[UserAccountService.RemoveClaim] claim removed");
            } 
            
            Update(account);
        }
       
        protected virtual LinkedAccount GetLinkedAccount(TAccount account, string provider, string id)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Information("[UserAccountService.GetLinkedAccount] called for account ID: {0}", account.ID);

            return account.LinkedAccounts.Where(x => x.ProviderName == provider && x.ProviderAccountID == id).SingleOrDefault();
        }

        public virtual void AddOrUpdateLinkedAccount(TAccount account, string provider, string id, IEnumerable<Claim> claims = null)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Information("[UserAccountService.AddOrUpdateLinkedAccount] called for accountID: {0}", account.ID);

            if (String.IsNullOrWhiteSpace(provider))
            {
                Tracing.Error("[UserAccountService.AddOrUpdateLinkedAccount] failed -- null provider");
                throw new ArgumentNullException("provider");
            }
            if (String.IsNullOrWhiteSpace(id))
            {
                Tracing.Error("[UserAccountService.AddOrUpdateLinkedAccount] failed -- null id");
                throw new ArgumentNullException("id");
            }

            var linked = GetLinkedAccount(account, provider, id);
            if (linked == null)
            {
                linked = new LinkedAccount();
                linked.UserAccountID = account.ID;
                linked.ProviderName = provider;
                linked.ProviderAccountID = id;
                account.LinkedAccounts.Add(linked);
                this.AddEvent(new LinkedAccountAddedEvent<TAccount> { Account = account, LinkedAccount = linked });

                Tracing.Verbose("[UserAccountService.AddOrUpdateLinkedAccount] linked account added");
            }

            linked.LastLogin = UtcNow;
            
            claims = claims ?? Enumerable.Empty<Claim>();

            linked.Claims.Clear();

            foreach (var c in claims)
            {
                var claim = new LinkedAccountClaim();
                claim.UserAccountID = linked.UserAccountID;
                claim.ProviderAccountID = linked.ProviderAccountID;
                claim.ProviderName = linked.ProviderName;
                claim.Type = c.Type;
                claim.Value = c.Value;
                linked.Claims.Add(claim);
            }

            Update(account);
        }

        public virtual void RemoveLinkedAccount(Guid accountID, string provider)
        {
            Tracing.Information("[UserAccountService.RemoveLinkedAccount] called for account ID: {0}", accountID);
            
            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            var linked = account.LinkedAccounts.Where(x => x.ProviderName == provider);
            foreach (var item in linked)
            {
                account.LinkedAccounts.Remove(item);
                this.AddEvent(new LinkedAccountRemovedEvent<TAccount> { Account = account, LinkedAccount = item });
                Tracing.Verbose("[UserAccountService.RemoveLinkedAccount] linked account removed");
            }
            
            Update(account);
        }

        public virtual void RemoveLinkedAccount(Guid accountID, string provider, string id)
        {
            Tracing.Information("[UserAccountService.RemoveLinkedAccount] called for account ID: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            var linked = GetLinkedAccount(account, provider, id);
            if (linked != null)
            {
                account.LinkedAccounts.Remove(linked);
                this.AddEvent(new LinkedAccountRemovedEvent<TAccount> { Account = account, LinkedAccount = linked });
                Tracing.Verbose("[UserAccountService.RemoveLinkedAccount] linked account removed");
            }
            
            Update(account);
        }

        public virtual void AddCertificate(Guid accountID, X509Certificate2 certificate)
        {
            Tracing.Information("[UserAccountService.AddCertificate] called for account ID: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            if (!certificate.Validate())
            {
                Tracing.Error("[UserAccountService.AddCertificate] failed -- cert failed to validate");
                throw new ValidationException(Resources.ValidationMessages.InvalidCertificate);
            }
            
            RemoveCertificate(account, certificate);
            AddCertificate(account, certificate.Thumbprint, certificate.Subject); 
            
            Update(account);
        }

        public virtual void AddCertificate(Guid accountID, string thumbprint, string subject)
        {
            Tracing.Information("[UserAccountService.AddCertificate] called for account ID: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            AddCertificate(account, thumbprint, subject);
            Update(account);
        }

        protected virtual void AddCertificate(TAccount account, string thumbprint, string subject)
        {
            Tracing.Information("[UserAccountService.AddCertificate] called for accountID: {0}", account.ID);

            if (String.IsNullOrWhiteSpace(thumbprint))
            {
                Tracing.Error("[UserAccountService.AddCertificate] failed -- null thumbprint");
                throw new ArgumentNullException("thumbprint");
            }
            if (String.IsNullOrWhiteSpace(subject))
            {
                Tracing.Error("[UserAccountService.AddCertificate] failed -- null subject");
                throw new ArgumentNullException("subject");
            }

            var cert = new UserCertificate();
            cert.UserAccountID = account.ID;
            cert.Thumbprint = thumbprint;
            cert.Subject = subject;
            account.Certificates.Add(cert);

            this.AddEvent(new CertificateAddedEvent<TAccount> { Account = account, Certificate = cert });
        }

        public virtual void RemoveCertificate(Guid accountID, X509Certificate2 certificate)
        {
            Tracing.Information("[UserAccountService.RemoveCertificate] called for account ID: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            RemoveCertificate(account, certificate);
            Update(account);
        }
        protected virtual void RemoveCertificate(TAccount account, X509Certificate2 certificate)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Information("[UserAccountService.RemoveCertificate] called for accountID: {0}", account.ID);

            if (certificate == null)
            {
                Tracing.Error("[UserAccountService.RemoveCertificate] failed -- null certificate");
                throw new ArgumentNullException("certificate");
            }
            if (certificate.Handle == IntPtr.Zero)
            {
                Tracing.Error("[UserAccountService.RemoveCertificate] failed -- invalid certificate handle");
                throw new ArgumentException("Invalid certificate");
            }

            RemoveCertificate(account, certificate.Thumbprint);
        }

        public virtual void RemoveCertificate(Guid accountID, string thumbprint)
        {
            Tracing.Information("[UserAccountService.RemoveCertificate] called for account ID: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            RemoveCertificate(account, thumbprint);
            Update(account);
        }
        protected virtual void RemoveCertificate(TAccount account, string thumbprint)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Information("[UserAccountService.RemoveCertificate] called for accountID: {0}", account.ID);

            if (String.IsNullOrWhiteSpace(thumbprint))
            {
                Tracing.Error("[UserAccountService.RemoveCertificate] failed -- no thumbprint");
                throw new ArgumentNullException("thumbprint");
            }

            var certs = account.Certificates.Where(x => x.Thumbprint.Equals(thumbprint, StringComparison.OrdinalIgnoreCase)).ToArray();
            foreach (var cert in certs)
            {
                this.AddEvent(new CertificateRemovedEvent<TAccount> { Account = account, Certificate = cert });
                account.Certificates.Remove(cert);
            }
            Tracing.Error("[UserAccountService.RemoveCertificate] certs removed: {0}", certs.Length);

            if (!account.Certificates.Any() &&
                account.AccountTwoFactorAuthMode == TwoFactorAuthMode.Certificate)
            {
                Tracing.Verbose("[UserAccountService.RemoveCertificate] last cert removed, disabling two factor auth");
                ConfigureTwoFactorAuthentication(account, TwoFactorAuthMode.None);
            }
        }

        protected virtual void CreateTwoFactorAuthToken(TAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Information("[UserAccountService.CreateTwoFactorAuthToken] called for accountID: {0}", account.ID);

            if (account.AccountTwoFactorAuthMode != TwoFactorAuthMode.Mobile)
            {
                Tracing.Error("[UserAccountService.CreateTwoFactorAuthToken] AccountTwoFactorAuthMode is not mobile");
                throw new Exception("AccountTwoFactorAuthMode is not Mobile");
            }

            var value = this.Configuration.Crypto.GenerateSalt();

            var item = new TwoFactorAuthToken();
            item.UserAccountID = account.ID;
            item.Token = this.Configuration.Crypto.Hash(value);
            item.Issued = UtcNow;
            account.TwoFactorAuthTokens.Add(item);

            this.AddEvent(new TwoFactorAuthenticationTokenCreatedEvent<TAccount> { Account = account, Token = value });
        }

        protected virtual bool VerifyTwoFactorAuthToken(TAccount account, string token)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Information("[UserAccountService.VerifyTwoFactorAuthToken] called for accountID: {0}", account.ID);

            if (account.AccountTwoFactorAuthMode != TwoFactorAuthMode.Mobile)
            {
                Tracing.Error("[UserAccountService.VerifyTwoFactorAuthToken] AccountTwoFactorAuthMode is not mobile");
                return false;
            }

            if (String.IsNullOrWhiteSpace(token))
            {
                Tracing.Error("[UserAccountService.VerifyTwoFactorAuthToken] failed -- no token");
                return false;
            }

            token = this.Configuration.Crypto.Hash(token);

            var expiration = UtcNow.AddDays(-MembershipRebootConstants.UserAccount.TwoFactorAuthTokenDurationDays);
            var removequery =
                from t in account.TwoFactorAuthTokens
                where
                    t.Issued < account.PasswordChanged ||
                    t.Issued < account.MobilePhoneNumberChanged ||
                    t.Issued < expiration
                select t;
            var itemsToRemove = removequery.ToArray();

            Tracing.Verbose("[UserAccountService.VerifyTwoFactorAuthToken] number of stale tokens being removed: {0}", itemsToRemove.Length);

            foreach (var item in itemsToRemove)
            {
                account.TwoFactorAuthTokens.Remove(item);
            }

            var matchquery =
                from t in account.TwoFactorAuthTokens.ToArray()
                where Configuration.Crypto.SlowEquals(t.Token, token)
                select t;

            var result = matchquery.Any();

            Tracing.Verbose("[UserAccountService.VerifyTwoFactorAuthToken] result was token verified: {0}", result);

            return result;
        }

        protected virtual void RemoveTwoFactorAuthTokens(TAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Information("[UserAccountService.RemoveTwoFactorAuthTokens] called for accountID: {0}", account.ID);

            var tokens = account.TwoFactorAuthTokens.ToArray();
            foreach (var item in tokens)
            {
                account.TwoFactorAuthTokens.Remove(item);
            }
            
            Tracing.Verbose("[UserAccountService.RemoveTwoFactorAuthTokens] tokens removed: {0}", tokens.Length);
        }

        protected virtual DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }

        static readonly string[] UglyBase64 = { "+", "/", "=" };
        protected virtual string StripUglyBase64(string s)
        {
            if (s == null) return s;
            foreach (var ugly in UglyBase64)
            {
                s = s.Replace(ugly, "");
            }
            return s;
        }
    }
    
    public class UserAccountService : UserAccountService<UserAccount>
    {
        public UserAccountService(IUserAccountRepository userRepository)
            : this(new MembershipRebootConfiguration(), userRepository)
        {
        }

        public UserAccountService(MembershipRebootConfiguration configuration, IUserAccountRepository userRepository)
            : base(configuration, userRepository)
        {
        }
    }
}
