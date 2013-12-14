/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Owin
{
    public static class MembershipRebootOwinExtensions
    {
        public static void UseMembershipReboot<TAccount>(
            this IAppBuilder app,
            Func<IOwinContext, UserAccountService<TAccount>> userAccountServiceFactory,
            Func<IOwinContext, AuthenticationService<TAccount>> authenticationServiceFactory = null
        )
            where TAccount : UserAccount
        {
            app.Use<MembershipRebootMiddleware<TAccount>>(userAccountServiceFactory, authenticationServiceFactory);
            app.UseMembershipRebootCookieAuthentication();
        }

        public static void UseMembershipReboot<TAccount>(
            this IAppBuilder app,
            CookieAuthenticationOptions cookieOptions,
            Func<IOwinContext, UserAccountService<TAccount>> userAccountServiceFactory,
            Func<IOwinContext, AuthenticationService<TAccount>> authenticationServiceFactory = null
        )
            where TAccount : UserAccount
        {
            app.Use<MembershipRebootMiddleware<TAccount>>(userAccountServiceFactory, authenticationServiceFactory);
            app.UseMembershipRebootCookieAuthentication(cookieOptions);
        }

        public static void UseMembershipRebootCookieAuthentication(this IAppBuilder app)
        {
            var opts = new CookieAuthenticationOptions
            {
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,
                AuthenticationType = MembershipRebootOwinConstants.AuthenticationType,
                CookieSecure = CookieSecureOption.SameAsRequest
            };
            app.UseCookieAuthentication(opts);
        }
        
        public static void UseMembershipRebootCookieAuthentication(this IAppBuilder app, CookieAuthenticationOptions cookieOptions)
        {
            app.UseCookieAuthentication(cookieOptions);
            app.UseMembershipRebootTwoFactorAuthentication(cookieOptions.CookieSecure);
        }
        
        public static void UseMembershipRebootTwoFactorAuthentication(this IAppBuilder app, CookieSecureOption secure = CookieSecureOption.SameAsRequest)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Passive,
                AuthenticationType = MembershipRebootOwinConstants.AuthenticationTwoFactorType,
                CookieSecure = secure
            });
        }

        public static void SetUserAccountService<TAccount>(this IOwinContext ctx, Func<UserAccountService<TAccount>> func)
            where TAccount : UserAccount
        {
            ctx.SetService(func);
        }
        public static UserAccountService<TAccount> GetUserAccountService<TAccount>(this IOwinContext ctx)
            where TAccount : UserAccount
        {
            return ctx.GetService<UserAccountService<TAccount>>();
        }
        
        public static void SetAuthenticationService<TAccount>(this IOwinContext ctx, Func<AuthenticationService<TAccount>> func)
            where TAccount : UserAccount
        {
            ctx.SetService(func);
        }
        public static AuthenticationService<TAccount> GetAuthenticationService<TAccount>(this IOwinContext ctx)
            where TAccount : UserAccount
        {
            return ctx.GetService<AuthenticationService<TAccount>>();
        }

        public static void SetService<T>(this IOwinContext ctx, Func<T> f)
        {
            var key = "mr.Service." + typeof(T).FullName;
            if (!ctx.Environment.Keys.Contains(key))
            {
                ctx.Set(key, new Lazy<T>(f));
            }
        }
        public static T GetService<T>(this IOwinContext ctx)
        {
            var key = "mr.Service." + typeof(T).FullName;
            var lazy = ctx.Get<Lazy<T>>(key);
            if (lazy != null) return lazy.Value;
            return default(T);
        }
    }
}
