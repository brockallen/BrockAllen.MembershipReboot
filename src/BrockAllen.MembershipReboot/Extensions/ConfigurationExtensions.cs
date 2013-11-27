/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot
{
    public static class ConfigurationExtensions
    {
        public static void ConfigureCookieBasedTwoFactorAuthPolicy<T>(this MembershipRebootConfiguration<T> config, CookieBasedTwoFactorAuthPolicy<T> policy)
            where T : UserAccount
        {
            if (config == null) throw new ArgumentNullException("config");
            config.TwoFactorAuthenticationPolicy = policy;
            config.AddEventHandler(policy);
        }

        public static void ConfigurePasswordComplexity<T>(this MembershipRebootConfiguration<T> config)
            where T : UserAccount
        {
            if (config == null) throw new ArgumentNullException("config");
            config.RegisterPasswordValidator(new PasswordComplexityValidator<T>());
        }

        public static void ConfigurePasswordComplexity<T>(this MembershipRebootConfiguration<T> config, int minimumLength, int minimumNumberOfComplexityRules)
            where T : UserAccount
        {
            if (config == null) throw new ArgumentNullException("config");
            config.RegisterPasswordValidator(new PasswordComplexityValidator<T>(minimumLength, minimumNumberOfComplexityRules));
        }
    }
}
