using System.ComponentModel.DataAnnotations;

namespace TimeBasedOneTimePasswords.Areas.UserAccount.Models
{
    public class TwoFactorAuthInputModel
    {
        [Required]
        public string Code { get; set; }

        public string ReturnUrl { get; set; }
    }
}