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

        public static void AddCommandHandler<TAccount, TCommand>(this MembershipRebootConfiguration<TAccount> config, Action<TCommand> action)
            where TAccount : UserAccount
            where TCommand : ICommand
        {
            if (config == null) throw new ArgumentNullException("config");
            config.AddCommandHandler(new DelegateCommandHandler<TCommand>(action));
        }

        public static void AddValidationHandler<TAccount, TEvent>(this MembershipRebootConfiguration<TAccount> config, Action<TEvent> action)
            where TAccount : UserAccount
            where TEvent : IEvent
        {
            if (config == null) throw new ArgumentNullException("config");
            config.AddValidationHandler(new DelegateEventHandler<TEvent>(action));
        }

        public static void AddEventHandler<TAccount, TEvent>(this MembershipRebootConfiguration<TAccount> config, Action<TEvent> action)
            where TAccount : UserAccount
            where TEvent : IEvent
        {
            if (config == null) throw new ArgumentNullException("config");
            config.AddEventHandler(new DelegateEventHandler<TEvent>(action));
        }
    }
}
