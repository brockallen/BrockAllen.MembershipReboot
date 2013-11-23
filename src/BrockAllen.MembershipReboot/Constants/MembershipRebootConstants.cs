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
            internal const int VerificationKeyStaleDurationMinutes = 20;
            internal const int MobileCodeLength = 6;
            internal const int MobileCodeStaleDurationMinutes = 10;
            internal const int TwoFactorAuthTokenDurationDays = 30;
        }

        public class AuthenticationService
        {
            internal static readonly TimeSpan TwoFactorAuthTokenLifetime = TimeSpan.FromMinutes(10);
            internal const int DefaultPersistentCookieDays = UserAccount.TwoFactorAuthTokenDurationDays;
            internal const string CookieBasedTwoFactorAuthPolicyCookieName = "mr.cbtfap.";
        }
    }
}
