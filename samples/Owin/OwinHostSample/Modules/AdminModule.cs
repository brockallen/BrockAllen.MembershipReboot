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
using Owin;
using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Ef;

namespace OwinHostSample.Modules
{
    public class AdminModule : NancyModule
    {
        public AdminModule()
            : base("/Admin")
        {
            this.Get[""] = ctx =>
                {
                    var db = new DefaultMembershipRebootDatabase();
                    var names =
                        from a in db.Users
                        select a;
                    return View["Index", names.ToArray()];
                };
            
            this.Get["Detail/{id}"] = ctx =>
            {
                var id = ctx.id;
                var userAccountService = this.Context.GetUserAccountService();
                var account = userAccountService.GetByID(Guid.Parse(id));

                return View["Detail", new { account }];
            };
        }
    }
}
