using System.ComponentModel.DataAnnotations;
using BrockAllen.MembershipReboot;

namespace TimeBasedOneTimePasswords.Areas.UserAccount.Models
{
    public class PasswordResetSecretsViewModel
    {
        public PasswordResetSecret[] Secrets { get; set; }
    }

    public class AddSecretQuestionInputModel
    {
        [Required]
        public string Question { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Answer { get; set; }
    }
}