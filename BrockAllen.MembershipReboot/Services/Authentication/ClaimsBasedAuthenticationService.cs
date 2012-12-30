using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class ClaimsBasedAuthenticationService : IDisposable
    {
        const int DefaultTokenLifetime_InHours = 10;

        UserAccountService userService;

        public ClaimsBasedAuthenticationService(UserAccountService userService)
        {
            this.userService = userService;
        }

        public void Dispose()
        {
            this.userService.Dispose();
        }

        public virtual void SignIn(string username)
        {
            SignIn(null, username);
        }

        public virtual void SignIn(string tenant, string username)
        {
            if (!SecuritySettings.Instance.MultiTenant)
            {
                tenant = SecuritySettings.Instance.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) throw new ArgumentException("tenant");
            if (String.IsNullOrWhiteSpace(username)) throw new ArgumentException("username");

            // find user
            var account = this.userService.GetByUsername(tenant, username);
            if (account == null) throw new ArgumentException("Invalid username");

            // gather claims
            var claims =
                (from uc in account.Claims
                 select new Claim(uc.Type, uc.Value)).ToList();
            claims.Insert(0, new Claim(ClaimTypes.Email, account.Email));
            claims.Insert(0, new Claim(ClaimTypes.AuthenticationMethod, "password"));
            claims.Insert(0, new Claim(ClaimTypes.AuthenticationInstant, DateTime.UtcNow.ToString("s")));
            claims.Insert(0, new Claim(ClaimTypes.NameIdentifier, account.Username));
            claims.Insert(0, new Claim(ClaimTypes.Name, account.Username));
            claims.Insert(0, new Claim(MembershipRebootConstants.ClaimTypes.Tenant, account.Tenant));

            // create principal/identity
            var id = new ClaimsIdentity(claims, "Forms");
            var cp = new ClaimsPrincipal(id);

            // claims transform
            cp = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.ClaimsAuthenticationManager.Authenticate(String.Empty, cp);

            // issue cookie
            var sam = FederatedAuthentication.SessionAuthenticationModule;
            if (sam == null) throw new Exception("SessionAuthenticationModule is not configured and it needs to be.");
            var token = new SessionSecurityToken(cp, TimeSpan.FromHours(DefaultTokenLifetime_InHours));
            sam.WriteSessionTokenToCookie(token);
        }

        public virtual void SignOut()
        {
            // clear cookie
            var sam = FederatedAuthentication.SessionAuthenticationModule;
            if (sam == null) throw new Exception("SessionAuthenticationModule is not configured and it needs to be.");
            sam.SignOut();
        }
    }
}
