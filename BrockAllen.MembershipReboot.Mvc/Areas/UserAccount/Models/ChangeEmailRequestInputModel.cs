using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models
{
    public class ChangeEmailRequestInputModel
    {
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; }
    }
}