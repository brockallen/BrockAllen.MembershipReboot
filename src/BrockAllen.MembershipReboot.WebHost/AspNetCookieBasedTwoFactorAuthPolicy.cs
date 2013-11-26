/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Linq;
using System.Web;

namespace BrockAllen.MembershipReboot.WebHost
{
    public class AspNetCookieBasedTwoFactorAuthPolicy : CookieBasedTwoFactorAuthPolicy
    {
        public bool Debugging { get; set; }

        public AspNetCookieBasedTwoFactorAuthPolicy(bool debugging = false)
        {
            Debugging = debugging;
        }

        protected override string GetCookie(string name)
        {
            var ctx = HttpContext.Current;
            if (ctx.Request.Cookies.AllKeys.Contains(name))
            {
                return ctx.Request.Cookies[name].Value;
            }
            return null;
        }

        protected override void IssueCookie(string name, string value)
        {
            var ctx = HttpContext.Current;
            if (ctx.Request.IsSecureConnection || this.Debugging)
            {
                var cookie = new HttpCookie(name, value);
                cookie.HttpOnly = true;
                cookie.Secure = ctx.Request.IsSecureConnection || !this.Debugging;
                cookie.Expires = DateTime.Now.AddDays(this.PersistentCookieDurationInDays);
                cookie.Shareable = false;
                cookie.Path = ctx.Request.ApplicationPath;
                if (!cookie.Path.EndsWith("/")) cookie.Path += "/";

                ctx.Response.Cookies.Add(cookie);
            }
        }

        protected override void RemoveCookie(string name)
        {
            var ctx = HttpContext.Current;
            if (ctx.Request.IsSecureConnection || this.Debugging)
            {
                var cookie = new HttpCookie(name, ".");
                cookie.HttpOnly = true;
                cookie.Secure = ctx.Request.IsSecureConnection || !this.Debugging;
                cookie.Expires = DateTime.Now.AddYears(-1);
                cookie.Shareable = false;
                cookie.Path = ctx.Request.ApplicationPath;
                if (!cookie.Path.EndsWith("/")) cookie.Path += "/";

                ctx.Response.Cookies.Add(cookie);
            }
        }
    }

    public class AspNetCookieBasedTwoFactorAuthPolicy<T> 
        : CookieBasedTwoFactorAuthPolicy<T>
        where T : UserAccount
    {
        public bool Debugging { get; set; }

        public AspNetCookieBasedTwoFactorAuthPolicy(bool debugging = false)
        {
            Debugging = debugging;
        }

        protected override string GetCookie(string name)
        {
            var ctx = HttpContext.Current;
            if (ctx.Request.Cookies.AllKeys.Contains(name))
            {
                return ctx.Request.Cookies[name].Value;
            }
            return null;
        }

        protected override void IssueCookie(string name, string value)
        {
            var ctx = HttpContext.Current;
            if (ctx.Request.IsSecureConnection || this.Debugging)
            {
                var cookie = new HttpCookie(name, value);
                cookie.HttpOnly = true;
                cookie.Secure = ctx.Request.IsSecureConnection || !this.Debugging;
                cookie.Expires = DateTime.Now.AddDays(this.PersistentCookieDurationInDays);
                cookie.Shareable = false;
                cookie.Path = ctx.Request.ApplicationPath;
                if (!cookie.Path.EndsWith("/")) cookie.Path += "/";

                ctx.Response.Cookies.Add(cookie);
            }
        }

        protected override void RemoveCookie(string name)
        {
            var ctx = HttpContext.Current;
            if (ctx.Request.IsSecureConnection || this.Debugging)
            {
                var cookie = new HttpCookie(name, ".");
                cookie.HttpOnly = true;
                cookie.Secure = ctx.Request.IsSecureConnection || !this.Debugging;
                cookie.Expires = DateTime.Now.AddYears(-1);
                cookie.Shareable = false;
                cookie.Path = ctx.Request.ApplicationPath;
                if (!cookie.Path.EndsWith("/")) cookie.Path += "/";

                ctx.Response.Cookies.Add(cookie);
            }
        }
    }
}
