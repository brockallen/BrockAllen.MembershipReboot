/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Security.Claims;

namespace BrockAllen.MembershipReboot.WebHost
{
    public class SamAuthenticationService : AuthenticationService
    {
        public SamAuthenticationService(UserAccountService userAccountService)
            : base(userAccountService)
        {
        }

        protected override void IssueToken(ClaimsPrincipal principal, TimeSpan? tokenLifetime = null, bool? persistentCookie = null)
        {
            if (principal == null) throw new ArgumentNullException("principal");

            if (tokenLifetime == null)
            {
                var handler = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.SecurityTokenHandlers[typeof(SessionSecurityToken)] as SessionSecurityTokenHandler;
                if (handler == null)
                {
                    Tracing.Verbose("[SamAuthenticationService.IssueToken] SessionSecurityTokenHandler is not configured");
                    throw new Exception("SessionSecurityTokenHandler is not configured and it needs to be.");
                }

                tokenLifetime = handler.TokenLifetime;
            }

            if (persistentCookie == null)
            {
                persistentCookie = FederatedAuthentication.FederationConfiguration.WsFederationConfiguration.PersistentCookiesOnPassiveRedirects;
            }

            var sam = FederatedAuthentication.SessionAuthenticationModule;
            if (sam == null)
            {
                Tracing.Verbose("[SamAuthenticationService.IssueToken] SessionAuthenticationModule is not configured");
                throw new Exception("SessionAuthenticationModule is not configured and it needs to be.");
            }

            var token = new SessionSecurityToken(principal, tokenLifetime.Value);
            token.IsPersistent = persistentCookie.Value;
            token.IsReferenceMode = sam.IsReferenceMode;

            sam.WriteSessionTokenToCookie(token);

            Tracing.Verbose("[SamAuthenticationService.IssueToken] cookie issued: {0}", principal.Claims.GetValue(ClaimTypes.NameIdentifier));
        }

        protected override void RevokeToken()
        {
            var sam = FederatedAuthentication.SessionAuthenticationModule;
            if (sam == null)
            {
                Tracing.Verbose("[SamAuthenticationService.RevokeToken] SessionAuthenticationModule is not configured");
                throw new Exception("SessionAuthenticationModule is not configured and it needs to be.");
            }

            sam.SignOut();
        }
    }

    public class SamAuthenticationService<T> : AuthenticationService<T>
        where T : UserAccount
    {
        public SamAuthenticationService(UserAccountService<T> userService)
            : base(userService, FederatedAuthentication.FederationConfiguration.IdentityConfiguration.ClaimsAuthenticationManager)
        {
        }

        protected override void IssueToken(ClaimsPrincipal principal, TimeSpan? tokenLifetime = null, bool? persistentCookie = null)
        {
            if (principal == null) throw new ArgumentNullException("principal");

            if (tokenLifetime == null)
            {
                var handler = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.SecurityTokenHandlers[typeof(SessionSecurityToken)] as SessionSecurityTokenHandler;
                if (handler == null)
                {
                    Tracing.Verbose("[SamAuthenticationService.IssueToken] SessionSecurityTokenHandler is not configured");
                    throw new Exception("SessionSecurityTokenHandler is not configured and it needs to be.");
                }
                
                tokenLifetime = handler.TokenLifetime;
            }

            if (persistentCookie == null)
            {
                persistentCookie = FederatedAuthentication.FederationConfiguration.WsFederationConfiguration.PersistentCookiesOnPassiveRedirects;
            }

            var sam = FederatedAuthentication.SessionAuthenticationModule;
            if (sam == null)
            {
                Tracing.Verbose("[SamAuthenticationService.IssueToken] SessionAuthenticationModule is not configured");
                throw new Exception("SessionAuthenticationModule is not configured and it needs to be.");
            }

            var token = new SessionSecurityToken(principal, tokenLifetime.Value);
            token.IsPersistent = persistentCookie.Value;
            token.IsReferenceMode = sam.IsReferenceMode;
            
            sam.WriteSessionTokenToCookie(token);

            Tracing.Verbose("[SamAuthenticationService.IssueToken] cookie issued: {0}", principal.Claims.GetValue(ClaimTypes.NameIdentifier));
        }

        protected override void RevokeToken()
        {
            var sam = FederatedAuthentication.SessionAuthenticationModule;
            if (sam == null)
            {
                Tracing.Verbose("[SamAuthenticationService.RevokeToken] SessionAuthenticationModule is not configured");
                throw new Exception("SessionAuthenticationModule is not configured and it needs to be.");
            }

            sam.SignOut();
        }
    }
}
