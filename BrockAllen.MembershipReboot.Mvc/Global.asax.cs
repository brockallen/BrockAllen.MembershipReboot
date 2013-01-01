using BrockAllen.MembershipReboot.Mvc.App_Start;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace BrockAllen.MembershipReboot.Mvc
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer<EFMembershipRebootDatabase>(new CreateDatabaseIfNotExists<EFMembershipRebootDatabase>());
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            InitDatabase();
        }

        private void InitDatabase()
        {
            var svc = new UserAccountService(new EFUserAccountRepository(Constants.ConnectionName), null, null);
            if (svc.GetByUsername("admin") == null)
            {
                var account = svc.CreateAccount("admin", "admin123", "brockallen@gmail.com");
                svc.VerifyAccount(account.VerificationKey);
                account.AddClaim(ClaimTypes.Role, "Administrator");
                svc.SaveChanges();
            }
        }
    }
}