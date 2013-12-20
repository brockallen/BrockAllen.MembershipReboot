using BrockAllen.MembershipReboot.Mvc.App_Start;
using System.Globalization;
using System.Security.Claims;
using System.Threading;
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
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        void Application_BeginRequest()
        {
            //Thread.CurrentThread.CurrentCulture =
            //    Thread.CurrentThread.CurrentUICulture =
            //    CultureInfo.CreateSpecificCulture("ru-ru");
        }
    }
}