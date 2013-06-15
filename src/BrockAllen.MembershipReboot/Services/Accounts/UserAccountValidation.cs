using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                    throw new ValidationException("Email is invalid.");
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
    }
}
