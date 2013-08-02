/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    internal class UserAccountValidation
    {
        public static readonly IValidator UsernameDoesNotContainAtSign =
            new DelegateValidator((service, account, value) =>
            {
                if (value.Contains("@"))
                {
                    Tracing.Verbose(String.Format("[UserAccountValidation.UsernameDoesNotContainAtSign] validation failed: {0}, {1}, {2}", account.Tenant, account.Username, value));

                    return new ValidationResult("Username cannot contain the '@' character.");
                }
                return null;
            });

        public static readonly IValidator UsernameMustNotAlreadyExist =
            new DelegateValidator((service, account, value) =>
            {
                if (service.UsernameExists(account.Tenant, value))
                {
                    Tracing.Verbose(String.Format("[UserAccountValidation.EmailMustNotAlreadyExist] validation failed: {0}, {1}, {2}", account.Tenant, account.Username, value));

                    return new ValidationResult("Username already in use.");
                }
                return null;
            });

        public static readonly IValidator EmailIsValidFormat =
            new DelegateValidator((service, account, value) =>
            {
                EmailAddressAttribute validator = new EmailAddressAttribute();
                if (!validator.IsValid(value))
                {
                    Tracing.Verbose(String.Format("[UserAccountValidation.EmailIsValidFormat] validation failed: {0}, {1}, {2}", account.Tenant, account.Username, value));

                    return new ValidationResult("Email is invalid.");
                }
                return null;
            });

        public static readonly IValidator EmailMustNotAlreadyExist =
            new DelegateValidator((service, account, value) =>
            {
                if (service.EmailExists(account.Tenant, value))
                {
                    Tracing.Verbose(String.Format("[UserAccountValidation.EmailMustNotAlreadyExist] validation failed: {0}, {1}, {2}", account.Tenant, account.Username, value));
                    
                    return new ValidationResult("Email already in use.");
                }
                return null;
            });

        public static readonly IValidator PasswordMustBeDifferentThanCurrent =
        new DelegateValidator((service, account, value) =>
        {
            // Use LastLogin null-check to see if it's a new account
            // we don't want to run this logic if it's a new account
            if (account.LastLogin != null && account.VerifyHashedPassword(value))
            {
                Tracing.Verbose(String.Format("[UserAccountValidation.PasswordMustBeDifferentThanCurrent] validation failed: {0}, {1}", account.Tenant, account.Username));

                return new ValidationResult("The new password must be different than the old password.");
            }
            return null;
        });
    }
}
