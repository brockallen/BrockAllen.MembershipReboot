using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Owin;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace OwinHostSample
{
    public class MembershipRebootConfigurationMiddleware
    {
        Func<IDictionary<string, object>, Task> next;
        public MembershipRebootConfigurationMiddleware(Func<IDictionary<string, object>, Task> next)
        {
            this.next = next;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            var ctx = new OwinContext(env);

            using (var db = new BrockAllen.MembershipReboot.Ef.DefaultUserAccountRepository())
            {
                var settings = SecuritySettings.FromConfiguration();
                var mrConfig = new MembershipRebootConfiguration(settings);
                mrConfig.ConfigureCookieBasedTwoFactorAuthPolicy(new OwinCookieBasedTwoFactorAuthPolicy(env));

                var appInfo = new OwinApplicationInformation(env,
                    "Test", "Test Email Signature",
                    "/Login",
                    "/Register/Confirm/",
                    "/Register/Cancel/",
                    "/PasswordReset/Confirm/",
                    "/ChangeEmail/Confirm/");

                var emailFormatter = new EmailMessageFormatter(appInfo);
                if (settings.RequireAccountVerification)
                {
                    // uncomment if you want email notifications -- also update smtp settings in web.config
                    mrConfig.AddEventHandler(new EmailAccountCreatedEventHandler(emailFormatter));
                }
                // uncomment if you want email notifications -- also update smtp settings in web.config
                mrConfig.AddEventHandler(new EmailAccountEventsHandler(emailFormatter));

                var uaSvc = new UserAccountService(mrConfig, db);
                var anSvc = new OwinAuthenticationService(uaSvc, env);
                try
                {
                    ctx.SetUserAccountService(uaSvc);
                    ctx.SetAuthenticationService(anSvc);

                    await next(env);
                }
                finally
                {
                    ctx.SetUserAccountService(null);
                    ctx.SetAuthenticationService(null);
                }
            }
        }
    }

}