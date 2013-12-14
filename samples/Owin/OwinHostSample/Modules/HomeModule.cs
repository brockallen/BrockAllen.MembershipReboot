using Microsoft.Owin;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Web;
using BrockAllen.MembershipReboot;

namespace OwinHostSample.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            this.Get[""] = ctx =>
                {
                    return View["Index"];
                };
            this.Post[""] = ctx =>
            {
                var gender = (string)this.Request.Form["gender"];
                var userAccountService = this.Context.GetUserAccountService();

                if (String.IsNullOrWhiteSpace(gender))
                {
                    userAccountService.RemoveClaim(this.Context.CurrentUser.GetUserID(), ClaimTypes.Gender);
                }
                else
                {
                    // if you only want one of these claim types, uncomment the next line
                    //account.RemoveClaim(ClaimTypes.Gender);
                    userAccountService.AddClaim(this.Context.CurrentUser.GetUserID(), ClaimTypes.Gender, gender);
                }

                // since we've changed the claims, we need to re-issue the cookie that
                // contains the claims.
                var authSvc = this.Context.GetAuthenticationService();
                authSvc.SignIn(this.Context.CurrentUser.GetUserID());

                return this.Response.AsRedirect("~/");
            };
        }
    }
}
