using Microsoft.Owin;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Validation;
using OwinHostSample.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace OwinHostSample.Modules
{
    public class RegisterModule : NancyModule
    {
        public RegisterModule()
            : base("/Register")
        {
            this.Get[""] = ctx =>
                {
                    return View["Index", new RegisterInputModel()];
                };
            this.Post[""] = ctx =>
            {
                var model = this.Bind<RegisterInputModel>();
                try
                {
                    var userAccountService = this.Context.ToOwinContext().GetUserAccountService();
                    userAccountService.CreateAccount(model.Username, model.Password, model.Email);
                    if (userAccountService.Configuration.SecuritySettings.RequireAccountVerification)
                    {
                        return View["Success", model];
                    }
                    else
                    {
                        return View["Confirm", true];
                    }
                }
                catch (ValidationException)
                {
                    //ModelState.AddModelError("", ex.Message);
                }

                return View["Index", model];
            };

            this.Post["Confirm"] = ctx =>
            {
                var userAccountService = this.Context.ToOwinContext().GetUserAccountService();
                var result = userAccountService.VerifyAccount((string)this.Request.Form["id"]);
                return View["Confirm", result];
            };

            this.Post["Cancel"] = ctx =>
            {
                var userAccountService = this.Context.ToOwinContext().GetUserAccountService();
                var result = userAccountService.CancelNewAccount((string)this.Request.Form["id"]);
                return View["Cancel", result];
            };
        }
    }
}
