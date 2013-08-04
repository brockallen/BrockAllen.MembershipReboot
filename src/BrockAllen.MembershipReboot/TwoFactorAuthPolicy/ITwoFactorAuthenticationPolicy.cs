using BrockAllen.MembershipReboot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public interface ITwoFactorAuthenticationPolicy
    {
        bool RequestRequiresTwoFactorAuth(UserAccount account);
    }
}
