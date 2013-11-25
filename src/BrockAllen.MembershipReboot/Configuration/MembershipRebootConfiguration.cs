/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot
{
    public class MembershipRebootConfiguration
    {
        public MembershipRebootConfiguration()
            : this(SecuritySettings.Instance)
        {
        }

        public MembershipRebootConfiguration(SecuritySettings securitySettings)
        {
            if (securitySettings == null) throw new ArgumentNullException("securitySettings");
            
            this.MultiTenant = securitySettings.MultiTenant;
            this.DefaultTenant = securitySettings.DefaultTenant;
            this.EmailIsUsername = securitySettings.EmailIsUsername;
            this.UsernamesUniqueAcrossTenants = securitySettings.UsernamesUniqueAcrossTenants;
            this.RequireAccountVerification = securitySettings.RequireAccountVerification;
            this.AllowLoginAfterAccountCreation = securitySettings.AllowLoginAfterAccountCreation;
            this.AccountLockoutFailedLoginAttempts = securitySettings.AccountLockoutFailedLoginAttempts;
            this.AccountLockoutDuration = securitySettings.AccountLockoutDuration;
            this.AllowAccountDeletion = securitySettings.AllowAccountDeletion;
            this.PasswordHashingIterationCount = securitySettings.PasswordHashingIterationCount;
            this.PasswordResetFrequency = securitySettings.PasswordResetFrequency;
        }

        public bool MultiTenant { get; set; }
        public string DefaultTenant { get; set; }
        public bool EmailIsUsername { get; set; }
        public bool UsernamesUniqueAcrossTenants { get; set; }
        public bool RequireAccountVerification { get; set; }
        public bool AllowLoginAfterAccountCreation { get; set; }
        public int AccountLockoutFailedLoginAttempts { get; set; }
        public TimeSpan AccountLockoutDuration { get; set; }
        public bool AllowAccountDeletion { get; set; }
        public int PasswordHashingIterationCount { get; set; }
        public int PasswordResetFrequency { get; set; }

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
        
        EventBus validationBus = new EventBus();
        public IEventBus ValidationBus { get { return validationBus; } }
        public void AddValidationHandler(params IEventHandler[] handlers)
        {
            validationBus.AddRange(handlers);
        }
        
        public ITwoFactorAuthenticationPolicy TwoFactorAuthenticationPolicy { get; set; }
    }
}
