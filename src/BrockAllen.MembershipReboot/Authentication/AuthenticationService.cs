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
    public abstract class AuthenticationService : IDisposable
    {
        static readonly TimeSpan TwoFactorAuthTokenLifetime = TimeSpan.FromMinutes(30);

        public UserAccountService UserAccountService { get; set; }

        public AuthenticationService(UserAccountService userService)
        {
            this.UserAccountService = userService;
        }

        public void Dispose()
        {
            if (this.UserAccountService != null)
            {
                this.UserAccountService.Dispose();
                this.UserAccountService = null;
            }
        }

        protected abstract void IssueToken(ClaimsPrincipal principal, TimeSpan? tokenLifetime = null, bool? persistentCookie = null);
        protected abstract void RevokeToken();

        public virtual void SignIn(Guid userID)
        {
            var account = this.UserAccountService.GetByID(userID);
            if (account == null) throw new ArgumentException("Invalid userID");

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

            if (account.RequiresTwoFactorAuthToSignIn)
            {
                Tracing.Verbose(String.Format("[AuthenticationService.SignIn] detected account requires two factor to sign in: {0}", account.ID));
                IssuePartialSignInTokenForTwoFactorAuth(account, method);
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
            IssueToken(cp);
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
            var x509 = from c in account.Certificates
                       select new Claim(ClaimTypes.X500DistinguishedName, c.Subject);
            claims.AddRange(x509);

            return claims;
        }

        private void IssuePartialSignInTokenForTwoFactorAuth(UserAccount account, string method)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Verbose(String.Format("[AuthenticationService.IssuePartialSignInCookieForTwoFactorAuth] Account ID: {0}", account.ID));

            var claims = GetBasicClaims(account, method);

            var ci = new ClaimsIdentity(claims); // no auth type param so user will not be actually authenticated
            var cp = new ClaimsPrincipal(ci);

            IssueToken(cp, TwoFactorAuthTokenLifetime, false);
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
            if (!UserAccountService.Configuration.SecuritySettings.MultiTenant)
            {
                tenant = UserAccountService.Configuration.SecuritySettings.DefaultTenant;
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
                account = this.UserAccountService.GetByID(user.GetUserID());
            }
            else
            {
                // see if there's already an account mapped to this provider
                account = this.UserAccountService.GetByLinkedAccount(tenant, providerName, providerAccountID);
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
                        this.UserAccountService.UsernameExists(tenant, name))
                    {
                        name = email;
                    }

                    // check to see if email already exists
                    if (this.UserAccountService.EmailExists(tenant, email))
                    {
                        throw new ValidationException("Can't login with this provider because the email is already associated with another account. Please login with your local account and then associate the provider.");
                    }

                    // auto-gen a password, they can always reset it later if they want to use the password feature
                    // this is slightly dangerous if we don't do email account verification, so if email account
                    // verification is disabled then we need to be very confident that the external provider has
                    // provided us with a verified email
                    var pwd = CryptoHelper.GenerateSalt();
                    account = this.UserAccountService.CreateAccount(tenant, name, pwd, email);
                }
            }

            if (account == null) throw new Exception("Failed to locate account");

            // add/update the provider with this account
            account.AddOrUpdateLinkedAccount(providerName, providerAccountID, claims);
            this.UserAccountService.Update(account);

            // signin from the account
            // if we want to include the provider's claims, then perhaps this
            // should be done in the claims transformer
            this.SignIn(account, providerName);
        }

        public virtual void SignOut()
        {
            Tracing.Information(String.Format("[AuthenticationService.SignOut] called: {0}", ClaimsPrincipal.Current.Claims.GetValue(ClaimTypes.NameIdentifier)));

            // clear cookie
            RevokeToken();
        }
    }
}
