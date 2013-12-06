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

        public class SecuritySettingDefaults
        {
            internal const bool MultiTenant = false;
            internal const string DefaultTenant = "default";
            internal const bool EmailIsUsername = false;
            internal const bool UsernamesUniqueAcrossTenants = false;
            internal const bool RequireAccountVerification = true;
            internal const bool AllowLoginAfterAccountCreation = true;
            internal const int AccountLockoutFailedLoginAttempts = 5;
            internal const string AccountLockoutDuration = "00:05:00";
            internal const bool AllowAccountDeletion = true;
            internal const int PasswordHashingIterationCount = 0;
            internal const int PasswordResetFrequency = 0;
        }

        public class UserAccount
        {
            internal const int VerificationKeyStaleDurationMinutes = 20;
            internal const int MobileCodeLength = 6;
            internal const int MobileCodeResendDelayMinutes = 1;
            internal const int MobileCodeStaleDurationMinutes = 10;
            internal const int TwoFactorAuthTokenDurationDays = 30;
        }

        public class AuthenticationService
        {
            internal static readonly TimeSpan TwoFactorAuthTokenLifetime = TimeSpan.FromMinutes(10);
            internal const int DefaultPersistentCookieDays = UserAccount.TwoFactorAuthTokenDurationDays;
            internal const string CookieBasedTwoFactorAuthPolicyCookieName = "mr.cbtfap.";
        }

        public class PasswordComplexity
        {
            internal const int MinimumLength = 10;
            internal const int NumberOfComplexityRules = 3;
        }
    }
}
