/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using BrockAllen.MembershipReboot.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owin
{
    public static class AppBuilderExtensions
    {
        public static void UseMembershipReboot(this IAppBuilder app, CookieSecureOption cookieMode = CookieSecureOption.Always)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,
                AuthenticationType = MembershipRebootOwinConstants.AuthenticationType, 
                CookieSecure = cookieMode
            });
            
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Passive,
                AuthenticationType = MembershipRebootOwinConstants.AuthenticationTwoFactorType,
                CookieSecure = cookieMode
            });
        }
    }
}
