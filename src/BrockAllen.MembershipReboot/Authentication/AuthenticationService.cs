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

        protected abstract ClaimsPrincipal GetCurentPrincipal();
        protected abstract void IssueToken(ClaimsPrincipal principal, TimeSpan? tokenLifetime = null, bool? persistentCookie = null);
        protected abstract void RevokeToken();

        public virtual void SignIn(Guid userID, bool persistent = false)
        {
            var account = this.UserAccountService.GetByID(userID);
            if (account == null) throw new ArgumentException("Invalid userID");

            SignIn(account, AuthenticationMethods.Password, persistent);
        }

        public virtual void SignIn(TAccount account, bool persistent = false)
        {
            SignIn(account, AuthenticationMethods.Password, persistent);
        }

        public virtual void SignIn(TAccount account, string method, bool persistent = false)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (String.IsNullOrWhiteSpace(method)) throw new ArgumentNullException("method");

            Tracing.Information("[AuthenticationService.SignIn] sign in called: {0}", account.ID);

            if (!account.IsLoginAllowed || account.IsAccountClosed)
            {
                throw new ValidationException(UserAccountService.GetValidationMessage(MembershipRebootConstants.ValidationMessages.LoginNotAllowed));
            }

            if (!account.IsAccountVerified && UserAccountService.Configuration.RequireAccountVerification)
            {
                throw new ValidationException(UserAccountService.GetValidationMessage(MembershipRebootConstants.ValidationMessages.AccountNotVerified));
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
            var claims = GetAllClaims(account, method);
            
            // get custom claims from properties
            claims.AddRange(this.UserAccountService.MapClaims(account));

            // create principal/identity
            var id = new ClaimsIdentity(claims, method);
            var cp = new ClaimsPrincipal(id);

            // claims transform
            if (this.ClaimsAuthenticationManager != null)
            {
                cp = ClaimsAuthenticationManager.Authenticate(String.Empty, cp);
            }

            // issue cookie
            Tracing.Verbose("[AuthenticationService.SignIn] token issued: {0}", account.ID);
            IssueToken(cp, persistentCookie: persistent);
        }

        private static List<Claim> GetBasicClaims(TAccount account, string method)
        {
            if (account == null) throw new ArgumentNullException("account");

            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.AuthenticationMethod, method));
            claims.Add(new Claim(ClaimTypes.AuthenticationInstant, DateTime.UtcNow.ToString("s")));
            claims.AddRange(account.GetIdentificationClaims());

            return claims;
        }
        
        private static List<Claim> GetAllClaims(TAccount account, string method)
        {
            if (account == null) throw new ArgumentNullException("account");

            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.AuthenticationMethod, method));
            claims.Add(new Claim(ClaimTypes.AuthenticationInstant, DateTime.UtcNow.ToString("s")));
            claims.AddRange(account.GetAllClaims());

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

            Tracing.Information("[AuthenticationService.SignInWithLinkedAccount] tenant: {0}, provider: {1}, id: {2}", tenant, providerName, providerAccountID);

            var user = GetCurentPrincipal();
            if (user != null && user.Identity.IsAuthenticated)
            {
                // already logged in, so use the current user's account
                Tracing.Verbose("[AuthenticationService.SignInWithLinkedAccount] user already logged in as: {0}", user.Identity.Name);
                account = this.UserAccountService.GetByID(user.GetUserID());
            }
            else
            {
                // see if there's already an account mapped to this provider
                account = this.UserAccountService.GetByLinkedAccount(tenant, providerName, providerAccountID);
                if (account == null)
                {
                    Tracing.Verbose("[AuthenticationService.SignInWithLinkedAccount] linked account not found");
                    
                    // no account associated, so create one
                    // we need email
                    var email = claims.GetValue(ClaimTypes.Email);
                    if (String.IsNullOrWhiteSpace(email))
                    {
                        throw new ValidationException(UserAccountService.GetValidationMessage(MembershipRebootConstants.ValidationMessages.AccountCreateFailNoEmailFromIdp));
                    }

                    // check to see if email already exists
                    if (this.UserAccountService.EmailExists(tenant, email))
                    {
                        throw new ValidationException(UserAccountService.GetValidationMessage(MembershipRebootConstants.ValidationMessages.LoginFailEmailAlreadyAssociated));
                    }

                    // guess at a username to use
                    var name = claims.GetValue(ClaimTypes.Name);
                    // remove whitespace
                    if (name != null) name = new String(name.Where(x => Char.IsLetterOrDigit(x)).ToArray());
                    
                    // check to see if username already exists
                    if (String.IsNullOrWhiteSpace(name) || this.UserAccountService.UsernameExists(tenant, name))
                    {
                        // try use email for name then
                        name = email.Substring(0, email.IndexOf('@'));
                        name = new String(name.Where(x=>Char.IsLetterOrDigit(x)).ToArray());

                        if (this.UserAccountService.UsernameExists(tenant, name))
                        {
                            // gen random username -- this isn't ideal but 
                            // they should always be able to change it later
                            name = Guid.NewGuid().ToString("N");
                        }
                    }

                    // create account without password -- user can verify their email and then 
                    // do a password reset to assign password
                    Tracing.Verbose("[AuthenticationService.SignInWithLinkedAccount] creating account: {0}, {1}", name, email);
                    account = this.UserAccountService.CreateAccount(tenant, name, null, email);

                    // update account with external claims
                    var cmd = new MapClaimsToAccount<TAccount> { Account = account, Claims = claims };
                    this.UserAccountService.ExecuteCommand(cmd);
                    this.UserAccountService.Update(account);
                }
                else
                {
                    Tracing.Verbose("[AuthenticationService.SignInWithLinkedAccount] linked account found: {0}", account.ID);
                }
            }

            if (account == null) throw new Exception("Failed to locate account");

            // add/update the provider with this account
            this.UserAccountService.AddOrUpdateLinkedAccount(account, providerName, providerAccountID, claims);

            // log them in if the account if they're verified
            if (account.IsAccountVerified || !UserAccountService.Configuration.RequireAccountVerification)
            {
                Tracing.Verbose("[AuthenticationService.SignInWithLinkedAccount] signing user in: {0}", account.ID);
                // signin from the account
                // if we want to include the provider's claims, then perhaps this
                // should be done in the claims transformer
                this.SignIn(account, providerName);
            }
            else
            {
                Tracing.Error("[AuthenticationService.SignInWithLinkedAccount] user account not verified, not allowed to login: {0}", account.ID);
            }
        }

        public virtual void SignOut()
        {
            var p = this.GetCurentPrincipal();
            if (p.HasUserID())
            {
                Tracing.Information("[AuthenticationService.SignOut] called: {0}", p.GetUserID());
            }

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
