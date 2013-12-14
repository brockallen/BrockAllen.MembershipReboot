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
    public class LoginModule : NancyModule
    {
        public LoginModule()
            : base("/Login")
        {
            this.Get[""] = ctx =>
            {
                return View["Index"];
            };

            this.Post[""] = ctx =>
            {
                var model = this.Bind<LoginInputModel>();

                var userSvc = this.Context.GetUserAccountService();
                UserAccount account;
                if (userSvc.Authenticate(model.Username, model.Password, out account))
                {
                    var authSvc = this.Context.GetAuthenticationService();
                    authSvc.SignIn(account);

                    return this.Response.AsRedirect("~/");
                }
                return View["Index"];
            };
        }

            //this.Get[""] = ctx =>
            //    {
            //        var owin = this.Context.ToOwinContext();
            //        var claims = new Claim[]
            //        {
            //            new Claim(ClaimTypes.Name, "brock")
            //        };
            //        var id = new ClaimsIdentity(claims, "MembershipReboot");
            //        owin.Authentication.SignIn(id);
            //        if (owin.Authentication.User == null)
            //        {
            //            return "anon";
            //        }
            //        else
            //        {
            //            return owin.Authentication.User.Identity.Name;
            //        }
            //    };

            //this.Post[""] = ctx =>
            //    {
            //        try
            //        {
            //            var m = this.Bind<Foo>();
            //            var r = this.Validate<Foo>(m);
            //            this.ModelValidationResult.AddError("name", "foo");
            //            var msg = r.Errors.First().GetMessage(r.Errors.First().MemberNames.First());
            //            //var msg = this.ModelValidationResult.Errors.First().GetMessage("name");
            //        }
            //        catch(ModelValidationException ex)
            //        {
            //            //ex.Message;
            //        }
            //        return View["Post", this.Request.Form];
            //    };
    }
}
