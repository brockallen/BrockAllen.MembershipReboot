using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class GetTwoFactorAuthToken : ICommand
    {
        public UserAccount Account { get; set; }
        public string Token { get; set; }
    }

    public class IssueTwoFactorAuthToken : ICommand
    {
        public UserAccount Account { get; set; }
        public string Token { get; set; }
        public bool Success { get; set; }
    }

    public class ClearTwoFactorAuthToken : ICommand
    {
        public UserAccount Account { get; set; }
    }
    
    public class GetValidationMessage : ICommand
    {
        public string ID { get; set; }
        public string Message { get; set; }
    }
}
