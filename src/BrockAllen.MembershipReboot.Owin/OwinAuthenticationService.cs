/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.Owin
{
    public class OwinAuthenticationService : AuthenticationService
    {
        IOwinContext context;
        string authenticationType;

        public OwinAuthenticationService(
            string authenticationType,
            UserAccountService svc,
            IOwinContext ctx,
            ClaimsAuthenticationManager transformer
        )
            : base(svc, transformer)
        {
            this.authenticationType = authenticationType;
            context = ctx;
        }

        public OwinAuthenticationService(
            string authenticationType,
            UserAccountService svc,
            IOwinContext ctx
        )
            : this(authenticationType, svc, ctx, null)
        {
        }

        public OwinAuthenticationService(
            UserAccountService svc,
            IOwinContext ctx
        )
            : this(MembershipRebootOwinConstants.AuthenticationType, svc, ctx, null)
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
    }
}
