using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.Owin
{
    public static class UserAccountServiceExtensions
    {
        public static void ConfigureTwoFactorAuthenticationCookies<TAccount>(this UserAccountService<TAccount> svc, IDictionary<string, object> env, bool debugging = false)
            where TAccount : UserAccount
        {
            svc.AddCommandHandler(new TwoFactorAuthPolicyCommandHandler(new OwinCookieBasedTwoFactorAuthPolicy(env, debugging)));
        }
    }
}
