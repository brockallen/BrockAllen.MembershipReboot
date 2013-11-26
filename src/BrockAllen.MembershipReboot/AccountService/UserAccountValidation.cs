/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot
{
    internal class UserAccountValidation<T>
        where T : UserAccount
    {
        public static readonly IValidator<T> UsernameDoesNotContainAtSign =
            new DelegateValidator<T>((service, account, value) =>
            {
                if (value.Contains("@"))
                {
                    Tracing.Verbose("[UserAccountValidation.UsernameDoesNotContainAtSign] validation failed: {0}, {1}, {2}", account.Tenant, account.Username, value);

                    return new ValidationResult(Resources.ValidationMessages.UsernameCannotContainAtSign);
                }
                return null;
            });

        public static readonly IValidator<T> UsernameMustNotAlreadyExist =
            new DelegateValidator<T>((service, account, value) =>
            {
                if (service.UsernameExists(account.Tenant, value))
                {
                    Tracing.Verbose("[UserAccountValidation.EmailMustNotAlreadyExist] validation failed: {0}, {1}, {2}", account.Tenant, account.Username, value);

                    return new ValidationResult(Resources.ValidationMessages.UsernameAlreadyInUse);
                }
                return null;
            });

        public static readonly IValidator<T> EmailIsValidFormat =
            new DelegateValidator<T>((service, account, value) =>
            {
                EmailAddressAttribute validator = new EmailAddressAttribute();
                if (!validator.IsValid(value))
                {
                    Tracing.Verbose("[UserAccountValidation.EmailIsValidFormat] validation failed: {0}, {1}, {2}", account.Tenant, account.Username, value);

                    return new ValidationResult(Resources.ValidationMessages.InvalidEmail);
                }
                return null;
            });

        public static readonly IValidator<T> EmailMustNotAlreadyExist =
            new DelegateValidator<T>((service, account, value) =>
            {
                if (service.EmailExists(account.Tenant, value))
                {
                    Tracing.Verbose("[UserAccountValidation.EmailMustNotAlreadyExist] validation failed: {0}, {1}, {2}", account.Tenant, account.Username, value);
                    
                    return new ValidationResult(Resources.ValidationMessages.EmailAlreadyInUse);
                }
                return null;
            });

        public static readonly IValidator<T> PasswordMustBeDifferentThanCurrent =
            new DelegateValidator<T>((service, account, value) =>
        {
            // Use LastLogin null-check to see if it's a new account
            // we don't want to run this logic if it's a new account
            if (account.LastLogin != null && service.VerifyHashedPassword(account, value))
            {
                Tracing.Verbose("[UserAccountValidation.PasswordMustBeDifferentThanCurrent] validation failed: {0}, {1}", account.Tenant, account.Username);

                return new ValidationResult(Resources.ValidationMessages.NewPasswordMustBeDifferent);
            }
            return null;
        });
    }
}
