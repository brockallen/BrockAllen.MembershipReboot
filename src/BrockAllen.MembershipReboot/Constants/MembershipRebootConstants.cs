/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
namespace BrockAllen.MembershipReboot
{
    public class MembershipRebootConstants
    {
        public class ClaimTypes
        {
            public const string Tenant = "http://brockallen.com/membershipreboot/claims/tenant";
        }

        public class UserAccount
        {
            internal const int VerificationKeyStaleDurationDays = 1;
            internal const int MobileCodeLength = 6;
            internal const int MobileCodeStaleDurationMinutes = 10;
        }

        public class AuthenticationService
        {
            internal static readonly TimeSpan TwoFactorAuthTokenLifetime = TimeSpan.FromMinutes(30);
            internal const int DefaultPersistentCookieDays = 30;
            internal const string CookieBasedTwoFactorAuthPolicyCookieName = "mr.cbtfap";
        }
    }
}
