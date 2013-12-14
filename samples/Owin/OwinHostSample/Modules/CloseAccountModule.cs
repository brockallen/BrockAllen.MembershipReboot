using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Owin;
using Microsoft.Owin;
using Nancy;
using Nancy.ModelBinding;
using OwinHostSample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace OwinHostSample.Modules
{
    public class CloseAccountModule : NancyModule
    {
        public CloseAccountModule()
            : base("/CloseAccount")
        {
            this.Get[""] = ctx =>
            {
                return View["Index"];
            };

            this.Post[""] = ctx =>
            {
                if (Request.Form["button"] == "yes")
                {
                    var userAccountService = this.Context.GetUserAccountService();
                    userAccountService.DeleteAccount(this.Context.CurrentUser.GetUserID());
                    return this.Response.AsRedirect("~/Logout");
                }
                return View["Index"];
            };
        }
    }
}
