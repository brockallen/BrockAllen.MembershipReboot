/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


namespace BrockAllen.MembershipReboot.Owin
{
    public class MembershipRebootOwinConstants
    {
        internal const string OwinAuthenticationService = "MembershipReboot.AuthenticationService";
        
        public const string AuthenticationType = "MembershipReboot";
        public const string AuthenticationTwoFactorType = AuthenticationType + ".2fa";
    }
}
