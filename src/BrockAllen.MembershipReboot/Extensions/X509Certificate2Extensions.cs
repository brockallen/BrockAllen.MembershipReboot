/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Security.Cryptography.X509Certificates;

namespace BrockAllen.MembershipReboot
{
    static class X509Certificate2Extensions
    {
        public static bool Validate(this X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                Tracing.Verbose("[X509Certificate2Extensions.Validate] failed -- null cert");
                return false;
            }
            
            if (certificate.Handle == IntPtr.Zero)
            {
                Tracing.Verbose("[X509Certificate2Extensions.Validate] failed -- invalid cert handle");
                return false;
            }

            return true;
        }
    }
}
