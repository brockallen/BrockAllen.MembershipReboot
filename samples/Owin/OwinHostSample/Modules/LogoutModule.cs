using Microsoft.Owin;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace OwinHostSample.Modules
{
    public class LogoutModule : NancyModule
    {
        public LogoutModule()
            : base("/Logout")
        {
            this.Get[""] = ctx =>
            {
                this.Context.GetOwinAuthentication().SignOut();
                return View["Index"];
            };
        }
   }
}
