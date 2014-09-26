/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    internal class UserAccountValidation<TAccount>
        where TAccount : UserAccount
    {
        public static readonly IValidator<TAccount> UsernameDoesNotContainAtSign =
            new DelegateValidator<TAccount>((service, account, value) =>
            {
                if (value.Contains("@"))
                {
                    Tracing.Verbose("[UserAccountValidation.UsernameDoesNotContainAtSign] validation failed: {0}, {1}, {2}", account.Tenant, account.Username, value);

                    return new ValidationResult(service.GetValidationMessage(MembershipRebootConstants.ValidationMessages.UsernameCannotContainAtSign));
                }
                return null;
            });

        static readonly char[] SpecialChars = { '.', ' ', '_', '-', '\'' };

        public static bool IsValidUsernameChar(char c)
        {
            return
                Char.IsLetterOrDigit(c) || 
                SpecialChars.Contains(c);
        }

        public static readonly IValidator<TAccount> UsernameOnlySingleInstanceOfSpecialCharacters =
                   new DelegateValidator<TAccount>((service, account, value) =>
                   {
                       foreach(var specialChar in SpecialChars)
                       {
                           var doubleChar = specialChar.ToString() + specialChar.ToString();
                           if (value.Contains(doubleChar))
                           {
                               Tracing.Verbose("[UserAccountValidation.UsernameOnlySingleInstanceOfSpecialCharacters] validation failed: {0}, {1}, {2}", account.Tenant, account.Username, value);
                               return new ValidationResult(service.GetValidationMessage(MembershipRebootConstants.ValidationMessages.UsernameCannotRepeatSpecialCharacters));
                           }
                       }

                       return null;
                   });
        
        public static readonly IValidator<TAccount> UsernameOnlyContainsValidCharacters =
            new DelegateValidator<TAccount>((service, account, value) =>
            {
                if (!value.All(x => IsValidUsernameChar(x)))
                {
                    Tracing.Verbose("[UserAccountValidation.UsernameOnlyContainsValidCharacters] validation failed: {0}, {1}, {2}", account.Tenant, account.Username, value);

                    return new ValidationResult(service.GetValidationMessage(MembershipRebootConstants.ValidationMessages.UsernameOnlyContainsValidCharacters));
                }
                return null;
            });

        public static readonly IValidator<TAccount> UsernameCanOnlyStartOrEndWithLetterOrDigit =
                   new DelegateValidator<TAccount>((service, account, value) =>
                   {
                       if (!Char.IsLetterOrDigit(value.First()) || !Char.IsLetterOrDigit(value.Last()))
                       {
                           Tracing.Verbose("[UserAccountValidation.UsernameCanOnlyStartOrEndWithLetterOrDigit] validation failed: {0}, {1}, {2}", account.Tenant, account.Username, value);

                           return new ValidationResult(service.GetValidationMessage(MembershipRebootConstants.ValidationMessages.UsernameCanOnlyStartOrEndWithLetterOrDigit));
                       }
                       return null;
                   });
        
        public static readonly IValidator<TAccount> UsernameMustNotAlreadyExist =
            new DelegateValidator<TAccount>((service, account, value) =>
            {
                if (service.UsernameExists(account.Tenant, value))
                {
                    Tracing.Verbose("[UserAccountValidation.EmailMustNotAlreadyExist] validation failed: {0}, {1}, {2}", account.Tenant, account.Username, value);

                    return new ValidationResult(service.GetValidationMessage(MembershipRebootConstants.ValidationMessages.UsernameAlreadyInUse));
                }
                return null;
            });

        public static readonly IValidator<TAccount> EmailRequired =
            new DelegateValidator<TAccount>((service, account, value) =>
            {
                if (service.Configuration.RequireAccountVerification &&
                    String.IsNullOrWhiteSpace(value))
                {
                    Tracing.Verbose("[UserAccountValidation.EmailRequired] validation failed: {0}, {1}", account.Tenant, account.Username);

                    return new ValidationResult(service.GetValidationMessage(MembershipRebootConstants.ValidationMessages.EmailRequired));
                }
                return null;
            });

        public static readonly IValidator<TAccount> EmailIsValidFormat =
            new DelegateValidator<TAccount>((service, account, value) =>
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    EmailAddressAttribute validator = new EmailAddressAttribute();
                    if (!validator.IsValid(value))
                    {
                        Tracing.Verbose("[UserAccountValidation.EmailIsValidFormat] validation failed: {0}, {1}, {2}", account.Tenant, account.Username, value);

                        return new ValidationResult(service.GetValidationMessage(MembershipRebootConstants.ValidationMessages.InvalidEmail));
                    }
                }
                return null;
            });

        public static readonly IValidator<TAccount> EmailIsRequiredIfRequireAccountVerificationEnabled =
            new DelegateValidator<TAccount>((service, account, value) =>
            {
                if (service.Configuration.RequireAccountVerification && String.IsNullOrWhiteSpace(value))
                {
                    return new ValidationResult(service.GetValidationMessage(MembershipRebootConstants.ValidationMessages.EmailRequired));
                }
                return null;
            });

        public static readonly IValidator<TAccount> EmailMustNotAlreadyExist =
            new DelegateValidator<TAccount>((service, account, value) =>
            {
                if (!String.IsNullOrWhiteSpace(value) && service.EmailExistsOtherThan(account, value))
                {
                    Tracing.Verbose("[UserAccountValidation.EmailMustNotAlreadyExist] validation failed: {0}, {1}, {2}", account.Tenant, account.Username, value);

                    return new ValidationResult(service.GetValidationMessage(MembershipRebootConstants.ValidationMessages.EmailAlreadyInUse));
                }
                return null;
            });

        public static readonly IValidator<TAccount> PasswordMustBeDifferentThanCurrent =
            new DelegateValidator<TAccount>((service, account, value) =>
        {
            // Use LastLogin null-check to see if it's a new account
            // we don't want to run this logic if it's a new account
            if (!account.IsNew() && service.VerifyHashedPassword(account, value))
            {
                Tracing.Verbose("[UserAccountValidation.PasswordMustBeDifferentThanCurrent] validation failed: {0}, {1}", account.Tenant, account.Username);

                return new ValidationResult(service.GetValidationMessage(MembershipRebootConstants.ValidationMessages.NewPasswordMustBeDifferent));
            }
            return null;
        });
    }
}
