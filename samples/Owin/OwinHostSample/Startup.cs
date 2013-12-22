using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Ef;
using BrockAllen.MembershipReboot.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Web;

[assembly: Microsoft.Owin.OwinStartup(typeof(OwinHostSample.Startup))]

namespace OwinHostSample
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMembershipReboot(app);
            app.UseNancy();
        }

        private static void ConfigureMembershipReboot(IAppBuilder app)
        {
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.MigrateDatabaseToLatestVersion<DefaultMembershipRebootDatabase, BrockAllen.MembershipReboot.Ef.Migrations.Configuration>());
            var cookieOptions = new CookieAuthenticationOptions
            {
                AuthenticationType = MembershipRebootOwinConstants.AuthenticationType
            };
            Func<IDictionary<string, object>, UserAccountService> uaFunc = env =>
            {
                var appInfo = new OwinApplicationInformation(
                    env,
                    "Test",
                    "Test Email Signature",
                    "/Login",
                    "/Register/Confirm/",
                    "/Register/Cancel/",
                    "/PasswordReset/Confirm/");

                var config = new MembershipRebootConfiguration();
                var emailFormatter = new EmailMessageFormatter(appInfo);
                // uncomment if you want email notifications -- also update smtp settings in web.config
                config.AddEventHandler(new EmailAccountEventsHandler(emailFormatter));

                var svc = new UserAccountService(config, new DefaultUserAccountRepository());
                var debugging = false;
#if DEBUG
                debugging = true;
#endif
                svc.ConfigureTwoFactorAuthenticationCookies(env, debugging);
                return svc;
            };
            Func<IDictionary<string, object>, AuthenticationService> authFunc = env =>
            {
                return new OwinAuthenticationService(cookieOptions.AuthenticationType, uaFunc(env), env);
            };

            app.UseMembershipReboot(cookieOptions, uaFunc, authFunc);
        }
    }
}