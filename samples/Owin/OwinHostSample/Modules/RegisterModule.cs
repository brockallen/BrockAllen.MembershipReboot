using BrockAllen.MembershipReboot;
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
                    var userAccountService = this.Context.GetUserAccountService();
                    userAccountService.CreateAccount(model.Username, model.Password, model.Email);
                    if (userAccountService.Configuration.RequireAccountVerification)
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
                var userAccountService = this.Context.GetUserAccountService();
                userAccountService.VerifyEmailFromKey((string)this.Request.Form["id"]);
                return View["Confirm"];
            };

            this.Post["Cancel"] = ctx =>
            {
                var userAccountService = this.Context.GetUserAccountService();
                userAccountService.CancelNewAccount((string)this.Request.Form["id"]);
                return View["Cancel"];
            };
            this.Get["Confirm/{id}"] = ctx =>
            {
                return View["Confirm"];
            };
            this.Post["Confirm/{id}"] = ctx =>
            {
                var userAccountService = this.Context.GetUserAccountService();
                userAccountService.VerifyEmailFromKey((string)ctx.id, Request.Form["pass"]);
                return View["Confirmed"];
            };

            this.Get["Cancel/{id}"] = ctx =>
            {
                var userAccountService = this.Context.GetUserAccountService();
                userAccountService.CancelNewAccount((string)ctx.id);
                return View["Cancel"];
            };
        }
    }
}
