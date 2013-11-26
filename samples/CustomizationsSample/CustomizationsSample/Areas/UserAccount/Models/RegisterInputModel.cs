using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models
{
    public class RegisterInputModel
    {
        [ScaffoldColumn(false)]
        public string Username { get; set; }

        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Required]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage="Password confirmation must match password.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}