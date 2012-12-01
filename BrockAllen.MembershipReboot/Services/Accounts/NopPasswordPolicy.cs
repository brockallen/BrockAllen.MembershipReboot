using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class NopPasswordPolicy : IPasswordPolicy
    {
        public string PolicyMessage
        {
            get { throw new NotImplementedException(); }
        }

        public bool ValidatePassword(string password)
        {
            throw new NotImplementedException();
        }
    }
}
