using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace TimeBasedOneTimePasswords.Areas.UserAccount.Models
{
    public class ChangePasswordFromResetKeyInputModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Required]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Password confirmation must match password.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        
        [HiddenInput]
        public string Key { get; set; }
    }
}