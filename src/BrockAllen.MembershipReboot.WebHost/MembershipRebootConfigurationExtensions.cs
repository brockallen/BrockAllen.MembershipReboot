/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.WebHost
{
    public static class MembershipRebootConfigurationExtensions
    {
        public static void ConfigureTwoFactorAuthenticationCookies<TAccount>(this MembershipRebootConfiguration<TAccount> config, bool debugging = false)
            where TAccount : UserAccount
        {
            config.AddCommandHandler(new TwoFactorAuthPolicyCommandHandler(new AspNetCookieBasedTwoFactorAuthPolicy(debugging)));
        }
    }
}
