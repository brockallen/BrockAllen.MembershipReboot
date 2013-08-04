/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Linq;
using System.Web;

namespace BrockAllen.MembershipReboot
{
    public class MembershipRebootConfiguration
    {
        public MembershipRebootConfiguration(SecuritySettings securitySettings, Func<IUserAccountRepository> factoryFunc)
            : this(securitySettings, new DelegateFactory(factoryFunc))
        {
            if (factoryFunc == null) throw new ArgumentNullException("factoryFunc");
        }
        
        public MembershipRebootConfiguration(Func<IUserAccountRepository> factoryFunc)
            : this(new DelegateFactory(factoryFunc))
        {
            if (factoryFunc == null) throw new ArgumentNullException("factoryFunc");
        }

        public MembershipRebootConfiguration(SecuritySettings securitySettings, IFactory factory)
        {
            if (securitySettings == null) throw new ArgumentNullException("securitySettings");
            if (factory == null) throw new ArgumentNullException("factory");

            this.factory = factory;
            this.SecuritySettings = securitySettings;
        }

        public MembershipRebootConfiguration(SecuritySettings securitySettings, IUserAccountRepository userAccountRepository)
            : this(securitySettings, new DelegateFactory(()=>userAccountRepository))
        {
            if (userAccountRepository == null) throw new ArgumentNullException("userAccountRepository");
        }

        public MembershipRebootConfiguration(IFactory factory)
            : this(SecuritySettings.Instance, factory)
        {
        }

        public MembershipRebootConfiguration(IUserAccountRepository userAccountRepository)
            : this(new DelegateFactory(() => userAccountRepository))
        {
            if (userAccountRepository == null) throw new ArgumentNullException("userAccountRepository");
        }

        public SecuritySettings SecuritySettings { get; private set; }

        IFactory factory;
        public IUserAccountRepository CreateUserAccountRepository()
        {
            return factory.CreateUserAccountRepository();
        }

        AggregateValidator usernameValidators = new AggregateValidator();
        public void RegisterUsernameValidator(params IValidator[] items)
        {
            usernameValidators.AddRange(items);
        }
        public IValidator UsernameValidator { get { return usernameValidators; } }

        AggregateValidator passwordValidators = new AggregateValidator();
        public void RegisterPasswordValidator(params IValidator[] items)
        {
            passwordValidators.AddRange(items);
        }
        public IValidator PasswordValidator { get { return passwordValidators; } }
        
        AggregateValidator emailValidators = new AggregateValidator();
        public void RegisterEmailValidator(params IValidator[] items)
        {
            emailValidators.AddRange(items);
        }
        public IValidator EmailValidator { get { return emailValidators; } }

        EventBus eventBus = new EventBus();
        public IEventBus EventBus { get { return eventBus; } }
        public void AddEventHandler(params IEventHandler[] handlers)
        {
            eventBus.AddRange(handlers);
        }

        public ITwoFactorAuthenticationPolicy TwoFactorAuthenticationPolicy { get; set; }
    }
}
