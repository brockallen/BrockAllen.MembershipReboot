using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models
{
    public class RegisterInputModel : IValidatableObject
    {
        [ScaffoldColumn(false)]
        public string Username { get; set; }
        
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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!SecuritySettings.Instance.EmailIsUsername && 
                String.IsNullOrWhiteSpace(Username))
            {
                yield return new ValidationResult("Username is required", new string[] { "Username" });
            }
        }
    }
}