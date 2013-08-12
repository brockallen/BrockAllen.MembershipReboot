using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Ef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace CertificateLogin
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            PopulateTestData();
        }

        private void PopulateTestData()
        {
            using (var svc = new UserAccountService(new DefaultUserAccountRepository()))
            {
                if (svc.GetByUsername("admin") == null)
                {
                    var account = svc.CreateAccount("admin", "admin123", "brockallen@gmail.com");
                    svc.VerifyAccount(account.VerificationKey);
                    account.AddClaim(ClaimTypes.Role, "Administrator");
                    var cert = new X509Certificate2(HttpContext.Current.Server.MapPath("~/App_Data/Brock.cer"));
                    account.AddCertificate(cert);
                    svc.Update(account);
                }
            }
        }
    }
}