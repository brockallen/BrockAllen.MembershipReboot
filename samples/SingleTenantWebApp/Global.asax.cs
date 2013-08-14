using BrockAllen.MembershipReboot.Ef;
using BrockAllen.MembershipReboot.Mvc.App_Start;
using System.Data.Entity;
using System.Security.Claims;
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
            Database.SetInitializer<DefaultMembershipRebootDatabase>(new CreateDatabaseIfNotExists<DefaultMembershipRebootDatabase>());
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            InitDatabase();
        }

        private void InitDatabase()
        {
            using (var svc = new UserAccountService(new DefaultUserAccountRepository()))
            {
                if (svc.GetByUsername("admin") == null)
                {
                    var account = svc.CreateAccount("admin", "admin123", "brockallen@gmail.com");
                    svc.VerifyAccount(account.VerificationKey);
                    account.AddClaim(ClaimTypes.Role, "Administrator");
                    account.AddClaim(ClaimTypes.Role, "Manager");
                    account.AddClaim(ClaimTypes.Country, "USA");
                    svc.Update(account);
                }
            }
        }
    }
}