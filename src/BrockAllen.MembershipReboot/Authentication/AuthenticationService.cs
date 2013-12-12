/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;

namespace BrockAllen.MembershipReboot
{
    public abstract class AuthenticationService<TAccount>
        where TAccount : UserAccount
    {
        public UserAccountService<TAccount> UserAccountService { get; set; }
        public ClaimsAuthenticationManager ClaimsAuthenticationManager { get; set; }

        public AuthenticationService(UserAccountService<TAccount> userService)
            : this(userService, null)
        {
        }

        public AuthenticationService(UserAccountService<TAccount> userService, ClaimsAuthenticationManager claimsAuthenticationManager)
        {
            this.UserAccountService = userService;
            this.ClaimsAuthenticationManager = claimsAuthenticationManager;
        }

        protected abstract void IssueToken(ClaimsPrincipal principal, TimeSpan? tokenLifetime = null, bool? persistentCookie = null);
        protected abstract void RevokeToken();

        public virtual void SignIn(Guid userID)
        {
            var account = this.UserAccountService.GetByID(userID);
            if (account == null) throw new ArgumentException("Invalid userID");

            SignIn(account, AuthenticationMethods.Password);
        }

        public virtual void SignIn(TAccount account)
        {
            SignIn(account, AuthenticationMethods.Password);
        }

        public virtual void SignIn(TAccount account, string method)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (String.IsNullOrWhiteSpace(method)) throw new ArgumentNullException("method");

            if (!account.IsLoginAllowed)
            {
                throw new ValidationException(Resources.ValidationMessages.LoginNotAllowed);
            }

            if (!account.IsAccountVerified && UserAccountService.Configuration.RequireAccountVerification)
            {
                throw new ValidationException(Resources.ValidationMessages.AccountNotVerified);
            }

            if (account.RequiresTwoFactorAuthToSignIn() || 
                account.RequiresPasswordReset || 
                this.UserAccountService.IsPasswordExpired(account))
            {
                Tracing.Verbose("[AuthenticationService.SignIn] detected account requires two factor or password reset to sign in: {0}", account.ID);
                IssuePartialSignInToken(account, method);
                return;
            }

            // gather claims
            var claims = GetBasicClaims(account, method);
            
            // get the rest
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
            var otherClaims =
                (from uc in account.Claims
                 select new Claim(uc.Type, uc.Value)).ToList();
            claims.AddRange(otherClaims);

            // get custom claims from properties
            if (this.UserAccountService.Configuration.CustomUserPropertiesToClaimsMap != null)
            {
                claims.AddRange(this.UserAccountService.Configuration.CustomUserPropertiesToClaimsMap(account));
            }

            // create principal/identity
            var id = new ClaimsIdentity(claims, method);
            var cp = new ClaimsPrincipal(id);

            // claims transform
            if (this.ClaimsAuthenticationManager != null)
            {
                cp = ClaimsAuthenticationManager.Authenticate(String.Empty, cp);
            }

            // issue cookie
            IssueToken(cp);
        }

        private static List<Claim> GetBasicClaims(TAccount account, string method)
        {
            if (account == null) throw new ArgumentNullException("account");

            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.AuthenticationMethod, method));
            claims.Add(new Claim(ClaimTypes.AuthenticationInstant, DateTime.UtcNow.ToString("s")));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, account.ID.ToString("D")));
            claims.Add(new Claim(ClaimTypes.Name, account.Username));
            claims.Add(new Claim(MembershipRebootConstants.ClaimTypes.Tenant, account.Tenant));

            return claims;
        }

        private void IssuePartialSignInToken(TAccount account, string method)
        {
            if (account == null) throw new ArgumentNullException("account");

            Tracing.Verbose("[AuthenticationService.IssuePartialSignInCookieForTwoFactorAuth] Account ID: {0}", account.ID);

            var claims = GetBasicClaims(account, method);

            var ci = new ClaimsIdentity(claims); // no auth type param so user will not be actually authenticated
            var cp = new ClaimsPrincipal(ci);

            IssueToken(cp, MembershipRebootConstants.AuthenticationService.TwoFactorAuthTokenLifetime, false);
        }

        public void SignInWithLinkedAccount(
                    string providerName,
                    string providerAccountID,
                    IEnumerable<Claim> externalClaims)
        {
            SignInWithLinkedAccount(null, providerName, providerAccountID, externalClaims);
        }

        public void SignInWithLinkedAccount(
           string providerName,
           string providerAccountID,
           IEnumerable<Claim> externalClaims,
           out TAccount account)
        {
            SignInWithLinkedAccount(null, providerName, providerAccountID, externalClaims, out account);
        }

        public void SignInWithLinkedAccount(
            string tenant,
            string providerName,
            string providerAccountID,
            IEnumerable<Claim> claims)
        {
            TAccount account;
            SignInWithLinkedAccount(null, providerName, providerAccountID, claims, out account);
        }

        public void SignInWithLinkedAccount(
            string tenant,
            string providerName,
            string providerAccountID,
            IEnumerable<Claim> claims,
            out TAccount account)
        {
            account = null;

            if (!UserAccountService.Configuration.MultiTenant)
            {
                tenant = UserAccountService.Configuration.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) throw new ArgumentException("tenant");
            if (String.IsNullOrWhiteSpace(providerName)) throw new ArgumentException("providerName");
            if (String.IsNullOrWhiteSpace(providerAccountID)) throw new ArgumentException("providerAccountID");
            if (claims == null) throw new ArgumentNullException("claims");

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
                        throw new ValidationException(Resources.ValidationMessages.AccountCreateFailNoEmailFromIdp);
                    }

                    // guess at a name to use
                    var name = claims.GetValue(ClaimTypes.Name);
                    if (name == null ||
                        this.UserAccountService.UsernameExists(tenant, name))
                    {
                        name = email;
                    }
                    else
                    {
                        name = name.Replace(" ", "");
                    }

                    // check to see if email already exists
                    if (this.UserAccountService.EmailExists(tenant, email))
                    {
                        throw new ValidationException(Resources.ValidationMessages.LoginFailEmailAlreadyAssociated);
                    }

                    // auto-gen a password, they can always reset it later if they want to use the password feature
                    // this is slightly dangerous if we don't do email account verification, so if email account
                    // verification is disabled then we need to be very confident that the external provider has
                    // provided us with a verified email
                    account = this.UserAccountService.CreateAccount(tenant, name, null, email);
                }
            }

            if (account == null) throw new Exception("Failed to locate account");

            // add/update the provider with this account
            this.UserAccountService.AddOrUpdateLinkedAccount(account, providerName, providerAccountID, claims);
            //this.UserAccountService.Update(account);

            // log them in if the account if they're verified
            if (account.IsAccountVerified || !UserAccountService.Configuration.RequireAccountVerification)
            {
                // signin from the account
                // if we want to include the provider's claims, then perhaps this
                // should be done in the claims transformer
                this.SignIn(account, providerName);
            }
        }

        public virtual void SignOut()
        {
            Tracing.Information("[AuthenticationService.SignOut] called: {0}", ClaimsPrincipal.Current.Claims.GetValue(ClaimTypes.NameIdentifier));

            // clear cookie
            RevokeToken();
        }
    }
    
    public abstract class AuthenticationService : AuthenticationService<UserAccount>
    {
        public new UserAccountService UserAccountService
        {
            get { return (UserAccountService)base.UserAccountService; }
            set { base.UserAccountService = value; }
        }

        public AuthenticationService(UserAccountService userService)
            : this(userService, null)
        {
        }

        public AuthenticationService(UserAccountService userService, ClaimsAuthenticationManager claimsAuthenticationManager)
            : base(userService, claimsAuthenticationManager)
        {
        }
    }
}
