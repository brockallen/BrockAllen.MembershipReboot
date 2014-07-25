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
            internal const string VerificationKeyLifetime = "00:20:00";
        }

        public class UserAccount
        {
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

        public static class ValidationMessages
        {
            public const string AccountAlreadyVerified = "AccountAlreadyVerified";
            public const string AccountCreateFailNoEmailFromIdp = "AccountCreateFailNoEmailFromIdp";
            public const string AccountNotConfiguredWithSecretQuestion = "AccountNotConfiguredWithSecretQuestion";
            public const string AccountNotVerified = "AccountNotVerified";
            public const string AccountPasswordResetRequiresSecretQuestion = "AccountPasswordResetRequiresSecretQuestion";
            public const string AddClientCertForTwoFactor = "AddClientCertForTwoFactor";
            public const string CantRemoveLastLinkedAccountIfNoPassword = "CantRemoveLastLinkedAccountIfNoPassword";
            public const string CertificateAlreadyInUse = "CertificateAlreadyInUse";
            public const string CodeRequired = "CodeRequired";
            public const string EmailAlreadyInUse = "EmailAlreadyInUse";
            public const string EmailRequired = "EmailRequired";
            public const string InvalidCertificate = "InvalidCertificate";
            public const string InvalidEmail = "InvalidEmail";
            public const string InvalidKey = "InvalidKey";
            public const string InvalidName = "InvalidName";
            public const string InvalidNewPassword = "InvalidNewPassword";
            public const string InvalidOldPassword = "InvalidOldPassword";
            public const string InvalidPassword = "InvalidPassword";
            public const string InvalidPhone = "InvalidPhone";
            public const string InvalidQuestionOrAnswer = "InvalidQuestionOrAnswer";
            public const string InvalidTenant = "InvalidTenant";
            public const string InvalidUsername = "InvalidUsername";
            public const string LoginFailEmailAlreadyAssociated = "LoginFailEmailAlreadyAssociated";
            public const string LoginNotAllowed = "LoginNotAllowed";
            public const string MobilePhoneAlreadyInUse = "MobilePhoneAlreadyInUse";
            public const string MobilePhoneMustBeDifferent = "MobilePhoneMustBeDifferent";
            public const string MobilePhoneRequired = "MobilePhoneRequired";
            public const string NameAlreadyInUse = "NameAlreadyInUse";
            public const string NameRequired = "NameRequired";
            public const string NewPasswordMustBeDifferent = "NewPasswordMustBeDifferent";
            public const string ParentGroupAlreadyChild = "ParentGroupAlreadyChild";
            public const string ParentGroupSameAsChild = "ParentGroupSameAsChild";
            public const string PasswordComplexityRules = "PasswordComplexityRules";
            public const string PasswordLength = "PasswordLength";
            public const string PasswordRequired = "PasswordRequired";
            public const string PasswordResetErrorNoEmail = "PasswordResetErrorNoEmail";
            public const string RegisterMobileForTwoFactor = "RegisterMobileForTwoFactor";
            public const string ReopenErrorNoEmail = "ReopenErrorNoEmail";
            public const string SecretAnswerRequired = "SecretAnswerRequired";
            public const string SecretQuestionAlreadyInUse = "SecretQuestionAlreadyInUse";
            public const string SecretQuestionRequired = "SecretQuestionRequired";
            public const string TenantRequired = "TenantRequired";
            public const string UsernameAlreadyInUse = "UsernameAlreadyInUse";
            public const string UsernameCannotContainAtSign = "UsernameCannotContainAtSign";
            public const string UsernameOnlyContainsValidCharacters = "UsernameOnlyContainsValidCharacters";
            public const string UsernameCannotRepeatSpecialCharacters = "UsernameCannotRepeatSpecialCharacters";
            public const string UsernameRequired = "UsernameRequired";
            public const string UsernameCanOnlyStartOrEndWithLetterOrDigit = "UsernameCanOnlyStartOrEndWithLetterOrDigit";
        }
    }
}
