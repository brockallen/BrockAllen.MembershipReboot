using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public static class ConfigurationExtensions
    {
        public static void ConfigureCookieBasedTwoFactorAuthPolicy(this MembershipRebootConfiguration config, CookieBasedTwoFactorAuthPolicy policy)
        {
            if (config == null) throw new ArgumentNullException("config");

            config.TwoFactorAuthenticationPolicy = policy;
            config.AddEventHandler(policy);
        }

        public static void ConfigureAspNetCookieBasedTwoFactorAuthPolicy(this MembershipRebootConfiguration config)
        {
            if (config == null) throw new ArgumentNullException("config");

            config.ConfigureCookieBasedTwoFactorAuthPolicy(new AspNetCookieBasedTwoFactorAuthPolicy());
        }
    }
}
