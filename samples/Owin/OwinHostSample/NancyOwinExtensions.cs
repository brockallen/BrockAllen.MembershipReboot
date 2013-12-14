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

        public static Guid GetUserID(this IUserIdentity user)
        {
            return ((ClaimsUserIdentity)user).Principal.GetUserID();
        }
    }
}