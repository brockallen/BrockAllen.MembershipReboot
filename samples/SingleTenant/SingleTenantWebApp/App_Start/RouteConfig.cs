﻿using System.Web.Mvc;
using System.Web.Routing;

namespace BrockAllen.MembershipReboot.Mvc.App_Start
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "BrockAllen.MembershipReboot.Mvc.Controllers" }
            );
        }
    }
}