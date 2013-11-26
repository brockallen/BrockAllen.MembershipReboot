/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace BrockAllen.MembershipReboot
{
    public class MembershipRebootConfiguration : MembershipRebootConfiguration<UserAccount>
    {
        public MembershipRebootConfiguration()
            : this(SecuritySettings.Instance)
        {
        }

        public MembershipRebootConfiguration(SecuritySettings securitySettings)
            : base(securitySettings)
        {
        }
    }

    public class MembershipRebootConfiguration<T>
        where T : UserAccount
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

            this.Crypto = new DefaultCrypto();
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

        AggregateValidator<T> usernameValidators = new AggregateValidator<T>();
        public void RegisterUsernameValidator(params IValidator<T>[] items)
        {
            usernameValidators.AddRange(items);
        }
        public IValidator<T> UsernameValidator { get { return usernameValidators; } }

        AggregateValidator<T> passwordValidators = new AggregateValidator<T>();
        public void RegisterPasswordValidator(params IValidator<T>[] items)
        {
            passwordValidators.AddRange(items);
        }
        public IValidator<T> PasswordValidator { get { return passwordValidators; } }

        AggregateValidator<T> emailValidators = new AggregateValidator<T>();
        public void RegisterEmailValidator(params IValidator<T>[] items)
        {
            emailValidators.AddRange(items);
        }
        public IValidator<T> EmailValidator { get { return emailValidators; } }

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
        public ICrypto Crypto { get; set; }
        public Func<T, IEnumerable<Claim>> CustomUserPropertiesToClaimsMap { get; set; }
    }
}
