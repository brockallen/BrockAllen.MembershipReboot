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
        [Key]
        public string Username { get; set; }
        [Required]
        public string HashedPassword { get; set; }
        [EmailAddress]
        public string Email { get; set; }

        public DateTime Created { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime PasswordChanged { get; set; }
        
        public int? FailedLoginCount { get; set; }
        public DateTime? LastFailedLogin { get; set; }
        
        public string ResetKey { get; set; }
        public DateTime? ResetKeySent { get; set; }

        public string GetLockString()
        {
            var val =
                this.Username +
                this.HashedPassword + 
                this.PasswordChanged.ToString("s");
            return Crypto.Hash(val);
        }
        
        public bool VerifyLockString(string s)
        {
            var val =
                this.Username +
                this.HashedPassword + 
                this.PasswordChanged.ToString("s");
            return Crypto.Hash(val) == s;
        }
    }
}
