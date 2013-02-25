using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

namespace BrockAllen.MembershipReboot.Test.Models
{
    public class MockUserAccount : Mock<UserAccount>
    {
        public MockUserAccount(string tenant, string username, string password, string email)
            : base(tenant, username, password, email)
        {
            this.CallBase = true;
        }

        public MockUserAccount()
        {
            this.CallBase = true;
        }
    }
}
