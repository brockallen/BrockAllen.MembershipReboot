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
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            this.Get[""] = ctx =>
                {
                    return View["Index"];
                };
        }
    }
}
