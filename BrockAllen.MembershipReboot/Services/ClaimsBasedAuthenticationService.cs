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
    public class ClaimsBasedAuthenticationService
    {
        IUserAccountRepository userAccountRepo;
        IUserClaimRepository userClaimRepo;
        
        public ClaimsBasedAuthenticationService(
            IUserAccountRepository userAccountRepo,
            IUserClaimRepository userClaimRepo)
        {
            this.userAccountRepo = userAccountRepo;
            this.userClaimRepo = userClaimRepo;
        }

        public void SignIn(string username)
        {
            var account = this.userAccountRepo.GetByUsername(username);
            if (account == null) throw new ArgumentException("Invalid username: " + username);

            var userClaims = this.userClaimRepo.Get(username);
            
            var claims =
                (from uc in userClaims
                 select new Claim(uc.Type, uc.Value)).ToList();

            claims.Insert(0, new Claim(ClaimTypes.AuthenticationMethod, "password"));
            claims.Insert(0, new Claim(ClaimTypes.AuthenticationInstant, DateTime.UtcNow.ToString("s")));
            claims.Insert(0, new Claim(ClaimTypes.NameIdentifier, account.Username));

            var id = new ClaimsIdentity(claims, "Forms");
            var cp = new ClaimsPrincipal(id);
            cp = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.ClaimsAuthenticationManager.Authenticate(String.Empty, cp);
            
            var sam = FederatedAuthentication.SessionAuthenticationModule;
            if (sam == null) throw new Exception("SessionAuthenticationModule is not configured and it needs to be.");

            var token = new SessionSecurityToken(cp, TimeSpan.FromHours(10));
            sam.WriteSessionTokenToCookie(token);
        }

        public void SignOut()
        {
            var sam = FederatedAuthentication.SessionAuthenticationModule;
            if (sam == null) throw new Exception("SessionAuthenticationModule is not configured and it needs to be.");

            sam.SignOut();
        }
    }
}
