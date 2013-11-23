using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.Owin
{
    public class OwinCookieBasedTwoFactorAuthPolicy : CookieBasedTwoFactorAuthPolicy
    {
        OwinContext owinContext;

        public OwinCookieBasedTwoFactorAuthPolicy(IDictionary<string, object> env)
        {
            owinContext = new OwinContext(env);
        }

        protected override string GetCookie(string name)
        {
            return owinContext.Request.Cookies[name];
        }

        protected override void IssueCookie(string name, string value)
        {
            var path = "/";
            if (owinContext.Request.PathBase.HasValue)
            {
                path = owinContext.Request.PathBase.Value;
            }
            if (!path.StartsWith("/"))
            {
                path = "/" + path;
            }

            owinContext.Response.Cookies.Append(name, value, new CookieOptions {
                Expires = DateTime.Now.AddDays(this.PersistentCookieDurationInDays),
                HttpOnly = true, 
                Secure = true,
                Path = path
            });
        }

        protected override void RemoveCookie(string name)
        {
            var path = "/";
            if (owinContext.Request.PathBase.HasValue)
            {
                path = owinContext.Request.PathBase.Value;
            }
            if (!path.StartsWith("/"))
            {
                path = "/" + path;
            }

            owinContext.Response.Cookies.Append(name, ".", new CookieOptions
            {
                Expires = DateTime.Now.AddYears(-1),
                HttpOnly = true,
                Secure = true,
                Path = path
            });
        }
    }
}
