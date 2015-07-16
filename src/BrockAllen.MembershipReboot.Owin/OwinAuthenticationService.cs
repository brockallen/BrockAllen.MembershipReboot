/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace BrockAllen.MembershipReboot.Owin
{
    public class OwinAuthenticationService : AuthenticationService
    {
        IOwinContext context;
        string authenticationType;

        public OwinAuthenticationService(
            string authenticationType,
            UserAccountService svc,
            IDictionary<string, object> env,
            ClaimsAuthenticationManager transformer
        )
            : base(svc, transformer)
        {
            this.authenticationType = authenticationType;
            context = new OwinContext(env);
        }

        public OwinAuthenticationService(
            string authenticationType,
            UserAccountService svc,
            IDictionary<string, object> env
        )
            : this(authenticationType, svc, env, null)
        {
        }

        public OwinAuthenticationService(
            UserAccountService svc,
            IDictionary<string, object> env
        )
            : this(MembershipRebootOwinConstants.AuthenticationType, svc, env, null)
        {
        }

        protected override void IssueToken(System.Security.Claims.ClaimsPrincipal principal, 
            TimeSpan? tokenLifetime = null, bool? persistentCookie = null)
        {
            if (principal == null) throw new ArgumentNullException("principal");

            if (principal.Identity.IsAuthenticated) 
            {
                IssueCookie(principal.Claims, authenticationType, tokenLifetime, persistentCookie);
            }
            else
            {
                IssueCookie(principal.Claims, MembershipRebootOwinConstants.AuthenticationTwoFactorType, tokenLifetime, persistentCookie);
            }
        }

        private void IssueCookie(IEnumerable<Claim> enumerable, string authType, TimeSpan? tokenLifetime, bool? persistentCookie)
        {
            SignOut();

            var props = new AuthenticationProperties();
            if (tokenLifetime.HasValue) props.ExpiresUtc = DateTime.UtcNow.Add(tokenLifetime.Value);
            if (persistentCookie.HasValue) props.IsPersistent = persistentCookie.Value;

            var id = new ClaimsIdentity(enumerable, authType);

            context.Authentication.SignIn(props, id);
        }

        protected override void RevokeToken()
        {
            context.Authentication.SignOut(
                this.authenticationType,
                MembershipRebootOwinConstants.AuthenticationTwoFactorType);
        }

        protected override ClaimsPrincipal GetCurentPrincipal()
        {
            var u = context.Request.User;
            if (u != null && u.Identity.AuthenticationType == authenticationType)
            {
                var cp = context.Request.User as ClaimsPrincipal;
                if (cp == null) cp = new ClaimsPrincipal(context.Request.User);
                return cp;
            }
            return null;
        }
    }
    
    public class OwinAuthenticationService<TAccount> : AuthenticationService<TAccount>
        where TAccount : UserAccount
    {
        IOwinContext context;
        string authenticationType;

        public OwinAuthenticationService(
            string authenticationType,
            UserAccountService<TAccount> svc,
            IDictionary<string, object> env,
            ClaimsAuthenticationManager transformer
        )
            : base(svc, transformer)
        {
            this.authenticationType = authenticationType;
            context = new OwinContext(env);
        }

        public OwinAuthenticationService(
            string authenticationType,
            UserAccountService<TAccount> svc,
            IDictionary<string, object> env
        )
            : this(authenticationType, svc, env, null)
        {
        }

        public OwinAuthenticationService(
            UserAccountService<TAccount> svc,
            IDictionary<string, object> env
        )
            : this(MembershipRebootOwinConstants.AuthenticationType, svc, env, null)
        {
        }

        protected override void IssueToken(System.Security.Claims.ClaimsPrincipal principal,
            TimeSpan? tokenLifetime = null, bool? persistentCookie = null)
        {
            if (principal == null) throw new ArgumentNullException("principal");

            if (principal.Identity.IsAuthenticated)
            {
                IssueCookie(principal.Claims, authenticationType, tokenLifetime, persistentCookie);
            }
            else
            {
                IssueCookie(principal.Claims, MembershipRebootOwinConstants.AuthenticationTwoFactorType, tokenLifetime, persistentCookie);
            }
        }

        private void IssueCookie(IEnumerable<Claim> enumerable, string authType, TimeSpan? tokenLifetime, bool? persistentCookie)
        {
            SignOut();

            var props = new AuthenticationProperties();
            if (tokenLifetime.HasValue) props.ExpiresUtc = DateTime.UtcNow.Add(tokenLifetime.Value);
            if (persistentCookie.HasValue) props.IsPersistent = persistentCookie.Value;

            var id = new ClaimsIdentity(enumerable, authType);

            context.Authentication.SignIn(props, id);
        }

        protected override void RevokeToken()
        {
            context.Authentication.SignOut(
                this.authenticationType,
                MembershipRebootOwinConstants.AuthenticationTwoFactorType);
        }

        protected override ClaimsPrincipal GetCurentPrincipal()
        {
            var u = context.Request.User;
            if (u != null && u.Identity.AuthenticationType == authenticationType)
            {
                var cp = context.Request.User as ClaimsPrincipal;
                if (cp == null) cp = new ClaimsPrincipal(context.Request.User);
                return cp;
            }
            return null;
        }
    }
}
