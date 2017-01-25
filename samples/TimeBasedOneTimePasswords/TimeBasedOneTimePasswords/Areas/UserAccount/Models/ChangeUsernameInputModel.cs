using System.ComponentModel.DataAnnotations;

namespace TimeBasedOneTimePasswords.Areas.UserAccount.Models
{
    public class ChangeUsernameInputModel
    {
        [Required]
        public string NewUsername { get; set; }
    }
}