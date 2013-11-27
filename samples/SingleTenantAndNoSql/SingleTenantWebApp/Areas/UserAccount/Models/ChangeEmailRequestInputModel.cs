using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models
{
    public class ChangeEmailRequestInputModel
    {
        //[Required]
        [EmailAddress]
        public string NewEmail { get; set; }
    }
}