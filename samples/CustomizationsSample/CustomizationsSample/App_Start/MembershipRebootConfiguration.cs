using BrockAllen.MembershipReboot.WebHost;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Web;

namespace BrockAllen.MembershipReboot.Mvc.App_Start
{
    public class PasswordValidator : IValidator<CustomUserAccount>
    {
        public ValidationResult Validate(UserAccountService<CustomUserAccount> service, CustomUserAccount account, string value)
        {
            if (value.Length < 4)
            {
                return new ValidationResult("Password must be at least 4 characters long");
            }
            
            return null;
        }
    }

    public class MembershipRebootConfig
    {
        public static MembershipRebootConfiguration<CustomUserAccount> Create()
        {
            var settings = SecuritySettings.Instance;
            settings.MultiTenant = false;
            
            var config = new MembershipRebootConfiguration<CustomUserAccount>(settings);
            config.RegisterPasswordValidator(new PasswordValidator());
            config.CustomUserPropertiesToClaimsMap = user =>
                {
                    return new System.Security.Claims.Claim[]
                    {
                        new System.Security.Claims.Claim(ClaimTypes.GivenName, user.FirstName),
                        new System.Security.Claims.Claim(ClaimTypes.Surname, user.LastName),
                    };
                };

            var delivery = new SmtpMessageDelivery();

            var appinfo = new AspNetApplicationInformation("Test", "Test Email Signature",
                "UserAccount/Login", 
                "UserAccount/Register/Confirm/",
                "UserAccount/Register/Cancel/",
                "UserAccount/PasswordReset/Confirm/",
                "UserAccount/ChangeEmail/Confirm/");
            var formatter = new CustomEmailMessageFormatter(appinfo);

            if (settings.RequireAccountVerification)
            {
                config.AddEventHandler(new EmailAccountCreatedEventHandler<CustomUserAccount>(formatter, delivery));
            }
            config.AddEventHandler(new EmailAccountEventsHandler<CustomUserAccount>(formatter, delivery));
            config.AddEventHandler(new AuthenticationAuditEventHandler());
            config.AddEventHandler(new NotifyAccountOwnerWhenTooManyFailedLoginAttempts());

            config.AddValidationHandler(new PasswordChanging());
            config.AddEventHandler(new PasswordChanged());

            config.ConfigureCookieBasedTwoFactorAuthPolicy(new AspNetCookieBasedTwoFactorAuthPolicy<CustomUserAccount>());

            return config;
        }
    }
}