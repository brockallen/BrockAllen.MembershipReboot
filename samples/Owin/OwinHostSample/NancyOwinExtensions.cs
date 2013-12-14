using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Nancy;
using Nancy.Owin;
using Nancy.Security;
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
        public static IDictionary<string, object> GetOwinEnvironment(this NancyContext ctx)
        {
            return (IDictionary<string, object>)(ctx.Items[NancyOwinHost.RequestEnvironmentKey]);
        }
        public static UserAccountService<TAccount> GetUserAccountService<TAccount>(this NancyContext ctx)
            where TAccount : UserAccount
        {
            return ctx.GetOwinEnvironment().GetUserAccountService<TAccount>();
        }
        public static AuthenticationService<TAccount> GetAuthenticationService<TAccount>(this NancyContext ctx)
            where TAccount : UserAccount
        {
            return ctx.GetOwinEnvironment().GetAuthenticationService<TAccount>();
        }

        public static UserAccountService<UserAccount> GetUserAccountService(this NancyContext ctx)
        {
            return ctx.GetUserAccountService<UserAccount>();
        }
        public static AuthenticationService<UserAccount> GetAuthenticationService(this NancyContext ctx)
        {
            return ctx.GetAuthenticationService<UserAccount>();
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

        public static Guid GetUserID(this IUserIdentity user)
        {
            return ((ClaimsUserIdentity)user).Principal.GetUserID();
        }
    }
}