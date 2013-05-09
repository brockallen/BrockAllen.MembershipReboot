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

            if (!SecuritySettings.Instance.MultiTenant)
            {
                tenant = SecuritySettings.Instance.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) throw new ArgumentException("tenant");
            if (String.IsNullOrWhiteSpace(username)) throw new ArgumentException("username");

            // find user
            var account = this.userService.GetByUsername(tenant, username);
            if (account == null) throw new ArgumentException("Invalid username");

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

            // gather claims
            var claims =
                (from uc in account.Claims
                 select new Claim(uc.Type, uc.Value)).ToList();

            if (!String.IsNullOrWhiteSpace(account.Email))
            {
                claims.Insert(0, new Claim(ClaimTypes.Email, account.Email));
            }
            claims.Insert(0, new Claim(ClaimTypes.AuthenticationMethod, method));
            claims.Insert(0, new Claim(ClaimTypes.AuthenticationInstant, DateTime.UtcNow.ToString("s")));
            claims.Insert(0, new Claim(ClaimTypes.Name, account.Username));
            claims.Insert(0, new Claim(MembershipRebootConstants.ClaimTypes.Tenant, account.Tenant));
            claims.Insert(0, new Claim(ClaimTypes.NameIdentifier, account.ID.ToString("D")));

            // create principal/identity
            var id = new ClaimsIdentity(claims, method);
            var cp = new ClaimsPrincipal(id);

            // claims transform
            cp = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.ClaimsAuthenticationManager.Authenticate(String.Empty, cp);

            // issue cookie
            var sam = FederatedAuthentication.SessionAuthenticationModule;
            if (sam == null)
            {
                Tracing.Verbose("[ClaimsBasedAuthenticationService.Signin] SessionAuthenticationModule is not configured");
                throw new Exception("SessionAuthenticationModule is not configured and it needs to be.");
            }

            var handler = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.SecurityTokenHandlers[typeof(SessionSecurityToken)] as SessionSecurityTokenHandler;
            if (handler == null)
            {
                Tracing.Verbose("[ClaimsBasedAuthenticationService.Signin] SessionSecurityTokenHandler is not configured");
                throw new Exception("SessionSecurityTokenHandler is not configured and it needs to be.");
            }

            var token = new SessionSecurityToken(cp, handler.TokenLifetime);
            token.IsPersistent = FederatedAuthentication.FederationConfiguration.WsFederationConfiguration.PersistentCookiesOnPassiveRedirects;
            token.IsReferenceMode = sam.IsReferenceMode;

            sam.WriteSessionTokenToCookie(token);

            Tracing.Verbose(String.Format("[ClaimsBasedAuthenticationService.Signin] cookie issued: {0}", claims.GetValue(ClaimTypes.NameIdentifier)));
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
            if (!SecuritySettings.Instance.MultiTenant)
            {
                tenant = SecuritySettings.Instance.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) throw new ArgumentException("tenant");
            if (String.IsNullOrWhiteSpace(providerName)) throw new ArgumentException("providerName");
            if (String.IsNullOrWhiteSpace(providerAccountID)) throw new ArgumentException("providerAccountID");
            if (claims == null) throw new ArgumentNullException("claims");

            UserAccount account = null;
            var user = ClaimsPrincipal.Current;
            if (user.Identity.IsAuthenticated)
            {
                account = this.userService.GetByID(user.Claims.GetValue(ClaimTypes.NameIdentifier));
            }
            else
            {
                account = this.userService.GetByLinkedAccount(providerName, providerAccountID);
                if (account == null)
                {
                    var email = claims.GetValue(ClaimTypes.Email);
                    if (String.IsNullOrWhiteSpace(email))
                    {
                        throw new ValidationException("Can't create an account because there was no email from the identity provider");
                    }

                    var name = claims.GetValue(ClaimTypes.Name);
                    if (name == null) name = email;
                    var pwd = CryptoHelper.GenerateSalt();

                    account = this.userService.CreateAccount(tenant, name, pwd, email);
                }
            }

            if (account == null) throw new Exception("Failed to locate account");

            account.AddOrUpdateLinkedAccount(providerName, providerAccountID, claims);
            this.userService.SaveChanges();

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
