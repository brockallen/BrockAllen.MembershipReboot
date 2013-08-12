using System.Web.Mvc;
using System.Web.Routing;

namespace LinkedAccounts
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //BrockAllen.OAuth2.OAuth2Client.AutoRegisterOAuthCallbackUrl = true;
            BrockAllen.OAuth2.OAuth2Client.RegisterCustomOAuthCallback(RouteTable.Routes, "OAuthCallback", "Home");

            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            AutofacConfig.Register();
        }
    }
}