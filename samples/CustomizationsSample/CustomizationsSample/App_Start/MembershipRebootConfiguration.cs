using BrockAllen.MembershipReboot.WebHost;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Web;

namespace BrockAllen.MembershipReboot.Mvc.App_Start
{
    public class MembershipRebootConfig
    {
        public static MembershipRebootConfiguration<CustomUserAccount> Create()
        {
            var settings = SecuritySettings.Instance;
            settings.MultiTenant = false;
            
            var config = new MembershipRebootConfiguration<CustomUserAccount>(settings);
            config.AddEventHandler(new DebuggerEventHandler<CustomUserAccount>());

            config.RegisterPasswordValidator(new PasswordValidator());
            config.ConfigurePasswordComplexity(5, 3);

            config.AddCommandHandler(new CustomClaimsMapper());

            var delivery = new SmtpMessageDelivery();

            var appinfo = new AspNetApplicationInformation("Test", "Test Email Signature",
                "UserAccount/Login",
                "UserAccount/ChangeEmail/Confirm/",
                "UserAccount/Register/Cancel/",
                "UserAccount/PasswordReset/Confirm/");
            var formatter = new CustomEmailMessageFormatter(appinfo);

            config.AddEventHandler(new EmailAccountEventsHandler<CustomUserAccount>(formatter, delivery));
            config.AddEventHandler(new AuthenticationAuditEventHandler());
            config.AddEventHandler(new NotifyAccountOwnerWhenTooManyFailedLoginAttempts());

            config.AddValidationHandler(new PasswordChanging());
            config.AddEventHandler(new PasswordChanged());
            config.AddCommandHandler(new CustomValidationMessages());

            return config;
        }
    }
}