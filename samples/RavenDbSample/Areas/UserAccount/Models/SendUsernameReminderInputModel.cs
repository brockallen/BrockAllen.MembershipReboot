using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models
{
    public class SendUsernameReminderInputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}