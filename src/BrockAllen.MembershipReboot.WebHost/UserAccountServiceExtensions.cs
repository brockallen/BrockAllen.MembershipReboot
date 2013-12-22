using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.WebHost
{
    public static class UserAccountServiceExtensions
    {
        public static void ConfigureTwoFactorAuthenticationCookies<TAccount>(this UserAccountService<TAccount> svc, bool debugging = false)
            where TAccount : UserAccount
        {
            svc.AddCommandHandler(new TwoFactorAuthPolicyCommandHandler(new AspNetCookieBasedTwoFactorAuthPolicy(debugging)));
        }
    }
}
