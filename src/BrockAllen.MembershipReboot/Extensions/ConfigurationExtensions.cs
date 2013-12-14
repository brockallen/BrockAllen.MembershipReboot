/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot
{
    public static class ConfigurationExtensions
    {
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
