using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BrockAllen.MembershipReboot.Mvc.App_Start
{
    public class PasswordValidator : IValidator
    {
        public ValidationResult Validate(UserAccountService service, UserAccount account, string value)
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
        public static MembershipRebootConfiguration Create()
        {
            var settings = SecuritySettings.Instance;
            settings.MultiTenant = false;
            
            var config = new MembershipRebootConfiguration(settings, new DelegateFactory(()=>new EFUserAccountRepository(settings.ConnectionStringName)));
            config.RegisterPasswordValidator(new PasswordValidator());
            var delivery = new SmtpMessageDelivery();
            var formatter = new EmailMessageFormatter(new Lazy<ApplicationInformation>(() =>
            {
                // build URL
                var baseUrl = HttpContext.Current.GetApplicationUrl();
                // area name
                baseUrl += "UserAccount/";

                return new ApplicationInformation
                {
                    ApplicationName = "Test",
                    LoginUrl = baseUrl + "Login",
                    VerifyAccountUrl = baseUrl + "Register/Confirm/",
                    CancelNewAccountUrl = baseUrl + "Register/Cancel/",
                    ConfirmPasswordResetUrl = baseUrl + "PasswordReset/Confirm/",
                    ConfirmChangeEmailUrl = baseUrl + "ChangeEmail/Confirm/"
                };
            }));
            if (settings.RequireAccountVerification)
            {
                config.AddEventHandler(new EmailAccountCreatedEventHandler(formatter, delivery));
            }
            config.AddEventHandler(new EmailAccountEventsHandler(formatter, delivery));
            
            return config;
        }

        /*
            kernel
                .Bind<ApplicationInformation>()
                .ToMethod(x=>
                    {
                       
                    });
        */
    }
}