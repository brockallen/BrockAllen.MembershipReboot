/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Owin;
using Microsoft.Owin.Security.Cookies;
using System;
using System.Collections.Generic;

namespace Owin
{
    public static class MembershipRebootOwinExtensions
    {
        public static void UseMembershipReboot<TAccount>(
            this IAppBuilder app,
            Func<IDictionary<string, object>, UserAccountService<TAccount>> userAccountServiceFactory,
            Func<IDictionary<string, object>, AuthenticationService<TAccount>> authenticationServiceFactory = null
        )
            where TAccount : UserAccount
        {
            app.Use<MembershipRebootMiddleware<TAccount>>(userAccountServiceFactory, authenticationServiceFactory);
            app.UseMembershipRebootCookieAuthentication();
        }

        public static void UseMembershipReboot<TAccount>(
            this IAppBuilder app,
            CookieAuthenticationOptions cookieOptions,
            Func<IDictionary<string, object>, UserAccountService<TAccount>> userAccountServiceFactory,
            Func<IDictionary<string, object>, AuthenticationService<TAccount>> authenticationServiceFactory = null
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

        public static void SetUserAccountService<TAccount>(this IDictionary<string, object> env, Func<UserAccountService<TAccount>> func)
            where TAccount : UserAccount
        {
            env.SetService(func);
        }
        public static UserAccountService<TAccount> GetUserAccountService<TAccount>(this IDictionary<string, object> env)
            where TAccount : UserAccount
        {
            return env.GetService<UserAccountService<TAccount>>();
        }

        public static void SetAuthenticationService<TAccount>(this IDictionary<string, object> env, Func<AuthenticationService<TAccount>> func)
            where TAccount : UserAccount
        {
            env.SetService(func);
        }
        public static AuthenticationService<TAccount> GetAuthenticationService<TAccount>(this IDictionary<string, object> env)
            where TAccount : UserAccount
        {
            return env.GetService<AuthenticationService<TAccount>>();
        }

        public static void SetService<T>(this IDictionary<string, object> env, Func<T> f)
        {
            var key = "mr.Service." + typeof(T).FullName;

            object val;
            if (!env.TryGetValue(key, out val))
            {
                env[key] = new Lazy<T>(f);
            }
        }
        public static T GetService<T>(this IDictionary<string, object> env)
        {
            var key = "mr.Service." + typeof(T).FullName;

            object val;
            if (env.TryGetValue(key, out val))
            {
                var lazy = (Lazy<T>)val;
                if (lazy != null) return lazy.Value;
            }

            return default(T);
        }
    }
}
