/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;

namespace BrockAllen.MembershipReboot
{
    public class ClaimsBasedAuthenticationService : IDisposable
    {
        static readonly TimeSpan TwoFactorAuthTokenLifetime = TimeSpan.FromMinutes(30);

        UserAccountService userService;

        public ClaimsBasedAuthenticationService(UserAccountService userService)
        {
            this.userService = userService;
        }

        public void Dispose()
        {
            if (this.userService != null)
            {
                this.userService.Dispose();
                this.userService = null;
            }
        }

        public virtual void SignIn(Guid userID)
        {
            var account = this.userService.GetByID(userID);
            if (account == null) throw new ArgumentException("Invalid userID");

            SignIn(account, AuthenticationMethods.Password);
        }

        public virtual void SignIn(string username)
        {
            SignIn((string)null, username);
        }

        public virtual void SignIn(string tenant, string username)
        {
            Tracing.Information(String.Format("[ClaimsBasedAuthenticationService.Signin] called: {0}, {1}", tenant, username));

            if (!userService.Configuration.SecuritySettings.MultiTenant)
            {
                tenant = userService.Configuration.SecuritySettings.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) throw new ArgumentException("tenant");
            if (String.IsNullOrWhiteSpace(username)) throw new ArgumentException("username");

            // find user
            var account = this.userService.GetByUsername(tenant, username);
            if (account == null) throw new ArgumentException("Invalid username");

            SignIn(account, AuthenticationMethods.Password);
        }

        public virtual void SignIn(UserAccount account)
        {
            SignIn(account, AuthenticationMethods.Password);
        }

        public virtual void SignIn(UserAccount account, string method)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (String.IsNullOrWhiteSpace(method)) throw new ArgumentNullException("method");

            if (!account.IsAccountVerified)
            {
                throw new ValidationException("Account not yet verified");
            }

            if (!account.IsLoginAllowed)
            {
                throw new ValidationException("Login not allowed for this account");
            }

            if (account.RequiresTwoFactorAuthCodeToSignIn)
            {
                Tracing.Verbose(String.Format("[ClaimsBasedAuthenticationService.SignIn] detected account requires two factor auth code to sign in: {0}", account.ID));
                IssuePartialSignInCookieForTwoFactorAuth(account);
                return;
            }

            // gather claims
            var claims = GetBasicClaims(account, method);
            var otherClaims =
                (from uc in account.Claims
                 select new Claim(uc.Type, uc.Value)).ToList();
            claims.AddRange(otherClaims);

            // create principal/identity
            var id = new ClaimsIdentity(claims, method);
            var cp = new ClaimsPrincipal(id);

            // claims transform
            cp = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.ClaimsAuthenticationManager.Authenticate(String.Empty, cp);

            // issue cookie
            IssueCookie(cp);
        }

        private static List<Claim> GetBasicClaims(UserAccount account, string method)
        {
            if (account == null) throw new ArgumentNullException("account");

            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.AuthenticationMethod, method));
            claims.Add(new Claim(ClaimTypes.AuthenticationInstant, DateTime.UtcNow.ToString("s")));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, account.ID.ToString("D")));
            claims.Add(new Claim(ClaimTypes.Name, account.Username));
            claims.Add(new Claim(MembershipRebootConstants.ClaimTypes.Tenant, account.Tenant));
            if (!String.IsNullOrWhiteSpace(account.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, account.Email));
            }
            if (!String.IsNullOrWhiteSpace(account.MobilePhoneNumber))
            {
                claims.Add(new Claim(ClaimTypes.MobilePhone, account.MobilePhoneNumber));
            }

            return claims;
        }

        void IssueCookie(ClaimsPrincipal principal)
        {
            var handler = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.SecurityTokenHandlers[typeof(SessionSecurityToken)] as SessionSecurityTokenHandler;
            if (handler == null)
            {
                Tracing.Verbose("[ClaimsBasedAuthenticationService.Signin] SessionSecurityTokenHandler is not configured");
                throw new Exception("SessionSecurityTokenHandler is not configured and it needs to be.");
            }
            
            IssueCookie(principal, handler.TokenLifetime, FederatedAuthentication.FederationConfiguration.WsFederationConfiguration.PersistentCookiesOnPassiveRedirects);
        }

        void IssueCookie(ClaimsPrincipal principal, TimeSpan tokenLifetime, bool persistentCookie)
        {
            if (principal == null) throw new ArgumentNullException("principal");

            var sam = FederatedAuthentication.SessionAuthenticationModule;
            if (sam == null)
            {
                Tracing.Verbose("[ClaimsBasedAuthenticationService.Signin] SessionAuthenticationModule is not configured");
                throw new Exception("SessionAuthenticationModule is not configured and it needs to be.");
            }

            var token = new SessionSecurityToken(principal, tokenLifetime);
            token.IsPersistent = persistentCookie;
            token.IsReferenceMode = sam.IsReferenceMode;

            sam.WriteSessionTokenToCookie(token);

            Tracing.Verbose(String.Format("[ClaimsBasedAuthenticationService.WriteCookie] cookie issued: {0}", principal.Claims.GetValue(ClaimTypes.NameIdentifier)));
        }

        private void IssuePartialSignInCookieForTwoFactorAuth(UserAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Verbose(String.Format("[ClaimsBasedAuthenticationService.IssuePartialSignInCookieForTwoFactorAuth] Account ID: {0}", account.ID));

            var claims = GetBasicClaims(account, AuthenticationMethods.Password);

            var ci = new ClaimsIdentity(claims); // no auth type param so user will not be actually authenticated
            var cp = new ClaimsPrincipal(ci);

            IssueCookie(cp, TwoFactorAuthTokenLifetime, false);
        }

        public void SignInWithLinkedAccount(
                    string providerName,
                    string providerAccountID,
                    IEnumerable<Claim> externalClaims)
        {
            SignInWithLinkedAccount(null, providerName, providerAccountID, externalClaims);
        }

        public void SignInWithLinkedAccount(
            string tenant,
            string providerName,
            string providerAccountID,
            IEnumerable<Claim> claims)
        {
            if (!userService.Configuration.SecuritySettings.MultiTenant)
            {
                tenant = userService.Configuration.SecuritySettings.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) throw new ArgumentException("tenant");
            if (String.IsNullOrWhiteSpace(providerName)) throw new ArgumentException("providerName");
            if (String.IsNullOrWhiteSpace(providerAccountID)) throw new ArgumentException("providerAccountID");
            if (claims == null) throw new ArgumentNullException("claims");

            UserAccount account = null;
            var user = ClaimsPrincipal.Current;
            if (user.Identity.IsAuthenticated)
            {
                // already logged in, so use the current user's account
                account = this.userService.GetByID(user.Claims.GetValue(ClaimTypes.NameIdentifier));
            }
            else
            {
                // see if there's already an account mapped to this provider
                account = this.userService.GetByLinkedAccount(tenant, providerName, providerAccountID);
                if (account == null)
                {
                    // no account associated, so create one
                    // we need email
                    var email = claims.GetValue(ClaimTypes.Email);
                    if (String.IsNullOrWhiteSpace(email))
                    {
                        throw new ValidationException("Can't create an account because there was no email from the identity provider");
                    }

                    // guess at a name to use
                    var name = claims.GetValue(ClaimTypes.Name);
                    if (name == null ||
                        this.userService.UsernameExists(tenant, name))
                    {
                        name = email;
                    }

                    // check to see if email already exists
                    if (this.userService.EmailExists(tenant, email))
                    {
                        throw new ValidationException("Can't login with this provider because the email is already associated with another account. Please login with your local account and then associate the provider.");
                    }

                    var pwd = CryptoHelper.GenerateSalt();
                    account = this.userService.CreateAccount(tenant, name, pwd, email);
                }
            }

            if (account == null) throw new Exception("Failed to locate account");

            // add/update the provider with this account
            account.AddOrUpdateLinkedAccount(providerName, providerAccountID, claims);
            this.userService.Update(account);

            // signin from the account
            // if we want to include the provider's claims, then perhaps this
            // should be done in the claims transformer
            this.SignIn(account, providerName);
        }

        public virtual void SignOut()
        {
            Tracing.Information(String.Format("[ClaimsBasedAuthenticationService.SignOut] called: {0}", ClaimsPrincipal.Current.Claims.GetValue(ClaimTypes.NameIdentifier)));

            // clear cookie
            var sam = FederatedAuthentication.SessionAuthenticationModule;
            if (sam == null)
            {
                Tracing.Verbose("[ClaimsBasedAuthenticationService.Signout] SessionAuthenticationModule is not configured");
                throw new Exception("SessionAuthenticationModule is not configured and it needs to be.");
            }

            sam.SignOut();
        }
    }
}
