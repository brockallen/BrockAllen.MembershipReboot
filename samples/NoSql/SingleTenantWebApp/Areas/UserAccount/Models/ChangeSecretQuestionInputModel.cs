using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models
{
    public class PasswordResetSecretsViewModel
    {
        public PasswordResetSecret[] Secrets { get; set; }
    }

    public class AddSecretQuestionInputModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        public string Question { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Answer { get; set; }
    }
}