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
}