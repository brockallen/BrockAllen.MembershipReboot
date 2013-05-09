using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class LinkedAccountAuthenticationService : ClaimsBasedAuthenticationService
    {
        UserAccountService userAccountService;
        LinkedAccountService linkedAccountService;

        public LinkedAccountAuthenticationService(
            UserAccountService userAccountService, 
            LinkedAccountService linkedAccountService)
            : base(userAccountService)
        {
            this.userAccountService = userAccountService;
            this.linkedAccountService = linkedAccountService;
        }

        public void SignIn(
            string providerName, 
            string providerAccountID, 
            IEnumerable<Claim> externalClaims)
        {
            SignIn(null, providerName, providerAccountID, externalClaims);
        }

        public void SignIn(
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

            var user = ClaimsPrincipal.Current;
            if (user.Identity.IsAuthenticated)
            {
                var nameID = user.Claims.GetValue(ClaimTypes.NameIdentifier);
                var localID = new Guid(nameID);
                this.linkedAccountService.Add(providerName, providerAccountID, localID, claims);
                return;
            }

            UserAccount account = null;

            var linkedAccount = this.linkedAccountService.Get(providerName, providerAccountID);
            if (linkedAccount == null)
            {
                var email = claims.GetValue(ClaimTypes.Email);
                if (String.IsNullOrWhiteSpace(email))
                {
                    throw new ValidationException("Can't create an account because there was no email from the identity provider");
                }

                var name = claims.GetValue(ClaimTypes.Name);
                if (name == null) name = email;
                var pwd = CryptoHelper.GenerateSalt();

                account = this.userAccountService.CreateAccount(tenant, name, pwd, email);
                this.linkedAccountService.Add(providerName, providerAccountID, account.NameID, claims);
            }
            else
            {
                account = this.userAccountService.GetByNameId(linkedAccount.LocalAccountID);
                this.linkedAccountService.Update(linkedAccount, claims);
            }

            if (account == null) throw new Exception("Account not found");

            this.SignIn(account, providerName);
        }
    }
}
