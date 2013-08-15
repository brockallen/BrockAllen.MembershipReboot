using System.Web;
using BrockAllen.MembershipReboot.Mvc.App_Start;
using System.Security.Claims;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using BrockAllen.MembershipReboot.RavenDb;
using Raven.Client;

namespace BrockAllen.MembershipReboot.RavenDbSample
{
    public class MvcApplication : HttpApplication
    {
        public static IDocumentStore DocumentStore { get; private set; }

        protected void Application_Start()
        {
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            InitializeDocumentStore();

            EnsureSeededDatabase();
        }

        private void EnsureSeededDatabase()
        {
            using (var svc = DependencyResolver.Current.GetService<UserAccountService>())
            {
                if (svc.GetByUsername("admin") == null)
                {
                    var account = svc.CreateAccount("admin", "admin123", "brockallen@gmail.com");
                    svc.VerifyAccount(account.VerificationKey);
                    account.AddClaim(ClaimTypes.Role, "Administrator");
                    svc.Update(account);
                }
            }
        }

        private static void InitializeDocumentStore()
        {
            if (DocumentStore != null) return;

            DocumentStore = new RavenMembershipRebootDatabase("RavenDb").DocumentStore;
        }
    }
}