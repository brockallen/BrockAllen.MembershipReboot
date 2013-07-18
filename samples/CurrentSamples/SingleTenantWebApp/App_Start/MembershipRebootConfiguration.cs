using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BrockAllen.MembershipReboot.Mvc.App_Start
{
    public class MembershipRebootConfig
    {
        public static MembershipRebootConfiguration Create()
        {
            var settings = SecuritySettings.Instance;
            var config = new MembershipRebootConfiguration(settings, new DelegateFactory(()=>new DefaultUserAccountRepository(settings.ConnectionStringName)));
            
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
                config.AddEventHandler(new EmailAccountCreatedEventHandler(formatter));
            }
            config.AddEventHandler(new EmailAccountEventsHandler(formatter));
            
            return config;
        }
    }
}