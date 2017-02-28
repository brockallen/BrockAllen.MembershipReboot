using System.ComponentModel.DataAnnotations;

namespace TimeBasedOneTimePasswords.Areas.UserAccount.Models
{
    public class ChangeMobileFromCodeInputModel
    {
        [Required]
        public string Code { get; set; }
    }
    
}