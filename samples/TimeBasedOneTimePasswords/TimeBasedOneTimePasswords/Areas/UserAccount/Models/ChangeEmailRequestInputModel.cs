using System.ComponentModel.DataAnnotations;

namespace TimeBasedOneTimePasswords.Areas.UserAccount.Models
{
    public class ChangeEmailRequestInputModel
    {
        //[Required]
        [EmailAddress]
        public string NewEmail { get; set; }
    }
}