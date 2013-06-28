/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class MembershipRebootConfiguration
    {
        public MembershipRebootConfiguration()
            : this(SecuritySettings.Instance, new DefaultFactory())
        {
        }

        public MembershipRebootConfiguration(SecuritySettings securitySettings, IFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (securitySettings == null) throw new ArgumentNullException("securitySettings");

            this.factory = factory;
            this.SecuritySettings = securitySettings;
        }

        IFactory factory;
        
        public SecuritySettings SecuritySettings { get; private set; }

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

        Type userAccountRepositoryType;
        public void RegisterUserAccountRepository<T>()
        {
            userAccountRepositoryType = typeof(T);
        }
        
        public IUserAccountRepository CreateUserAccountRepository()
        {
            if (userAccountRepositoryType == null) throw new Exception("UserAccountRepository type not registered");
            return factory.Create<IUserAccountRepository>(userAccountRepositoryType);
        }

        EventBus eventBus = new EventBus();
        public IEventBus EventBus { get { return eventBus; } }
        public void AddEventHandler(params IEventHandler[] handlers)
        {
            eventBus.AddRange(handlers);
        }
    }
}
