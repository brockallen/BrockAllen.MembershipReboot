/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.Owin
{
    public class MembershipRebootOwinConstants
    {
        internal const string OwinAuthenticationService = "MembershipReboot.AuthenticationService";
        
        public const string AuthenticationType = "MembershipReboot";
        public const string AuthenticationTwoFactorType = AuthenticationType + ".2fa";
    }
}
