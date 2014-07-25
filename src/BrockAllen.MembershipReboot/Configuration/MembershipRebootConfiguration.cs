/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot
{
    public class MembershipRebootConfiguration<TAccount>
        where TAccount : UserAccount
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
            this.VerificationKeyLifetime = securitySettings.VerificationKeyLifetime;

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
        public TimeSpan VerificationKeyLifetime { get; set; }

        AggregateValidator<TAccount> usernameValidators = new AggregateValidator<TAccount>();
        public void RegisterUsernameValidator(params IValidator<TAccount>[] items)
        {
            usernameValidators.AddRange(items);
        }
        public IValidator<TAccount> UsernameValidator { get { return usernameValidators; } }

        AggregateValidator<TAccount> passwordValidators = new AggregateValidator<TAccount>();
        public void RegisterPasswordValidator(params IValidator<TAccount>[] items)
        {
            passwordValidators.AddRange(items);
        }
        public IValidator<TAccount> PasswordValidator { get { return passwordValidators; } }

        AggregateValidator<TAccount> emailValidators = new AggregateValidator<TAccount>();
        public void RegisterEmailValidator(params IValidator<TAccount>[] items)
        {
            emailValidators.AddRange(items);
        }
        public IValidator<TAccount> EmailValidator { get { return emailValidators; } }

        EventBus eventBus = new EventBus();
        public IEventBus EventBus { get { return eventBus; } }
        public void AddEventHandler(params IEventHandler[] handlers)
        {
            foreach (var h in handlers) VerifyHandler(h);
            eventBus.AddRange(handlers);
        }
        
        EventBus validationBus = new EventBus();
        public IEventBus ValidationBus { get { return validationBus; } }
        public void AddValidationHandler(params IEventHandler[] handlers)
        {
            foreach (var h in handlers) VerifyHandler(h);
            validationBus.AddRange(handlers);
        }

        CommandBus commandBus = new CommandBus();
        public ICommandBus CommandBus { get { return commandBus; } }
        public void AddCommandHandler(ICommandHandler handler)
        {
            VerifyHandler(handler);
            commandBus.Add(handler);
        }

        private void VerifyHandler(IEventHandler e)
        {
            var type = e.GetType();
            var interfaces = type.GetInterfaces();
            foreach (var itf in interfaces)
            {
                if (itf.IsGenericType && itf.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                {
                    var eventHandlerType = itf.GetGenericArguments()[0];
                    if (eventHandlerType.IsGenericType)
                    {
                        var targetUserAccountType = eventHandlerType.GetGenericArguments()[0];
                        var isSameType = targetUserAccountType == typeof(TAccount);
                        if (!isSameType)
                        {
                            throw new ArgumentException(String.Format("Event handler: {0} must handle events for User Account type: {1}",
                                e.GetType().FullName,
                                typeof(TAccount).FullName));
                        }
                    }
                }
            }
        }
        private void VerifyHandler(ICommandHandler e)
        {
            var type = e.GetType();
            var interfaces = type.GetInterfaces();
            foreach (var itf in interfaces)
            {
                if (itf.IsGenericType && itf.GetGenericTypeDefinition() == typeof(ICommandHandler<>))
                {
                    var eventHandlerType = itf.GetGenericArguments()[0];
                    if (eventHandlerType.IsGenericType)
                    {
                        var targetUserAccountType = eventHandlerType.GetGenericArguments()[0];
                        var isSameType = targetUserAccountType == typeof(TAccount);
                        if (!isSameType)
                        {
                            throw new ArgumentException(String.Format("Command handler: {0} must handle commands for User Account type: {1}",
                                e.GetType().FullName,
                                typeof(TAccount).FullName));
                        }
                    }
                }
            }
        }
        
        public ICrypto Crypto { get; set; }
    }
    
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
}
