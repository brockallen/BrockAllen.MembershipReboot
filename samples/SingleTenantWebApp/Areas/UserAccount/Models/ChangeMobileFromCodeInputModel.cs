using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models
{
    public class ChangeMobileFromCodeInputModel
    {
        [Required]
        public string Code { get; set; }
    }
    
}