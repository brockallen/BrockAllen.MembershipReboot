/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Microsoft.Owin;
using System;
using System.Collections.Generic;

namespace BrockAllen.MembershipReboot.Owin
{
    public class OwinCookieBasedTwoFactorAuthPolicy : CookieBasedTwoFactorAuthPolicy
    {
        IOwinContext owinContext;
        bool debugging;

        public OwinCookieBasedTwoFactorAuthPolicy(IDictionary<string, object> env, bool debugging = false)
        {
            this.owinContext = new OwinContext(env);
            this.debugging = debugging;
        }

        protected override string GetCookie(string name)
        {
            return owinContext.Request.Cookies[name];
        }

        protected override void IssueCookie(string name, string value)
        {
            if (owinContext.Request.IsSecure || this.debugging)
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

                owinContext.Response.Cookies.Append(name, value, new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(this.PersistentCookieDurationInDays),
                    HttpOnly = true,
                    Secure = owinContext.Request.IsSecure || !this.debugging,
                    Path = path
                });
            }
        }

        protected override void RemoveCookie(string name)
        {
            if (owinContext.Request.IsSecure || this.debugging)
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
                    Secure = owinContext.Request.IsSecure || !this.debugging,
                    Path = path
                });
            }
        }
    }
}
