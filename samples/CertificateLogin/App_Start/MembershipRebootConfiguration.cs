using BrockAllen.MembershipReboot;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;

namespace CertificateLogin
{
    public class MembershipRebootConfig
    {
        public static MembershipRebootConfiguration Create()
        {
            var settings = SecuritySettings.FromConfiguration();
            settings.RequireAccountVerification = false;
            var config = new MembershipRebootConfiguration(settings);
            return config;
        }
    }
}