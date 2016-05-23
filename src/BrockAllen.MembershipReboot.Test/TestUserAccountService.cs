using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.Test
{
    public class TestUserAccountService : UserAccountService
    {
        public DateTime? Now { get; set; }

        public TestUserAccountService(MembershipRebootConfiguration configuration, IUserAccountRepository userRepository)
            : base(configuration, userRepository)
        {
        }

        protected internal override DateTime UtcNow
        {
            get
            {
                if (Now != null) return Now.Value;
                else return base.UtcNow;
            }
        }
    }
}
