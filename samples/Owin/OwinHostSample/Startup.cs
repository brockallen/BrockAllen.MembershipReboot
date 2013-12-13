using BrockAllen.MembershipReboot;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

[assembly: Microsoft.Owin.OwinStartup(typeof(OwinHostSample.Startup))]

namespace OwinHostSample
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            InitDb();

            app.ConfigureMembershipReboot();
            app.UseNancy();
        }

        private void InitDb()
        {
            using (var db = new BrockAllen.MembershipReboot.Ef.DefaultUserAccountRepository())
            {
                var svc = new UserAccountService(db);
                if (svc.GetByUsername("admin") == null)
                {
                    var account = svc.CreateAccount("admin", "admin123", "brockallen@gmail.com");
                    svc.VerifyAccount(account.VerificationKey);

                    account = svc.GetByID(account.ID);
                    account.AddClaim(ClaimTypes.Role, "Administrator");
                    account.AddClaim(ClaimTypes.Role, "Manager");
                    account.AddClaim(ClaimTypes.Country, "USA");
                    svc.Update(account);
                }
            }
        }
    }
}