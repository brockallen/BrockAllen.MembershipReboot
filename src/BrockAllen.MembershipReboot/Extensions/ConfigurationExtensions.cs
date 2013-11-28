/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot
{
    public static class ConfigurationExtensions
    {
        public static void ConfigureCookieBasedTwoFactorAuthPolicy<TAccount>(this MembershipRebootConfiguration<TAccount> config, CookieBasedTwoFactorAuthPolicy<TAccount> policy)
            where TAccount : UserAccount
        {
            if (config == null) throw new ArgumentNullException("config");
            config.TwoFactorAuthenticationPolicy = policy;
            config.AddEventHandler(policy);
        }

        public static void ConfigurePasswordComplexity<TAccount>(this MembershipRebootConfiguration<TAccount> config)
            where TAccount : UserAccount
        {
            if (config == null) throw new ArgumentNullException("config");
            config.RegisterPasswordValidator(new PasswordComplexityValidator<TAccount>());
        }

        public static void ConfigurePasswordComplexity<TAccount>(this MembershipRebootConfiguration<TAccount> config, int minimumLength, int minimumNumberOfComplexityRules)
            where TAccount : UserAccount
        {
            if (config == null) throw new ArgumentNullException("config");
            config.RegisterPasswordValidator(new PasswordComplexityValidator<TAccount>(minimumLength, minimumNumberOfComplexityRules));
        }
    }
}
