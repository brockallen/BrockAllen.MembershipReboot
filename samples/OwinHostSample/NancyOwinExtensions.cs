using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Nancy;
using Nancy.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace OwinHostSample
{
    public static class NancyOwinExtensions
    {
        public static OwinContext ToOwinContext(this NancyContext ctx)
        {
            return new OwinContext((IDictionary<string, object>)ctx.Items[NancyOwinHost.RequestEnvironmentKey]);
        }

        public static IAuthenticationManager GetOwinAuthentication(this NancyContext context)
        {
            var environment = (IDictionary<string, object>)context.Items[NancyOwinHost.RequestEnvironmentKey];
            var owinContext = new OwinContext(environment);
            return owinContext.Authentication;
        }

        public static ClaimsPrincipal GetOwinPrincipal(this NancyContext context)
        {
            var auth = context.GetOwinAuthentication();
            return auth.User;
        }

        public static void ConfigureMembershipReboot(this IAppBuilder app)
        {
            app.UseMembershipReboot(Microsoft.Owin.Security.Cookies.CookieSecureOption.SameAsRequest);
            app.Use<MembershipRebootConfigurationMiddleware>();
        }

        public static void SetUserAccountService(this IOwinContext ctx, UserAccountService svc)
        {
            ctx.Set("mr.UserAccountService", svc);
        }
        public static UserAccountService GetUserAccountService(this IOwinContext ctx)
        {
            return ctx.Get<UserAccountService>("mr.UserAccountService");
        }
        public static void SetAuthenticationService(this IOwinContext ctx, AuthenticationService svc)
        {
            ctx.Set("mr.UserAuthenticationService", svc);
        }
        public static AuthenticationService GetAuthenticationService(this IOwinContext ctx)
        {
            return ctx.Get<AuthenticationService>("mr.UserAuthenticationService");
        }
    }

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
                    "Login",
                    "Register/Confirm/",
                    "Register/Cancel/",
                    "PasswordReset/Confirm/",
                    "ChangeEmail/Confirm/");

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