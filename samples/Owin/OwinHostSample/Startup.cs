using BrockAllen.MembershipReboot;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

[assembly: Microsoft.Owin.OwinStartup(typeof(OwinHostSample.Startup))]

namespace OwinHostSample
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.ConfigureMembershipReboot();
            app.UseNancy();
        }
    }
}