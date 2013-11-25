/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public static class IUserAccountExtensions
    {
        public static bool HasClaim(this IUserAccount account, string type)
        {
            if (account == null) throw new ArgumentException("account");
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            return account.Claims.Any(x => x.Type == type);
        }

        public static bool HasClaim(this IUserAccount account, string type, string value)
        {
            if (account == null) throw new ArgumentException("account");
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");
            if (String.IsNullOrWhiteSpace(value)) throw new ArgumentException("value");

            return account.Claims.Any(x => x.Type == type && x.Value == value);
        }

        public static IEnumerable<string> GetClaimValues(this IUserAccount account, string type)
        {
            if (account == null) throw new ArgumentException("account");
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            var query =
                from claim in account.Claims
                where claim.Type == type
                select claim.Value;
            return query.ToArray();
        }

        public static string GetClaimValue(this IUserAccount account, string type)
        {
            if (account == null) throw new ArgumentException("account");
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            var query =
                from claim in account.Claims
                where claim.Type == type
                select claim.Value;
            return query.SingleOrDefault();
        }

        public static bool RequiresTwoFactorAuthToSignIn(this IUserAccount account)
        {
            if (account == null) throw new ArgumentException("account");
            return account.CurrentTwoFactorAuthStatus != TwoFactorAuthMode.None;
        }

        public static bool RequiresTwoFactorCertificateToSignIn(this IUserAccount account)
        {
            if (account == null) throw new ArgumentException("account");
            return
                account.AccountTwoFactorAuthMode == TwoFactorAuthMode.Certificate &&
                account.CurrentTwoFactorAuthStatus == TwoFactorAuthMode.Certificate;
        }

        public static bool RequiresTwoFactorAuthCodeToSignIn(this IUserAccount account)
        {
            if (account == null) throw new ArgumentException("account");
            return
                account.AccountTwoFactorAuthMode == TwoFactorAuthMode.Mobile &&
                account.CurrentTwoFactorAuthStatus == TwoFactorAuthMode.Mobile;
        }
    }
}
