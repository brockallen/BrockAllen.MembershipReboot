using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace BrockAllen.MembershipReboot
{
    public class UserAccountService
    {
        const int DefaultFailedLoginCount = 10;
        static readonly TimeSpan DefaultLockoutDuration = TimeSpan.FromMinutes(5);

        IUserAccountRepository userRepository;
        NotificationService notificationService;
        public UserAccountService(IUserAccountRepository userRepository, NotificationService notificationService)
        {
            this.userRepository = userRepository;
            this.notificationService = notificationService;
        }

        static readonly string[] UglyBase64 = {"+", "/", "="};
        string StripUglyBase64(string s)
        {
            if (s == null) return s;
            foreach (var ugly in UglyBase64)
            {
                s = s.Replace(ugly, "");
            }
            return s;
        }

        /// <summary>
        /// Useful in scenarios where user interface allows user to specify username and immediately tells them whether it is available.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public virtual bool CheckUsernameExists(string username)
        {
            if (String.IsNullOrWhiteSpace(username)) throw new ArgumentException("username");

            var account = this.userRepository.GetByUsername(username);
            return account != null;
        }

        public virtual void CreateAccount(string username, string password, string email)
        {
            if (String.IsNullOrWhiteSpace(username)) throw new ArgumentException("username");
            if (String.IsNullOrWhiteSpace(password)) throw new ArgumentException("password");
            if (String.IsNullOrWhiteSpace(email)) throw new ArgumentException("email");

            EmailAddressAttribute validator = new EmailAddressAttribute();
            if (!validator.IsValid(email))
            {
                throw new ValidationException("Email is invalid.");
            }

            var usernameExists = this.userRepository.GetByUsername(username);
            var emailExists = this.userRepository.GetByEmail(email);
            if (usernameExists != null || emailExists != null)
            {
                throw new ValidationException("Username or Email already in use.");
            }

            var hashedPassword = Crypto.HashPassword(password);
            var now = DateTime.UtcNow;
            var key = StripUglyBase64(Crypto.GenerateSalt());

            UserAccount account = new UserAccount
            {
                Username = username,
                HashedPassword = hashedPassword,
                Email = email,
                Created = now,
                PasswordChanged = now,
                ResetKey = key,
                ResetKeySent = null, // null in this field is used to indicate a new account was created
                LastLogin = null,
                FailedLoginCount = null,
                LastFailedLogin = null
            };
            this.userRepository.Create(account);
            this.notificationService.SendAccountCreate(account);
        }

        public virtual bool VerifyAccountCreate(string username, string key)
        {
            if (String.IsNullOrWhiteSpace(username) ||
                String.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            var account = this.userRepository.GetByUsername(username);
            if (account == null) return false;

            if (account.ResetKeySent != null) return false;
            if (account.ResetKey != key) return false;

            account.ResetKey = null;
            account.ResetKeySent = null;
            this.userRepository.Update(account);
            this.notificationService.SendAccountVerified(account);

            return true;
        }
        
        public virtual bool CancelAccountCreate(string key)
        {
            if (String.IsNullOrWhiteSpace(key)) return false;

            var account = this.userRepository.GetByResetKey(key);
            if (account == null) return false;

            if (account.ResetKeySent != null) return false;
            if (account.ResetKey != key) return false;

            this.userRepository.Delete(account);

            return true;
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

        public virtual bool ResetPassword(string username, string email)
        {
            if (String.IsNullOrWhiteSpace(username) ||
                String.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            var account = this.userRepository.GetByUsername(username);
            if (account == null) return false;

            if (!account.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)) return false;

            account.ResetKey = StripUglyBase64(Crypto.GenerateSalt());
            account.ResetKeySent = DateTime.UtcNow;
            
            this.userRepository.Update(account);

            this.notificationService.SendResetPassword(account);

            return true;
        }
        
        public virtual bool CancelResetPassword(string key)
        {
            if (String.IsNullOrWhiteSpace(key)) return false;

            var account = this.userRepository.GetByResetKey(key);
            if (account == null) return false;

            account.ResetKey = null;
            account.ResetKeySent = null;

            this.userRepository.Update(account);

            return true;
        }

        public virtual bool ChangePasswordFromResetKey(string username, string key, string newPassword)
        {
            if (String.IsNullOrWhiteSpace(username) ||
                String.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            var account = this.userRepository.GetByUsername(username);
            if (account == null) return false;

            if (account.ResetKey != key || account.ResetKeySent == null) return false;

            return ChangePassword(account, newPassword);
        }

        public virtual bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            return ChangePassword(username, oldPassword, newPassword, DefaultFailedLoginCount, DefaultLockoutDuration);
        }

        public virtual bool ChangePassword(string username, string oldPassword, string newPassword, int failedLoginCount, TimeSpan lockoutDuration)
        {
            if (String.IsNullOrWhiteSpace(username)) return false;

            var account = this.userRepository.GetByUsername(username);
            if (account == null) return false;
            
            if (Authenticate(account, oldPassword, failedLoginCount, lockoutDuration))
            {
                return ChangePassword(account, newPassword);
            }

            return false;
        }

        bool ChangePassword(UserAccount account, string newPassword)
        {
            if (account == null ||
                String.IsNullOrWhiteSpace(newPassword))
            {
                return false;
            }

            var hashedPassword = Crypto.HashPassword(newPassword);

            account.HashedPassword = hashedPassword;
            account.PasswordChanged = DateTime.UtcNow;
            account.ResetKey = null;
            account.ResetKeySent = null;
            
            this.userRepository.Update(account);

            this.notificationService.SendPasswordChangeNotice(account);

            return true;
        }
        
        public virtual bool Authenticate(string username, string password)
        {
            return Authenticate(username, password, DefaultFailedLoginCount, DefaultLockoutDuration);
        }

        public virtual bool Authenticate(string username, string password, int failedLoginCount, TimeSpan lockoutDuration)
        {
            if (String.IsNullOrWhiteSpace(username)) return false;

            var account = this.userRepository.GetByUsername(username);
            if (account == null) return false;

            return Authenticate(account, password, failedLoginCount, lockoutDuration);
        }
        
        protected virtual bool Authenticate(UserAccount account, string password, int failedLoginCount, TimeSpan lockoutDuration)
        {
            if (failedLoginCount <= 0) throw new ArgumentException("failedLoginCount");

            if (account == null || 
                String.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            // if account is not verified then don't allow in
            if (account.ResetKey != null && 
                account.ResetKeySent == null)
            {
                return false;
            }

            if (failedLoginCount <= account.FailedLoginCount &&
                account.LastFailedLogin <= DateTime.UtcNow.Add(lockoutDuration))
            {
                account.FailedLoginCount = account.FailedLoginCount + 1;
                this.userRepository.Update(account);

                // if we get ten times the allowed guesses within the time limit
                // then let them know again
                if (account.FailedLoginCount == failedLoginCount * 10)
                {
                    //this.notificationService.SendFailedLogin(account);
                }

                return false;
            }

            var valid = Crypto.VerifyHashedPassword(account.HashedPassword, password);
            if (valid)
            {
                account.LastLogin = DateTime.UtcNow;
                account.FailedLoginCount = 0;
                this.userRepository.Update(account);
            }
            else
            {
                account.LastFailedLogin = DateTime.UtcNow;
                if (account.FailedLoginCount > 0) account.FailedLoginCount = account.FailedLoginCount + 1;
                else account.FailedLoginCount = account.FailedLoginCount = 1;
                
                this.userRepository.Update(account);

                if (account.FailedLoginCount == failedLoginCount)
                {
                    //this.notificationService.SendFailedLogin(account);
                }
            }

            return valid;
        }
    }
}
