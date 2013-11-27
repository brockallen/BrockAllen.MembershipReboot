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
    public class UserAccountService<T> : IEventSource
        where T : UserAccount
    {
        public MembershipRebootConfiguration<T> Configuration { get; set; }

        IUserAccountRepository<T> userRepository;

        Lazy<AggregateValidator<T>> usernameValidator;
        Lazy<AggregateValidator<T>> emailValidator;
        Lazy<AggregateValidator<T>> passwordValidator;

        public UserAccountService(IUserAccountRepository<T> userRepository)
            : this(new MembershipRebootConfiguration<T>(), userRepository)
        {
        }

        public UserAccountService(MembershipRebootConfiguration<T> configuration, IUserAccountRepository<T> userRepository)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (userRepository == null) throw new ArgumentNullException("userRepository");

            this.Configuration = configuration;

            var validationEventBus = new EventBus();
            validationEventBus.Add(new UserAccountValidator<T>(this));
            this.userRepository = new EventBusUserAccountRepository<T>(this, userRepository,
                new AggregateEventBus { validationEventBus, configuration.ValidationBus },
                configuration.EventBus);

            this.usernameValidator = new Lazy<AggregateValidator<T>>(() =>
            {
                var val = new AggregateValidator<T>();
                if (!this.Configuration.EmailIsUsername)
                {
                    val.Add(UserAccountValidation<T>.UsernameDoesNotContainAtSign);
                }
                val.Add(UserAccountValidation<T>.UsernameOnlyContainsLettersAndDigits);
                val.Add(UserAccountValidation<T>.UsernameMustNotAlreadyExist);
                val.Add(configuration.UsernameValidator);
                return val;
            });

            this.emailValidator = new Lazy<AggregateValidator<T>>(() =>
            {
                var val = new AggregateValidator<T>();
                val.Add(UserAccountValidation<T>.EmailIsValidFormat);
                val.Add(UserAccountValidation<T>.EmailMustNotAlreadyExist);
                val.Add(configuration.EmailValidator);
                return val;
            });

            this.passwordValidator = new Lazy<AggregateValidator<T>>(() =>
            {
                var val = new AggregateValidator<T>();
                val.Add(UserAccountValidation<T>.PasswordMustBeDifferentThanCurrent);
                val.Add(configuration.PasswordValidator);
                return val;
            });
        }

        internal protected void ValidateUsername(T account, string value)
        {
            var result = this.usernameValidator.Value.Validate(this, account, value);
            if (result != null && result != ValidationResult.Success)
            {
                Tracing.Error("ValidateUsername failed: " + result.ErrorMessage);
                throw new ValidationException(result.ErrorMessage);
            }
        }
        internal protected void ValidatePassword(T account, string value)
        {
            var result = this.passwordValidator.Value.Validate(this, account, value);
            if (result != null && result != ValidationResult.Success)
            {
                Tracing.Error("ValidatePassword failed: " + result.ErrorMessage);
                throw new ValidationException(result.ErrorMessage);
            }
        }
        internal protected void ValidateEmail(T account, string value)
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
        protected internal void AddEvent<E>(E evt) where E : IEvent
        {
            if (evt is IAllowMultiple ||
                !events.Any(x => x.GetType() == evt.GetType()))
            {
                events.Add(evt);
            }
        }

        public virtual IQueryable<T> GetAll()
        {
            return GetAll(null);
        }

        public virtual void Update(T account)
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

        public virtual IQueryable<T> GetAll(string tenant)
        {
            if (!Configuration.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.GetAll] applying default tenant");
                tenant = Configuration.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) return Enumerable.Empty<T>().AsQueryable();

            return this.userRepository.GetAll().Where(x => x.Tenant == tenant && x.IsAccountClosed == false);
        }

        public virtual T GetByUsername(string username)
        {
            return GetByUsername(null, username);
        }

        public virtual T GetByUsername(string tenant, string username)
        {
            if (!Configuration.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.GetByUsername] applying default tenant");
                tenant = Configuration.DefaultTenant;
            }

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

        public virtual T GetByEmail(string email)
        {
            return GetByEmail(null, email);
        }

        public virtual T GetByEmail(string tenant, string email)
        {
            if (!Configuration.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.GetByEmail] applying default tenant");
                tenant = Configuration.DefaultTenant;
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

        public virtual T GetByID(Guid id)
        {
            var account = this.userRepository.Get(id);
            if (account == null)
            {
                Tracing.Warning("[UserAccountService.GetByID] failed to locate account: {0}", id);
            }
            return account;
        }

        public virtual T GetByVerificationKey(string key)
        {
            if (String.IsNullOrWhiteSpace(key)) return null;

            key =  this.Configuration.Crypto.Hash(key);

            var account = userRepository.GetAll().Where(x => x.VerificationKey == key).SingleOrDefault();
            if (account == null)
            {
                Tracing.Warning("[UserAccountService.GetByVerificationKey] failed to locate account: {0}", key);
            }
            return account;
        }

        public virtual T GetByLinkedAccount(string provider, string id)
        {
            return GetByLinkedAccount(null, provider, id);
        }

        public virtual T GetByLinkedAccount(string tenant, string provider, string id)
        {
            if (!Configuration.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.GetByLinkedAccount] applying default tenant");
                tenant = Configuration.DefaultTenant;
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

        public virtual T GetByCertificate(string thumbprint)
        {
            return GetByCertificate(null, thumbprint);
        }

        public virtual T GetByCertificate(string tenant, string thumbprint)
        {
            if (!Configuration.MultiTenant)
            {
                Tracing.Verbose("[UserAccountService.GetByCertificate] applying default tenant");
                tenant = Configuration.DefaultTenant;
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

            if (String.IsNullOrWhiteSpace(tenant)) return false;
            if (String.IsNullOrWhiteSpace(email)) return false;

            return this.userRepository.GetAll().Where(x => x.Tenant == tenant && x.Email == email).Any();
        }

        public virtual T CreateAccount(string username, string password, string email)
        {
            return CreateAccount(null, username, password, email);
        }

        public virtual T CreateAccount(string tenant, string username, string password, string email)
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

            if (Configuration.RequireAccountVerification)
            {
                RequestAccountVerification(account);
                account.IsLoginAllowed = false;
                Tracing.Verbose("[UserAccountService.CreateAccount] SecuritySettings.RequireAccountVerification is true, so setting IsLoginAllowed to false");
            }
            else
            {
                account.IsLoginAllowed = Configuration.AllowLoginAfterAccountCreation;
                Tracing.Verbose("[UserAccountService.CreateAccount] SecuritySettings.AllowLoginAfterAccountCreation is set to: {0}", account.IsLoginAllowed);
            }

            Tracing.Verbose("[UserAccountService.CreateAccount] success");

            this.userRepository.Add(account);

            return account;
        }

        internal protected virtual void Init(T account, string tenant, string username, string password, string email)
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
            if (String.IsNullOrWhiteSpace(password))
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
            account.HashedPassword = Configuration.Crypto.HashPassword(password, this.Configuration.PasswordHashingIterationCount);
            account.PasswordChanged = account.Created;
            account.IsAccountVerified = false;
            account.IsLoginAllowed = false;
            account.AccountTwoFactorAuthMode = TwoFactorAuthMode.None;
            account.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.None;

            this.AddEvent(new AccountCreatedEvent<T> { Account = account, InitialPassword = password });
        }

        protected internal virtual void RequestAccountVerification(T account, string email = null)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (account.IsAccountVerified)
            {
                throw new Exception("Account already verified");
            }

            Tracing.Verbose("[UserAccountService.RequestAccountVerification] creating a new reset key");
            var key = SetVerificationKey(account, VerificationKeyPurpose.VerifyAccount);

            Tracing.Verbose("[UserAccountService.RequestAccountVerification] success");
            this.AddEvent(new EmailChangeRequestedEvent<T> { Account = account, VerificationKey = key });
        }

        public virtual bool VerifyAccount(string key, string password)
        {
            T account;
            return VerifyAccount(key, password, out account);
        }

        public virtual bool VerifyAccount(string key, string password, out T account)
        {
            Tracing.Information("[UserAccountService.VerifyAccount] called: {0}", key);

            if (String.IsNullOrWhiteSpace(password))
            {
                Tracing.Error("[UserAccountService.VerifyAccount] failed -- no password");
                throw new ValidationException(Resources.ValidationMessages.PasswordRequired);
            }

            account = this.GetByVerificationKey(key);
            if (account == null) return false;

            if (account.IsAccountVerified)
            {
                Tracing.Error("[UserAccountService.VerifyAccount] failed -- account already verified");
                throw new ValidationException(Resources.ValidationMessages.AccountAlreadyVerified);
            }

            if (!IsVerificationKeyValid(account, VerificationKeyPurpose.VerifyAccount, key))
            {
                Tracing.Error("[UserAccountService.VerifyAccount] failed -- key verification failed");
                return false;
            }

            if (!VerifyHashedPassword(account, password))
            {
                Tracing.Error("[UserAccountService.VerifyAccount] failed -- invalid password");
                return false;
            }

            Tracing.Verbose("[UserAccountService.VerifyAccount] succeeded");

            account.IsAccountVerified = true;
            ClearVerificationKey(account);

            account.IsLoginAllowed = Configuration.AllowLoginAfterAccountCreation;
            Tracing.Verbose("[UserAccountService.VerifyAccount] Setting IsLoginAllowed from SecuritySettings.AllowLoginAfterAccountCreation. value is: {0}", account.IsLoginAllowed);

            this.AddEvent(new AccountVerifiedEvent<T> { Account = account });

            Update(account);

            return true;
        }

        public virtual bool CancelNewAccount(string key)
        {
            Tracing.Information("[UserAccountService.CancelNewAccount] called: {0}", key);

            var account = this.GetByVerificationKey(key);
            if (account == null) return false;

            if (account.IsAccountVerified)
            {
                Tracing.Error("[UserAccountService.CancelNewAccount] failed -- account already verified");
                return false;
            }
            
            if (account.LastLogin != null)
            {
                Tracing.Error("[UserAccountService.CancelNewAccount] failed -- user has already logged in -- account isn't new");
                return false;
            }

            // this is the one place where we don't want to check how stale the 
            // verification key is -- if the user has never logged in we want
            // to allow the old verification key to allow account cancellation
            if (account.VerificationPurpose != VerificationKeyPurpose.VerifyAccount)
            {
                Tracing.Error("[UserAccountService.CancelNewAccount] failed -- verification purpose invalid");
                return false;
            }

            var hashedKey = Configuration.Crypto.Hash(key);
            var result = Configuration.Crypto.SlowEquals(account.VerificationKey, hashedKey);
            if (!result)
            {
                Tracing.Error("[UserAccountService.CancelNewAccount] failed -- key verification failed");
                return false;
            }

            Tracing.Verbose("[UserAccountService.CancelNewAccount] succeeded (deleting account)");

            DeleteAccount(account);

            return true;
        }

        public virtual void DeleteAccount(Guid accountID)
        {
            Tracing.Information("[UserAccountService.DeleteAccount] called: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            DeleteAccount(account);
        }

        protected internal virtual void DeleteAccount(T account)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Verbose("[UserAccountService.DeleteAccount] marking account as closed: {0}", account.ID);
            CloseAccount(account);
            Update(account);

            if (Configuration.AllowAccountDeletion || account.LastLogin == null)
            {
                Tracing.Verbose("[UserAccountService.DeleteAccount] removing account record: {0}", account.ID);
                this.userRepository.Remove(account);
            }
        }

        public virtual bool Authenticate(string username, string password)
        {
            return Authenticate(null, username, password);
        }
        public virtual bool Authenticate(string username, string password, out T account)
        {
            return Authenticate(null, username, password, out account);
        }

        public virtual bool Authenticate(string tenant, string username, string password)
        {
            T account;
            return Authenticate(tenant, username, password, out account);
        }
        public virtual bool Authenticate(string tenant, string username, string password, out T account)
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
            if (String.IsNullOrWhiteSpace(password)) return false;

            account = this.GetByUsername(tenant, username);
            if (account == null) return false;

            return Authenticate(account, password, AuthenticationPurpose.SignIn);
        }

        public virtual bool AuthenticateWithEmail(string email, string password)
        {
            return AuthenticateWithEmail(null, email, password);
        }
        public virtual bool AuthenticateWithEmail(string email, string password, out T account)
        {
            return AuthenticateWithEmail(null, email, password, out account);
        }

        public virtual bool AuthenticateWithEmail(string tenant, string email, string password)
        {
            T account;
            return AuthenticateWithEmail(null, email, password, out account);
        }
        public virtual bool AuthenticateWithEmail(string tenant, string email, string password, out T account)
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
            if (String.IsNullOrWhiteSpace(password)) return false;

            account = this.GetByEmail(tenant, email);
            if (account == null) return false;

            return Authenticate(account, password, AuthenticationPurpose.SignIn);
        }

        public virtual bool AuthenticateWithUsernameOrEmail(string userNameOrEmail, string password, out T account)
        {
            return AuthenticateWithUsernameOrEmail(null, userNameOrEmail, password, out account);
        }

        public virtual bool AuthenticateWithUsernameOrEmail(string tenant, string userNameOrEmail, string password, out T account)
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
            if (String.IsNullOrWhiteSpace(password)) return false;

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

        protected internal virtual bool Authenticate(T account, string password, AuthenticationPurpose purpose)
        {
            Tracing.Verbose("[UserAccountService.Authenticate] for account: {0}", account.ID);

            var result = Authenticate(account, password);
            if (result &&
                purpose == AuthenticationPurpose.SignIn &&
                account.AccountTwoFactorAuthMode != TwoFactorAuthMode.None)
            {
                Tracing.Verbose("[UserAccountService.Authenticate] password authN successful, doing two factor auth checks: {0}, {1}", account.Tenant, account.Username);

                bool shouldRequestTwoFactorAuthCode = true;
                if (this.Configuration.TwoFactorAuthenticationPolicy != null)
                {
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
            }

            Update(account);

            Tracing.Verbose("[UserAccountService.Authenticate] authentication outcome: {0}", result ? "Successful Login" : "Failed Login");

            return result;
        }

        public virtual bool AuthenticateWithCode(Guid accountID, string code)
        {
            T account;
            return AuthenticateWithCode(accountID, code, out account);
        }

        public virtual bool AuthenticateWithCode(Guid accountID, string code, out T account)
        {
            Tracing.Information("[UserAccountService.AuthenticateWithCode] called {0}", accountID);

            account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            var result = VerifyTwoFactorAuthCode(account, code);
            Tracing.Verbose("[UserAccountService.AuthenticateWithCode] result {0}", result);

            if (result && this.Configuration.TwoFactorAuthenticationPolicy != null)
            {
                CreateTwoFactorAuthToken(account);
                Tracing.Verbose("[UserAccountService.AuthenticateWithCode] TwoFactorAuthenticationPolicy issuing a new two factor auth token");
            };

            Update(account);

            return result;
        }

        public virtual bool AuthenticateWithCertificate(X509Certificate2 certificate)
        {
            T account;
            return AuthenticateWithCertificate(certificate, out account);
        }

        public virtual bool AuthenticateWithCertificate(X509Certificate2 certificate, out T account)
        {
            Tracing.Information("[UserAccountService.AuthenticateWithCertificate] called");

            if (!certificate.Validate())
            {
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
            T account;
            return AuthenticateWithCertificate(accountID, certificate, out account);
        }

        public virtual bool AuthenticateWithCertificate(Guid accountID, X509Certificate2 certificate, out T account)
        {
            Tracing.Information("[UserAccountService.AuthenticateWithCertificate] called for userID: {0}", accountID);

            certificate.Validate();

            account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            var result = Authenticate(account, certificate);
            Update(account);

            Tracing.Verbose("[UserAccountService.AuthenticateWithCertificate] result: {0}", result);

            return result;
        }

        public virtual void ConfigureTwoFactorAuthentication(Guid accountID, TwoFactorAuthMode mode)
        {
            Tracing.Information("[UserAccountService.ConfigureTwoFactorAuthentication] called: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            ConfigureTwoFactorAuthentication(account, mode);
            Update(account);
        }

        public virtual void SendTwoFactorAuthenticationCode(Guid accountID)
        {
            Tracing.Information("[UserAccountService.SendTwoFactorAuthenticationCode] called: {0}", accountID);

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            RequestTwoFactorAuthCode(account);
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
                Tracing.Error("[UserAccountService.ResetPasswordFromSecretQuestionAndAnswer] failed -- account configured for secret question/answer");
                throw new ValidationException(Resources.ValidationMessages.AccountPasswordResetRequiresSecretQuestion);
            }

            ResetPassword(account);
            Update(account);
        }

        public virtual bool ChangePasswordFromResetKey(string key, string newPassword)
        {
            T account;
            return ChangePasswordFromResetKey(key, newPassword, out account);
        }

        public virtual bool ChangePasswordFromResetKey(string key, string newPassword, out T account)
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

            var result = ChangePasswordFromResetKey(account, key, newPassword);
            Update(account);

            Tracing.Verbose("[UserAccountService.ChangePasswordFromResetKey] result: {0}", result);

            return result;
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

            var secret = new PasswordResetSecret();
            secret.PasswordResetSecretID = Guid.NewGuid();
            secret.UserAccountID = account.ID;
            secret.Question = question;
            secret.Answer = this.Configuration.Crypto.Hash(answer);
            account.PasswordResetSecrets.Add(secret);

            this.AddEvent(new PasswordResetSecretAddedEvent<T> { Account = account, Secret = secret });

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
                account.PasswordResetSecrets.Remove(item);
                this.AddEvent(new PasswordResetSecretRemovedEvent<T> { Account = account, Secret = item });
                Update(account);
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

                this.AddEvent(new PasswordResetFailedEvent<T> { Account = account });

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
                this.AddEvent(new PasswordResetFailedEvent<T> { Account = account });
            }
            else
            {
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

            this.AddEvent(new UsernameReminderRequestedEvent<T> { Account = account });
            
            Update(account);
        }

        public virtual void ChangeUsername(Guid accountID, string newUsername)
        {
            Tracing.Information("[UserAccountService.ChangeUsername] called: {0}", accountID);

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

            this.AddEvent(new UsernameChangedEvent<T> { Account = account }); 
            
            Update(account);
        }

        public virtual void ChangeEmailRequest(Guid accountID, string newEmail)
        {
            Tracing.Information("[UserAccountService.ChangeEmailRequest] called: {0}, {1}", accountID, newEmail);

            if (String.IsNullOrWhiteSpace(newEmail))
            {
                Tracing.Error("[UserAccountService.ChangeEmailRequest] failed -- null newEmail");
                throw new ValidationException(Resources.ValidationMessages.InvalidEmail);
            }

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            ValidateEmail(account, newEmail);

            ChangeEmailRequest(account, newEmail);
            Update(account);
        }
        protected internal virtual void ChangeEmailRequest(T account, string newEmail)
        {
            Tracing.Information("[UserAccountService.ChangeEmailRequest] called for accountID: {0}", account.ID);

            if (String.IsNullOrWhiteSpace(newEmail))
            {
                Tracing.Error("[UserAccountService.ChangeEmailRequest] failed -- invalid newEmail");
                throw new ValidationException(Resources.ValidationMessages.InvalidEmail);
            }

            Tracing.Verbose("[UserAccountService.ChangeEmailRequest] creating a new reset key");
            var key = SetVerificationKey(account, VerificationKeyPurpose.ChangeEmail, state: newEmail);

            Tracing.Verbose("[UserAccountService.ChangeEmailRequest] success");

            this.AddEvent(new EmailChangeRequestedEvent<T> { Account = account, NewEmail = newEmail, VerificationKey = key });
        }
        public virtual bool ChangeEmailFromKey(Guid accountID, string password, string key)
        {
            Tracing.Information("[UserAccountService.ChangeEmailFromKey] called: {0}", accountID);

            if (String.IsNullOrWhiteSpace(password))
            {
                Tracing.Error("[UserAccountService.ChangeEmailFromKey] failed -- null password");
                throw new ValidationException(Resources.ValidationMessages.InvalidPassword);
            }
            if (String.IsNullOrWhiteSpace(key))
            {
                Tracing.Error("[UserAccountService.ChangeEmailFromKey] failed -- null key");
                throw new ValidationException(Resources.ValidationMessages.InvalidKey);
            }

            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            if (!Authenticate(account, password, AuthenticationPurpose.VerifyPassword))
            {
                Tracing.Error("[UserAccountService.ChangeEmailFromKey] failed -- authN failed");
                throw new ValidationException(Resources.ValidationMessages.InvalidPassword);
            }

            // one last check
            ValidateEmail(account, account.VerificationStorage);

            var result = ChangeEmailFromKey(account, key);
            if (result && Configuration.EmailIsUsername)
            {
                Tracing.Verbose("[UserAccountService.ChangeEmailFromKey] security setting EmailIsUsername is true and AllowEmailChangeWhenEmailIsUsername is true, so changing username: {0}, to: {1}", account.Username, account.Email);
                account.Username = account.Email;
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

            ClearMobilePhoneNumber(account);

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

            RequestChangeMobilePhoneNumber(account, newMobilePhoneNumber);
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

            var result = ConfirmMobilePhoneNumberFromCode(account, code);
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

        public virtual bool IsPasswordExpired(T account)
        {
            if (account == null) throw new ArgumentNullException("account");

            if (account.RequiresPasswordReset) return true;

            if (Configuration.PasswordResetFrequency <= 0) return false;

            var now = UtcNow;
            var last = account.PasswordChanged;
            return last.AddDays(Configuration.PasswordResetFrequency) <= now;
        }

        internal string SetVerificationKey(T account, VerificationKeyPurpose purpose, string key = null, string state = null)
        {
            if (key == null) key = StripUglyBase64(Configuration.Crypto.GenerateSalt());

            account.VerificationKey = this.Configuration.Crypto.Hash(key);
            account.VerificationPurpose = purpose;
            account.VerificationKeySent = UtcNow;
            account.VerificationStorage = state;

            return key;
        }

        internal bool IsVerificationKeyValid(T account, VerificationKeyPurpose purpose, string key)
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

        internal bool IsVerificationPurposeValid(T account, VerificationKeyPurpose purpose)
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

        protected internal virtual bool IsVerificationKeyStale(T account)
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

        internal void ClearVerificationKey(T account)
        {
            account.VerificationKey = null;
            account.VerificationPurpose = null;
            account.VerificationKeySent = null;
            account.VerificationStorage = null;
        }

        internal bool VerifyHashedPassword(T account, string password)
        {
            return Configuration.Crypto.VerifyHashedPassword(account.HashedPassword, password);
        }

        protected internal virtual void SetPassword(T account, string password)
        {
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

            this.AddEvent(new PasswordChangedEvent<T> { Account = account, NewPassword = password });
        }

        protected internal virtual void ResetPassword(T account)
        {
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
                Tracing.Verbose("[UserAccountService.ResetPassword] account not verified -- raising account verification event to resend notification");
                RequestAccountVerification(account);
            }
            else
            {
                Tracing.Verbose("[UserAccountService.ResetPassword] creating new verification keys");
                var key = SetVerificationKey(account, VerificationKeyPurpose.ResetPassword);

                Tracing.Verbose("[UserAccountService.ResetPassword] account verified -- raising event to send reset notification");
                this.AddEvent(new PasswordResetRequestedEvent<T> { Account = account, VerificationKey = key });
            }
        }

        protected internal virtual bool ChangePasswordFromResetKey(T account, string key, string newPassword)
        {
            Tracing.Information("[UserAccountService.ChangePasswordFromResetKey] called for accountID: {0}", account.ID);

            if (String.IsNullOrWhiteSpace(key))
            {
                Tracing.Error("[UserAccountService.ChangePasswordFromResetKey] failed -- no key");
                return false;
            }

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

            return true;
        }

        protected internal virtual bool Authenticate(T account, string password)
        {
            Tracing.Information("[UserAccountService.Authenticate] called for accountID: {0}", account.ID);

            if (String.IsNullOrWhiteSpace(password))
            {
                Tracing.Error("[UserAccountService.Authenticate] failed -- no password");
                return false;
            }

            if (account.IsAccountClosed)
            {
                Tracing.Error("[UserAccountService.Authenticate] failed -- account closed");
                return false;
            }

            if (!account.IsLoginAllowed)
            {
                Tracing.Error("[UserAccountService.Authenticate] failed -- account not allowed to login");
                this.AddEvent(new AccountLockedEvent<T> { Account = account });
                return false;
            }

            if (HasTooManyRecentPasswordFailures(account))
            {
                Tracing.Error("[UserAccountService.Authenticate] failed -- account in lockout due to failed login attempts");

                this.AddEvent(new TooManyRecentPasswordFailuresEvent<T> { Account = account });

                return false;
            }

            var valid = VerifyHashedPassword(account, password);
            if (valid)
            {
                Tracing.Verbose("[UserAccountService.Authenticate] authentication success");

                RecordSuccessfulLogin(account);

                this.AddEvent(new SuccessfulPasswordLoginEvent<T> { Account = account });
            }
            else
            {
                Tracing.Error("[UserAccountService.Authenticate] failed -- invalid password");

                RecordInvalidLoginAttempt(account);

                this.AddEvent(new InvalidPasswordEvent<T> { Account = account });
            }

            return valid;
        }

        protected internal virtual bool HasTooManyRecentPasswordFailures(T account)
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

        protected internal virtual void RecordSuccessfulLogin(T account)
        {
            account.LastLogin = UtcNow;
            account.FailedLoginCount = 0;
        }

        protected internal virtual void RecordInvalidLoginAttempt(T account)
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

        protected internal virtual bool Authenticate(T account, X509Certificate2 certificate)
        {
            Tracing.Information("[UserAccountService.Authenticate] certificate auth called for account ID: {0}", account.ID);

            certificate.Validate();

            Tracing.Verbose("[UserAccountService.Authenticate] cert: {0}", certificate.Thumbprint);

            if (!(certificate.NotBefore < UtcNow && UtcNow < certificate.NotAfter))
            {
                Tracing.Error("[UserAccountService.Authenticate] failed -- invalid certificate dates");
                this.AddEvent(new InvalidCertificateEvent<T> { Account = account, Certificate = certificate });
                return false;
            }

            var match = account.Certificates.FirstOrDefault(x => x.Thumbprint.Equals(certificate.Thumbprint, StringComparison.OrdinalIgnoreCase));
            if (match == null)
            {
                Tracing.Error("[UserAccountService.Authenticate] failed -- no certificate thumbprint match");
                this.AddEvent(new InvalidCertificateEvent<T> { Account = account, Certificate = certificate });
                return false;
            }

            Tracing.Verbose("[UserAccountService.Authenticate] success");

            account.LastLogin = UtcNow;
            account.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.None;

            this.AddEvent(new SuccessfulCertificateLoginEvent<T> { Account = account, UserCertificate = match, Certificate = certificate });

            return true;
        }

        string IssueMobileCode(T account)
        {
            string code = this.Configuration.Crypto.GenerateNumericCode(MembershipRebootConstants.UserAccount.MobileCodeLength);
            account.MobileCode = this.Configuration.Crypto.HashPassword(code, this.Configuration.PasswordHashingIterationCount);
            account.MobileCodeSent = UtcNow;

            return code;
        }

        bool VerifyMobileCode(T account, string code)
        {
            if (IsMobileCodeStale(account))
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

        void ClearMobileAuthCode(T account)
        {
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

        protected virtual bool IsMobileCodeStale(T account)
        {
            if (account.MobileCodeSent == null || String.IsNullOrWhiteSpace(account.MobileCode))
            {
                return true;
            }

            if (account.MobileCodeSent < UtcNow.AddMinutes(-MembershipRebootConstants.UserAccount.MobileCodeStaleDurationMinutes))
            {
                return true;
            }

            return false;
        }

        protected internal virtual void RequestChangeMobilePhoneNumber(T account, string newMobilePhoneNumber)
        {
            Tracing.Information("[UserAccountService.RequestChangeMobilePhoneNumber] called for accountID: {0}", account.ID);

            if (String.IsNullOrWhiteSpace(newMobilePhoneNumber))
            {
                Tracing.Error("[UserAccountService.RequestChangeMobilePhoneNumber] invalid mobile phone");
                throw new ValidationException(Resources.ValidationMessages.MobilePhoneRequired);
            }

            if (account.MobilePhoneNumber == newMobilePhoneNumber)
            {
                Tracing.Error("[UserAccountService.RequestChangeMobilePhoneNumber] mobile phone same as current");
                throw new ValidationException(Resources.ValidationMessages.MobilePhoneMustBeDifferent);
            }


            if (!IsVerificationPurposeValid(account, VerificationKeyPurpose.ChangeMobile) ||
                IsMobileCodeStale(account) ||
                newMobilePhoneNumber != account.VerificationStorage ||
                account.CurrentTwoFactorAuthStatus == TwoFactorAuthMode.Mobile)
            {
                ClearMobileAuthCode(account);

                SetVerificationKey(account, VerificationKeyPurpose.ChangeMobile, state: newMobilePhoneNumber);
                var code = IssueMobileCode(account);

                Tracing.Verbose("[UserAccountService.RequestChangeMobilePhoneNumber] success");

                this.AddEvent(new MobilePhoneChangeRequestedEvent<T> { Account = account, NewMobilePhoneNumber = newMobilePhoneNumber, Code = code });
            }
            else
            {
                Tracing.Verbose("[UserAccountService.RequestChangeMobilePhoneNumber] complete, but not issuing a new code");
            }
        }

        protected internal virtual bool ConfirmMobilePhoneNumberFromCode(T account, string code)
        {
            Tracing.Information("[UserAccountService.ConfirmMobilePhoneNumberFromCode] called for accountID: {0}", account.ID);

            if (String.IsNullOrWhiteSpace(code))
            {
                Tracing.Error("[UserAccountService.ConfirmMobilePhoneNumberFromCode] failed -- no code");
                throw new ValidationException(Resources.ValidationMessages.CodeRequired);
            }
            
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

            this.AddEvent(new MobilePhoneChangedEvent<T> { Account = account });

            return true;
        }

        protected internal virtual void ClearMobilePhoneNumber(T account)
        {
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

            this.AddEvent(new MobilePhoneRemovedEvent<T> { Account = account });
        }

        protected internal virtual void ConfigureTwoFactorAuthentication(T account, TwoFactorAuthMode mode)
        {
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
                this.AddEvent(new TwoFactorAuthenticationDisabledEvent<T> { Account = account });
            }
            else
            {
                Tracing.Verbose("[UserAccountService.ConfigureTwoFactorAuthentication] success -- two factor auth enabled, mode: {0}", mode);
                this.AddEvent(new TwoFactorAuthenticationEnabledEvent<T> { Account = account, Mode = mode });
            }
        }

        protected internal virtual bool RequestTwoFactorAuthCertificate(T account)
        {
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

        protected internal virtual bool RequestTwoFactorAuthCode(T account)
        {
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

            if (IsMobileCodeStale(account) || account.CurrentTwoFactorAuthStatus != TwoFactorAuthMode.Mobile)
            {
                ClearMobileAuthCode(account);

                Tracing.Verbose("[UserAccountService.RequestTwoFactorAuthCode] new mobile code issued");
                var code = IssueMobileCode(account);
                account.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.Mobile;

                Tracing.Verbose("[UserAccountService.RequestTwoFactorAuthCode] success");

                this.AddEvent(new TwoFactorAuthenticationCodeNotificationEvent<T> { Account = account, Code = code });
            }
            else
            {
                Tracing.Verbose("[UserAccountService.RequestTwoFactorAuthCode] success, but not issing a new code");
            }

            return true;
        }

        protected internal virtual bool VerifyTwoFactorAuthCode(T account, string code)
        {
            Tracing.Information("[UserAccountService.VerifyTwoFactorAuthCode] called for accountID: {0}", account.ID);

            if (code == null)
            {
                Tracing.Error("[UserAccountService.VerifyTwoFactorAuthCode] failed - null code");
                return false;
            }

            if (account.IsAccountClosed)
            {
                Tracing.Error("[UserAccountService.VerifyTwoFactorAuthCode] failed -- account closed");
                return false;
            }

            if (!account.IsLoginAllowed)
            {
                Tracing.Error("[UserAccountService.VerifyTwoFactorAuthCode] failed -- login not allowed");
                return false;
            }

            if (account.AccountTwoFactorAuthMode != TwoFactorAuthMode.Mobile)
            {
                Tracing.Error("[UserAccountService.VerifyTwoFactorAuthCode] failed -- two factor auth mode not mobile");
                return false;
            }

            if (account.CurrentTwoFactorAuthStatus != TwoFactorAuthMode.Mobile)
            {
                Tracing.Error("[UserAccountService.VerifyTwoFactorAuthCode] failed -- current auth status not mobile");
                return false;
            }

            if (!VerifyMobileCode(account, code))
            {
                Tracing.Error("[UserAccountService.VerifyTwoFactorAuthCode] failed -- mobile code failed to verify");
                return false;
            }

            Tracing.Verbose("[UserAccountService.VerifyTwoFactorAuthCode] success");

            ClearMobileAuthCode(account);

            account.LastLogin = UtcNow;
            account.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.None;

            this.AddEvent(new SuccessfulTwoFactorAuthCodeLoginEvent<T> { Account = account });

            return true;
        }

        protected internal virtual bool ChangeEmailFromKey(T account, string key)
        {
            Tracing.Information("[UserAccountService.ChangeEmailFromKey] called for accountID: {0}", account.ID);

            if (String.IsNullOrWhiteSpace(key))
            {
                Tracing.Error("[UserAccountService.ChangeEmailFromKey] failed -- invalid key");
                throw new ValidationException(Resources.ValidationMessages.InvalidKey);
            }

            if (!IsVerificationKeyValid(account, VerificationKeyPurpose.ChangeEmail, key))
            {
                Tracing.Error("[UserAccountService.ChangeEmailFromKey] failed -- key verification failed");
                return false;
            }

            if (String.IsNullOrWhiteSpace(account.VerificationStorage))
            {
                Tracing.Verbose("[UserAccountService.ChangeEmailFromKey] failed -- verification storage empty");
                return false;
            }

            Tracing.Verbose("[UserAccountService.ChangeEmailFromKey] success");

            var oldEmail = account.Email;
            account.Email = account.VerificationStorage;

            ClearVerificationKey(account);

            this.AddEvent(new EmailChangedEvent<T> { Account = account, OldEmail = oldEmail });

            return true;
        }

        protected internal virtual void CloseAccount(T account)
        {
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

                this.AddEvent(new AccountClosedEvent<T> { Account = account });
            }
            else
            {
                Tracing.Warning("[UserAccountService.CloseAccount] account already closed");
            }
        }

        public virtual void AddClaim(Guid accountID, string type, string value)
        {
            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            AddClaim(account, type, value);
            Update(account);
        }
        protected virtual void AddClaim(T account, string type, string value)
        {
            Tracing.Information("[UserAccountService.AddClaim] called for accountID: {0}", account.ID);

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
                this.AddEvent(new ClaimAddedEvent<T> { Account = account, Claim = claim });
            }
        }

        public virtual void RemoveClaim(Guid accountID, string type)
        {
            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            RemoveClaim(account, type);
            Update(account);
        }
        protected virtual void RemoveClaim(T account, string type)
        {
            Tracing.Information("[UserAccountService.RemoveClaim] called for accountID: {0}", account.ID);

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
                this.AddEvent(new ClaimRemovedEvent<T> { Account = account, Claim = claim });
            }
        }

        public virtual void RemoveClaim(Guid accountID, string type, string value)
        {
            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            RemoveClaim(account, type, value);
            Update(account);
        }
        protected virtual void RemoveClaim(T account, string type, string value)
        {
            Tracing.Information("[UserAccountService.RemoveClaim] called for accountID: {0}", account.ID);

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
                this.AddEvent(new ClaimRemovedEvent<T> { Account = account, Claim = claim });
            }
        }

        protected virtual LinkedAccount GetLinkedAccount(T account, string provider, string id)
        {
            return account.LinkedAccounts.Where(x => x.ProviderName == provider && x.ProviderAccountID == id).SingleOrDefault();
        }

        public virtual void AddOrUpdateLinkedAccount(T account, string provider, string id, IEnumerable<Claim> claims = null)
        {
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
                this.AddEvent(new LinkedAccountAddedEvent<T> { Account = account, LinkedAccount = linked });

                Tracing.Verbose("[UserAccountService.AddOrUpdateLinkedAccount] linked account added");
            }

            linked.LastLogin = UtcNow;
            UpdateClaims(linked, claims);
            Update(account);
        }

        protected virtual void UpdateClaims(LinkedAccount linked, IEnumerable<Claim> claims)
        {
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
        }

        public virtual void RemoveLinkedAccount(Guid accountID, string provider)
        {
            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            RemoveLinkedAccount(account, provider);
            Update(account);
        }
        protected virtual void RemoveLinkedAccount(T account, string provider)
        {
            Tracing.Information("[UserAccountService.RemoveLinkedAccount] called for accountID: {0}", account.ID);

            var linked = account.LinkedAccounts.Where(x => x.ProviderName == provider);
            foreach (var item in linked)
            {
                account.LinkedAccounts.Remove(item);
                this.AddEvent(new LinkedAccountRemovedEvent<T> { Account = account, LinkedAccount = item });
            }
        }

        public virtual void RemoveLinkedAccount(Guid accountID, string provider, string id)
        {
            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            RemoveLinkedAccount(account, provider, id);
            Update(account);
        }
        protected virtual void RemoveLinkedAccount(T account, string provider, string id)
        {
            Tracing.Information("[UserAccountService.RemoveLinkedAccount] called for accountID: {0}", account.ID);

            var linked = GetLinkedAccount(account, provider, id);
            if (linked != null)
            {
                account.LinkedAccounts.Remove(linked);
                this.AddEvent(new LinkedAccountRemovedEvent<T> { Account = account, LinkedAccount = linked });
            }
        }

        public virtual void AddCertificate(Guid accountID, X509Certificate2 certificate)
        {
            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            AddCertificate(account, certificate);
            Update(account);
        }
        protected virtual void AddCertificate(T account, X509Certificate2 certificate)
        {
            Tracing.Information("[UserAccountService.AddCertificate] called for accountID: {0}", account.ID);

            certificate.Validate();
            RemoveCertificate(account, certificate);
            AddCertificate(account, certificate.Thumbprint, certificate.Subject);
        }

        public virtual void AddCertificate(Guid accountID, string thumbprint, string subject)
        {
            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            AddCertificate(account, thumbprint, subject);
            Update(account);
        }
        protected virtual void AddCertificate(T account, string thumbprint, string subject)
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

            this.AddEvent(new CertificateAddedEvent<T> { Account = account, Certificate = cert });
        }

        public virtual void RemoveCertificate(Guid accountID, X509Certificate2 certificate)
        {
            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            RemoveCertificate(account, certificate);
            Update(account);
        }
        protected virtual void RemoveCertificate(T account, X509Certificate2 certificate)
        {
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
            var account = this.GetByID(accountID);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            RemoveCertificate(account, thumbprint);
            Update(account);
        }
        protected virtual void RemoveCertificate(T account, string thumbprint)
        {
            Tracing.Information("[UserAccountService.RemoveCertificate] called for accountID: {0}", account.ID);

            if (String.IsNullOrWhiteSpace(thumbprint))
            {
                Tracing.Error("[UserAccountService.RemoveCertificate] failed -- no thumbprint");
                throw new ArgumentNullException("thumbprint");
            }

            var certs = account.Certificates.Where(x => x.Thumbprint.Equals(thumbprint, StringComparison.OrdinalIgnoreCase)).ToArray();
            foreach (var cert in certs)
            {
                this.AddEvent(new CertificateRemovedEvent<T> { Account = account, Certificate = cert });
                account.Certificates.Remove(cert);
            }

            if (!account.Certificates.Any() &&
                account.AccountTwoFactorAuthMode == TwoFactorAuthMode.Certificate)
            {
                Tracing.Verbose("[UserAccountService.RemoveCertificate] last cert removed, disabling two factor auth");
                ConfigureTwoFactorAuthentication(account, TwoFactorAuthMode.None);
            }
        }

        internal virtual void CreateTwoFactorAuthToken(T account)
        {
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

            this.AddEvent(new TwoFactorAuthenticationTokenCreatedEvent<T> { Account = account, Token = value });
        }

        internal virtual bool VerifyTwoFactorAuthToken(T account, string token)
        {
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

        internal virtual void RemoveTwoFactorAuthTokens(T account)
        {
            Tracing.Information("[UserAccountService.RemoveTwoFactorAuthTokens] called for accountID: {0}", account.ID);

            foreach (var item in account.TwoFactorAuthTokens.ToArray())
            {
                account.TwoFactorAuthTokens.Remove(item);
            }
        }

        protected internal virtual DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }

        static readonly string[] UglyBase64 = { "+", "/", "=" };
        internal static string StripUglyBase64(string s)
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
