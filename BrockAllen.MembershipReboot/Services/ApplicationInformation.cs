using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class ApplicationInformation
    {
        public string ApplicationName { get; set; }
        public string FromEmail { get; set; }
        public string LoginUrl { get; set; }
        public string ConfirmAccountCreateUrl { get; set; }
        public string CancelAccountCreateUrl { get; set; }
        public string ConfirmPasswordResetUrl { get; set; }
        public string CancelPasswordResetUrl { get; set; }
    }
}
