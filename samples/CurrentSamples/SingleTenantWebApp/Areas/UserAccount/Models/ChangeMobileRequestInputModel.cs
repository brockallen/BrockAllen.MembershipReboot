using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models
{
    public class ChangeMobileRequestInputModel
    {
        //[Required]
        public string NewMobilePhone { get; set; }
    }
}