/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot.WebHost
{
    public static class ConfigurationExtensions
    {
        public static void ConfigureAspNetCookieBasedTwoFactorAuthPolicy(this MembershipRebootConfiguration config)
        {
            if (config == null) throw new ArgumentNullException("config");

            config.ConfigureCookieBasedTwoFactorAuthPolicy(new AspNetCookieBasedTwoFactorAuthPolicy());
        }
    }
}
