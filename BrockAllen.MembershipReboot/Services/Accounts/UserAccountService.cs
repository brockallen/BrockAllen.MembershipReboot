using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Helpers;

namespace BrockAllen.MembershipReboot
{
    public class UserAccountService : IDisposable
    {
        IUserAccountRepository userRepository;
        NotificationService notificationService;
        IPasswordPolicy passwordPolicy;

        public UserAccountService(
            IUserAccountRepository userAccountRepository,
            NotificationService notificationService,
            IPasswordPolicy passwordPolicy)
        {
            this.userRepository = userAccountRepository;
            this.notificationService = notificationService;
            this.passwordPolicy = passwordPolicy;
        }

        public void Dispose()
        {
            this.userRepository.Dispose();
        }

        public virtual bool UsernameExists(string username)
        {
            return this.userRepository.GetAll().Where(x => x.Username == username).Any();
        }

        public virtual bool EmailExists(string email)
        {
            return this.userRepository.GetAll().Where(x => x.Email == email).Any();
        }

        public virtual void CreateAccount(string username, string password, string email)
        {
            if (SecuritySettings.Instance.EmailIsUsername)
            {
                username = email;
            }

            if (String.IsNullOrWhiteSpace(username)) throw new ArgumentException("username");
            if (String.IsNullOrWhiteSpace(password)) throw new ArgumentException("password");
            if (String.IsNullOrWhiteSpace(email)) throw new ArgumentException("email");

            ValidatePassword(password);

            EmailAddressAttribute validator = new EmailAddressAttribute();
            if (!validator.IsValid(email))
            {
                throw new ValidationException("Email is invalid.");
            }

            if ((!SecuritySettings.Instance.EmailIsUsername && UsernameExists(username)) 
                || EmailExists(email))
            {
                throw new ValidationException("Username/Email already in use.");
            }
            
            using (var tx = new TransactionScope())
            {
                var account = UserAccount.Create(username, password, email);
                this.userRepository.Add(account);
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
                this.userRepository.SaveChanges();
                tx.Complete();
            }
        }

        public virtual bool VerifyAccount(string key)
        {
            var account = this.userRepository.GetByVerificationKey(key);
            if (account == null) return false;

            var result = account.VerifyAccount(key);
            using (var tx = new TransactionScope())
            {
                if (result && this.notificationService != null)
                {
                    this.notificationService.SendAccountVerified(account);
                }
                this.userRepository.SaveChanges();
                tx.Complete();
            }
            return result;
        }

        public virtual bool CancelNewAccount(string key)
        {
            var account = this.userRepository.GetByVerificationKey(key);
            if (account == null) return false;

            if (account.IsAccountVerified) return false;
            if (account.VerificationKey != key) return false;

            DeleteAccount(account);

            return true;
        }

        public virtual bool DeleteAccount(string username)
        {
            var account = this.userRepository.GetByUsername(username);
            if (account == null) return false;

            DeleteAccount(account);

            return true;
        }

        private void DeleteAccount(UserAccount account)
        {
            if (SecuritySettings.Instance.AllowAccountDeletion || !account.IsAccountVerified)
            {
                this.userRepository.Remove(account);
            }
            else
            {
                account.IsLoginAllowed = false;
                account.IsAccountClosed = true;
            }

            using (var tx = new TransactionScope())
            {
                if (this.notificationService != null)
                {
                    this.notificationService.SendAccountDelete(account);
                }
                this.userRepository.SaveChanges();
                tx.Complete();
            }
        }

        public virtual bool Authenticate(string username, string password)
        {
            return Authenticate(
                username, password, 
                SecuritySettings.Instance.AccountLockoutFailedLoginAttempts, 
                SecuritySettings.Instance.AccountLockoutDuration);
        }

        public virtual bool Authenticate(
            string username, string password, 
            int failedLoginCount, TimeSpan lockoutDuration)
        {
            if (String.IsNullOrWhiteSpace(username)) return false;

            var account = this.userRepository.GetByUsername(username);
            if (account == null) return false;

            var result = account.Authenticate(password, failedLoginCount, lockoutDuration);
            this.userRepository.SaveChanges();
            return result;
        }

        public virtual bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            return ChangePassword(
                username, oldPassword, newPassword,
                SecuritySettings.Instance.AccountLockoutFailedLoginAttempts,
                SecuritySettings.Instance.AccountLockoutDuration);
        }

        public virtual bool ChangePassword(
            string username, string oldPassword, string newPassword, 
            int failedLoginCount, TimeSpan lockoutDuration)
        {
            if (String.IsNullOrWhiteSpace(username)) return false;

            ValidatePassword(newPassword);

            var account = this.userRepository.GetByUsername(username);
            if (account == null) return false;

            var result = account.ChangePassword(oldPassword, newPassword, failedLoginCount, lockoutDuration);
            using (var tx = new TransactionScope())
            {
                if (result && this.notificationService != null)
                {
                    this.notificationService.SendPasswordChangeNotice(account);
                }
                this.userRepository.SaveChanges();
                tx.Complete();
            }
            return result;
        }

        public virtual bool ResetPassword(string email)
        {
            if (String.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            var account = this.userRepository.GetByEmail(email);
            if (account == null) return false;

            var result = account.ResetPassword(email);
            using (var tx = new TransactionScope())
            {
                if (result && this.notificationService != null)
                {
                    this.notificationService.SendResetPassword(account);
                }
                this.userRepository.SaveChanges();
                tx.Complete();
            }

            return result;
        }
        
        public virtual bool ChangePasswordFromResetKey(string key, string newPassword)
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            var account = this.userRepository.GetByVerificationKey(key);
            if (account == null) return false;

            ValidatePassword(newPassword);

            var result = account.ChangePasswordFromResetKey(key, newPassword);
            using (var tx = new TransactionScope())
            {
                if (result && this.notificationService != null)
                {
                    this.notificationService.SendPasswordChangeNotice(account);
                }
                this.userRepository.SaveChanges();
                tx.Complete();
            }
            return result;
        }

        public virtual void SendUsernameReminder(string email)
        {
            if (String.IsNullOrWhiteSpace(email)) return;

            var user = this.userRepository.GetByEmail(email);
            if (user != null)
            {
                this.notificationService.SendAccountNameReminder(user);
            }
        }
        
        private void ValidatePassword(string password)
        {
            if (passwordPolicy != null &&
                !passwordPolicy.ValidatePassword(password))
            {
                throw new ValidationException("Invalid password: " + passwordPolicy.PolicyMessage);
            }
        }
    }
}
