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

        IUserAccountRepository userRepository;

        public ClaimsBasedAuthenticationService(
            IUserAccountRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public void Dispose()
        {
            this.userRepository.Dispose();
        }

        public virtual void SignIn(string username)
        {
            // find user
            var account = this.userRepository.GetByUsername(username);
            if (account == null) throw new ArgumentException("Invalid username");

            // gather claims
            var claims =
                (from uc in account.Claims
                 select new Claim(uc.Type, uc.Value)).ToList();
            claims.Insert(0, new Claim(ClaimTypes.Email, account.Email));
            claims.Insert(0, new Claim(ClaimTypes.AuthenticationMethod, "password"));
            claims.Insert(0, new Claim(ClaimTypes.AuthenticationInstant, DateTime.UtcNow.ToString("s")));
            claims.Insert(0, new Claim(ClaimTypes.NameIdentifier, account.Username));

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
