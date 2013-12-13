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

namespace OwinHostSample.Modules
{
    public class AdminModule : NancyModule
    {
        public AdminModule()
            : base("/Admin")
        {
            this.Get[""] = ctx =>
                {
                    var userAccountService = this.Context.ToOwinContext().GetUserAccountService();
                    var names =
                        from a in userAccountService.GetAll()
                        select a;
                    return View["Index", names.ToArray()];
                };
            
            this.Get["Detail/{id}"] = ctx =>
            {
                var id = ctx.id;
                var userAccountService = this.Context.ToOwinContext().GetUserAccountService();
                var account = userAccountService.GetByID(Guid.Parse(id));

                return View["Detail", new { account }];
            };
        }
    }
}
