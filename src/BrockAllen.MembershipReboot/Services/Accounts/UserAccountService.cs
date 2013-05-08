using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Transactions;

namespace BrockAllen.MembershipReboot
{
    public class UserAccountService : IDisposable
    {
        IUserAccountRepository userRepository;
        INotificationService notificationService;
        IPasswordPolicy passwordPolicy;

        public UserAccountService(
            IUserAccountRepository userAccountRepository,
            INotificationService notificationService,
            IPasswordPolicy passwordPolicy)
        {
            if (userAccountRepository == null) throw new ArgumentNullException("userAccountRepository");

            this.userRepository = userAccountRepository;
            this.notificationService = notificationService;
            this.passwordPolicy = passwordPolicy;
        }

        public void Dispose()
        {
            if (this.userRepository.TryDispose())
            {
                this.userRepository = null;
            }
        }

        public virtual void SaveChanges()
        {
            this.userRepository.SaveChanges();
        }

        public virtual IQueryable<UserAccount> GetAll()
        {
            return GetAll(null);
        }

        public virtual IQueryable<UserAccount> GetAll(string tenant)
        {
            if (!SecuritySettings.Instance.MultiTenant)
            {
                tenant = SecuritySettings.Instance.DefaultTenant;
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
            if (!SecuritySettings.Instance.MultiTenant)
            {
                tenant = SecuritySettings.Instance.DefaultTenant;
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
            if (!SecuritySettings.Instance.MultiTenant)
            {
                tenant = SecuritySettings.Instance.DefaultTenant;
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

        public virtual UserAccount GetByID(int id)
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
        
        public virtual UserAccount GetByNameId(Guid nameId)
        {
            var account = userRepository.GetAll().Where(x => x.NameID == nameId).SingleOrDefault();
            if (account == null)
            {
                Tracing.Verbose(String.Format("[UserAccountService.GetByNameId] failed to locate account: {0}", nameId));
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

            if (SecuritySettings.Instance.UsernamesUniqueAcrossTenants)
            {
                return this.userRepository.GetAll().Where(x => x.Username == username).Any();
            }
            else
            {
                if (!SecuritySettings.Instance.MultiTenant)
                {
                    tenant = SecuritySettings.Instance.DefaultTenant;
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
            if (!SecuritySettings.Instance.MultiTenant)
            {
                tenant = SecuritySettings.Instance.DefaultTenant;
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

            if (SecuritySettings.Instance.EmailIsUsername)
            {
                username = email;
            }

            if (!SecuritySettings.Instance.MultiTenant)
            {
                tenant = SecuritySettings.Instance.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) throw new ArgumentException("tenant");
            if (String.IsNullOrWhiteSpace(username)) throw new ArgumentException("username");
            if (String.IsNullOrWhiteSpace(password)) throw new ArgumentException("password");
            if (String.IsNullOrWhiteSpace(email)) throw new ArgumentException("email");

            ValidatePassword(tenant, username, password);

            EmailAddressAttribute validator = new EmailAddressAttribute();
            if (!validator.IsValid(email))
            {
                Tracing.Verbose(String.Format("[UserAccountService.CreateAccount] Email validation failed: {0}, {1}, {2}", tenant, username, email));

                throw new ValidationException("Email is invalid.");
            }

            if (EmailExists(tenant, email))
            {
                Tracing.Verbose(String.Format("[UserAccountService.CreateAccount] Email already exists: {0}, {1}, {2}", tenant, username, email));

                throw new ValidationException("Email already in use.");
            }
            
            if (UsernameExists(tenant, username))
            {
                Tracing.Verbose(String.Format("[UserAccountService.CreateAccount] Username already exists: {0}, {1}", tenant, username));
                
                throw new ValidationException("Username already in use.");
            }

            using (var tx = new TransactionScope())
            {
                var account = new UserAccount(tenant, username, password, email);
                this.userRepository.Add(account);
                this.userRepository.SaveChanges();

                if (this.notificationService != null)
                {
                    if (SecuritySettings.Instance.RequireAccountVerification)
                    {
                        this.notificationService.SendAccountCreate(account);
                    }
                    else
                    {
                        this.notificationService.SendAccountVerified(account);
                    }
                }

                tx.Complete();

                return account;
            }
        }

        protected internal virtual void ValidatePassword(string tenant, string username, string password)
        {
            if (passwordPolicy != null)
            {
                if (!passwordPolicy.ValidatePassword(password))
                {
                    Tracing.Verbose(String.Format("[ValidatePassword] Failed: {0}, {1}, {2}", tenant, username, passwordPolicy.PolicyMessage));

                    throw new ValidationException("Invalid password: " + passwordPolicy.PolicyMessage);
                }
            }
        }

        public virtual bool VerifyAccount(string key)
        {
            Tracing.Information(String.Format("[UserAccountService.VerifyAccount] called: {0}", key));

            var account = this.GetByVerificationKey(key);
            if (account == null) return false;

            Tracing.Verbose(String.Format("[UserAccountService.VerifyAccount] account located: {0}, {1}", account.Tenant, account.Username));

            var result = account.VerifyAccount(key);
            using (var tx = new TransactionScope())
            {
                this.userRepository.SaveChanges();

                if (result && this.notificationService != null)
                {
                    this.notificationService.SendAccountVerified(account);
                }

                tx.Complete();
            }
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

            if (!SecuritySettings.Instance.MultiTenant)
            {
                tenant = SecuritySettings.Instance.DefaultTenant;
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
            if (SecuritySettings.Instance.AllowAccountDeletion || !account.IsAccountVerified)
            {
                Tracing.Verbose(String.Format("[UserAccountService.DeleteAccount] removing account record: {0}, {1}", account.Tenant, account.Username));
                this.userRepository.Remove(account);
            }
            else
            {
                Tracing.Verbose(String.Format("[UserAccountService.DeleteAccount] marking account closed: {0}, {1}", account.Tenant, account.Username));
                account.CloseAccount();
            }

            using (var tx = new TransactionScope())
            {
                this.userRepository.SaveChanges();

                if (this.notificationService != null)
                {
                    this.notificationService.SendAccountDelete(account);
                }

                tx.Complete();
            }
        }

        public virtual bool Authenticate(string username, string password)
        {
            return Authenticate(null, username, password);
        }

        public virtual bool Authenticate(string tenant, string username, string password)
        {
            return Authenticate(
                tenant, username, password,
                SecuritySettings.Instance.AccountLockoutFailedLoginAttempts,
                SecuritySettings.Instance.AccountLockoutDuration);
        }

        public virtual bool Authenticate(
            string username, string password,
            int failedLoginCount, TimeSpan lockoutDuration)
        {
            return Authenticate(null, username, password, failedLoginCount, lockoutDuration);
        }

        public virtual bool Authenticate(
            string tenant, string username, string password,
            int failedLoginCount, TimeSpan lockoutDuration)
        {
            Tracing.Information(String.Format("[UserAccountService.Authenticate] called: {0}, {1}", tenant, username));

            if (!SecuritySettings.Instance.MultiTenant)
            {
                tenant = SecuritySettings.Instance.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) return false;
            if (String.IsNullOrWhiteSpace(username)) return false;
            if (String.IsNullOrWhiteSpace(password)) return false;

            var account = this.GetByUsername(tenant, username);
            if (account == null) return false;

            return Authenticate(account, password, failedLoginCount, lockoutDuration);
        }

        protected internal virtual bool Authenticate(UserAccount account, string password, int failedLoginCount, TimeSpan lockoutDuration)
        {
            var result = account.Authenticate(password, failedLoginCount, lockoutDuration);
            this.userRepository.SaveChanges();

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

            if (!SecuritySettings.Instance.MultiTenant)
            {
                tenant = SecuritySettings.Instance.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) throw new ValidationException("Invalid tenant.");
            if (String.IsNullOrWhiteSpace(username)) throw new ValidationException("Invalid username.");
            if (String.IsNullOrWhiteSpace(newPassword)) throw new ValidationException("Invalid newPassword.");

            ValidatePassword(tenant, username, newPassword);

            var account = this.GetByUsername(tenant, username);
            if (account == null) throw new ValidationException("Invalid tenant and/or username.");

            Tracing.Information(String.Format("[UserAccountService.SetPassword] setting new password for: {0}, {1}", tenant, username));

            using (var tx = new TransactionScope())
            {
                account.SetPassword(newPassword);
                this.userRepository.SaveChanges();
                
                if (this.notificationService != null)
                {
                    this.notificationService.SendPasswordChangeNotice(account);
                }
                
                tx.Complete();
            }
        }

        public virtual bool ChangePassword(
            string username, string oldPassword, string newPassword)
        {
            return ChangePassword(null, username, oldPassword, newPassword);
        }

        public virtual bool ChangePassword(
            string tenant, string username,
            string oldPassword, string newPassword)
        {
            return ChangePassword(
                tenant, username,
                oldPassword, newPassword,
                SecuritySettings.Instance.AccountLockoutFailedLoginAttempts,
                SecuritySettings.Instance.AccountLockoutDuration);
        }

        public virtual bool ChangePassword(
            string username,
            string oldPassword, string newPassword,
            int failedLoginCount, TimeSpan lockoutDuration)
        {
            return ChangePassword(null, username, oldPassword, newPassword, failedLoginCount, lockoutDuration);
        }

        public virtual bool ChangePassword(
            string tenant, string username,
            string oldPassword, string newPassword,
            int failedLoginCount, TimeSpan lockoutDuration)
        {
            Tracing.Information(String.Format("[UserAccountService.ChangePassword] called: {0}, {1}", tenant, username));

            if (!SecuritySettings.Instance.MultiTenant)
            {
                tenant = SecuritySettings.Instance.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) return false;
            if (String.IsNullOrWhiteSpace(username)) return false;
            if (String.IsNullOrWhiteSpace(oldPassword)) return false;
            if (String.IsNullOrWhiteSpace(newPassword)) return false;

            ValidatePassword(tenant, username, newPassword);

            var account = this.GetByUsername(tenant, username);
            if (account == null) return false;

            bool result = false;
            try
            {
                result = account.ChangePassword(oldPassword, newPassword, failedLoginCount, lockoutDuration);
                Tracing.Verbose(String.Format("[UserAccountService.ChangePassword] change password outcome: {0}, {1}, {2}", account.Tenant, account.Username, result ? "Successful" : "Failed"));
            }
            finally
            {
                // put this into finally since ChangePassword uses Authenticate which modifies state
                using (var tx = new TransactionScope())
                {
                    this.userRepository.SaveChanges();

                    if (result && this.notificationService != null)
                    {
                        this.notificationService.SendPasswordChangeNotice(account);
                    }

                    tx.Complete();
                }
            }
            return result;
        }

        public virtual bool ResetPassword(string email)
        {
            return ResetPassword(null, email);
        }

        public virtual bool ResetPassword(string tenant, string email)
        {
            Tracing.Information(String.Format("[UserAccountService.ResetPassword] called: {0}, {1}", tenant, email));

            if (!SecuritySettings.Instance.MultiTenant)
            {
                tenant = SecuritySettings.Instance.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) return false;
            if (String.IsNullOrWhiteSpace(email)) return false;

            var account = this.GetByEmail(tenant, email);
            if (account == null) return false;

            if (!account.IsAccountVerified)
            {
                // if they're not verified, resend the new account email
                if (SecuritySettings.Instance.RequireAccountVerification &&
                    this.notificationService != null)
                {
                    Tracing.Verbose(String.Format("[UserAccountService.ResetPassword] account not verified, re-sending account create notification: {0}, {1}", account.Tenant, account.Username));

                    this.notificationService.SendAccountCreate(account);
                    return true;
                }

                // if we don't have a notification system then not much we can do
                Tracing.Warning(String.Format("[UserAccountService.ResetPassword] account not verified, no notification to re-send invite: {0}, {1}", account.Tenant, account.Username));

                return false;
            }

            var result = account.ResetPassword();

            Tracing.Verbose(String.Format("[UserAccountService.ResetPassword] reset password outcome: {0}, {1}, {2}", account.Tenant, account.Username, result ? "Successful" : "Failed"));

            if (result)
            {
                using (var tx = new TransactionScope())
                {
                    this.userRepository.SaveChanges();

                    if (this.notificationService != null)
                    {
                        this.notificationService.SendResetPassword(account);
                    }

                    tx.Complete();
                }
            }
            return result;
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

            ValidatePassword(account.Tenant, account.Username, newPassword);

            var result = account.ChangePasswordFromResetKey(key, newPassword);

            Tracing.Verbose(String.Format("[UserAccountService.ChangePasswordFromResetKey] change password outcome: {0}, {1}, {2}", account.Tenant, account.Username, result ? "Successful" : "Failed"));

            if (result)
            {
                using (var tx = new TransactionScope())
                {
                    this.userRepository.SaveChanges();

                    if (this.notificationService != null)
                    {
                        this.notificationService.SendPasswordChangeNotice(account);
                    }

                    tx.Complete();
                }
            }
            return result;
        }

        public virtual void SendUsernameReminder(string email)
        {
            SendUsernameReminder(null, email);
        }

        public virtual void SendUsernameReminder(string tenant, string email)
        {
            if (this.notificationService == null)
            {
                throw new InvalidOperationException("NotificationService not configured.");
            }

            Tracing.Information(String.Format("[UserAccountService.SendUsernameReminder] called: {0}, {1}", tenant, email));

            if (!SecuritySettings.Instance.MultiTenant)
            {
                tenant = SecuritySettings.Instance.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) return;
            if (String.IsNullOrWhiteSpace(email)) return;

            var account = this.GetByEmail(tenant, email);
            if (account != null)
            {
                Tracing.Verbose(String.Format("[UserAccountService.SendUsernameReminder] account located: {0}, {1}", account.Tenant, account.Username));
                    
                this.notificationService.SendAccountNameReminder(account);
            }
        }

        public virtual void ChangeUsername(string username, string newUsername)
        {
            ChangeUsername(null, username, newUsername);
        }

        public virtual void ChangeUsername(string tenant, string username, string newUsername)
        {
            if (SecuritySettings.Instance.EmailIsUsername)
            {
                throw new Exception("EmailIsUsername is enabled in SecuritySettings -- use ChangeEmail APIs instead.");
            }

            Tracing.Information(String.Format("[UserAccountService.ChangeUsername] called: {0}, {1}, {2}", tenant, username, newUsername));

            if (!SecuritySettings.Instance.MultiTenant)
            {
                tenant = SecuritySettings.Instance.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) throw new ArgumentException("tenant");
            if (String.IsNullOrWhiteSpace(username)) throw new ArgumentException("username");
            if (String.IsNullOrWhiteSpace(newUsername)) throw new ArgumentException("newUsername");

            var account = GetByUsername(tenant, username);
            if (account == null) throw new ValidationException("Invalid account");

            if (UsernameExists(tenant, newUsername))
            {
                Tracing.Information(String.Format("[UserAccountService.ChangeUsername] failed because new username already in use: {0}, {1}, {2}", tenant, username, newUsername));
                throw new ValidationException("Username is already in use.");
            }

            Tracing.Information(String.Format("[UserAccountService.ChangeUsername] changing username: {0}, {1}, {2}", tenant, username, newUsername));

            account.Username = newUsername;

            using (var tx = new TransactionScope())
            {
                this.userRepository.SaveChanges();

                if (this.notificationService != null)
                {
                    this.notificationService.SendChangeUsernameRequestNotice(account);
                }

                tx.Complete();
            }
        }

        public virtual bool ChangeEmailRequest(string username, string newEmail)
        {
            return ChangeEmailRequest(null, username, newEmail);
        }

        public virtual bool ChangeEmailRequest(string tenant, string username, string newEmail)
        {
            Tracing.Information(String.Format("[UserAccountService.ChangeEmailRequest] called: {0}, {1}, {2}", tenant, username, newEmail));

            if (!SecuritySettings.Instance.MultiTenant)
            {
                tenant = SecuritySettings.Instance.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) return false;
            if (String.IsNullOrWhiteSpace(username)) return false;
            if (String.IsNullOrWhiteSpace(newEmail)) return false;

            EmailAddressAttribute validator = new EmailAddressAttribute();
            if (!validator.IsValid(newEmail))
            {
                Tracing.Verbose(String.Format("[UserAccountService.ChangeEmailRequest] email validation failed: {0}, {1}, {2}", tenant, username, newEmail));

                throw new ValidationException("Email is invalid.");
            }

            var account = this.GetByUsername(tenant, username);
            if (account == null) return false;

            Tracing.Verbose(String.Format("[UserAccountService.ChangeEmailRequest] account located: {0}, {1}", account.Tenant, account.Username));

            if (EmailExists(tenant, newEmail))
            {
                Tracing.Verbose(String.Format("[UserAccountService.ChangeEmailRequest] Email already exists: {0}, {1}, new email: {2}", tenant, username, newEmail));

                throw new ValidationException("Email already in use.");
            }

            var result = account.ChangeEmailRequest(newEmail);

            Tracing.Verbose(String.Format("[UserAccountService.ChangeEmailRequest] change request outcome: {0}, {1}, {2}", account.Tenant, account.Username, result ? "Successful" : "Failed"));

            if (result)
            {
                using (var tx = new TransactionScope())
                {
                    this.userRepository.SaveChanges();

                    if (this.notificationService != null)
                    {
                        this.notificationService.SendChangeEmailRequestNotice(account, newEmail);
                    }

                    tx.Complete();
                }
            }

            return result;
        }

        public virtual bool ChangeEmailFromKey(string password, string key, string newEmail)
        {
            return ChangeEmailFromKey(
                password, key, newEmail,
                SecuritySettings.Instance.AccountLockoutFailedLoginAttempts,
                SecuritySettings.Instance.AccountLockoutDuration);
        }

        public virtual bool ChangeEmailFromKey(
            string password, string key, string newEmail,
            int failedLoginCount, TimeSpan lockoutDuration)
        {
            Tracing.Information(String.Format("[UserAccountService.ChangeEmailFromKey] called: {0}, {1}", key, newEmail));

            if (String.IsNullOrWhiteSpace(password)) return false;
            if (String.IsNullOrWhiteSpace(key)) return false;
            if (String.IsNullOrWhiteSpace(newEmail)) return false;

            var account = this.GetByVerificationKey(key);
            if (account == null) return false;

            Tracing.Verbose(String.Format("[UserAccountService.ChangeEmailFromKey] account located: {0}, {1}", account.Tenant, account.Username));

            if (!Authenticate(account, password, failedLoginCount, lockoutDuration))
            {
                return false;
            }

            var oldEmail = account.Email;
            var result = account.ChangeEmailFromKey(key, newEmail);
            
            if (result && SecuritySettings.Instance.EmailIsUsername)
            {
                Tracing.Warning(String.Format("[UserAccountService.ChangeEmailFromKey] security setting EmailIsUsername is true and AllowEmailChangeWhenEmailIsUsername is true, so changing username: {0}, to: {1}", account.Username, newEmail));
                account.Username = newEmail;
            }

            Tracing.Verbose(String.Format("[UserAccountService.ChangeEmailFromKey] change email outcome: {0}, {1}, {2}", account.Tenant, account.Username, result ? "Successful" : "Failed"));

            if (result)
            {
                using (var tx = new TransactionScope())
                {
                    this.userRepository.SaveChanges();

                    if (this.notificationService != null)
                    {
                        this.notificationService.SendEmailChangedNotice(account, oldEmail);
                    }

                    tx.Complete();
                }
            }
            return result;
        }

        public virtual bool IsPasswordExpired(string username)
        {
            return IsPasswordExpired(null, username);
        }

        public virtual bool IsPasswordExpired(string tenant, string username)
        {
            if (!SecuritySettings.Instance.MultiTenant)
            {
                tenant = SecuritySettings.Instance.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) return false;
            if (String.IsNullOrWhiteSpace(username)) return false;

            var account = this.GetByUsername(tenant, username);
            if (account == null) return false;

            return account.IsPasswordExpired;
        }
    }
}
