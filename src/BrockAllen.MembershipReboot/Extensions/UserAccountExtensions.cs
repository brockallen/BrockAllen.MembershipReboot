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
    public static class UserAccountExtensions
    {
        public static bool HasClaim(this UserAccount account, string type)
        {
            if (account == null) throw new ArgumentException("account");
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            return account.Claims.Any(x => x.Type == type);
        }

        public static bool HasClaim(this UserAccount account, string type, string value)
        {
            if (account == null) throw new ArgumentException("account");
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");
            if (String.IsNullOrWhiteSpace(value)) throw new ArgumentException("value");

            return account.Claims.Any(x => x.Type == type && x.Value == value);
        }

        public static IEnumerable<string> GetClaimValues(this UserAccount account, string type)
        {
            if (account == null) throw new ArgumentException("account");
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            var query =
                from claim in account.Claims
                where claim.Type == type
                select claim.Value;
            return query.ToArray();
        }

        public static string GetClaimValue(this UserAccount account, string type)
        {
            if (account == null) throw new ArgumentException("account");
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            var query =
                from claim in account.Claims
                where claim.Type == type
                select claim.Value;
            return query.SingleOrDefault();
        }

        public static bool RequiresTwoFactorAuthToSignIn(this UserAccount account)
        {
            if (account == null) throw new ArgumentException("account");
            return account.CurrentTwoFactorAuthStatus != TwoFactorAuthMode.None;
        }

        public static bool RequiresTwoFactorCertificateToSignIn(this UserAccount account)
        {
            if (account == null) throw new ArgumentException("account");
            return
                account.AccountTwoFactorAuthMode == TwoFactorAuthMode.Certificate &&
                account.CurrentTwoFactorAuthStatus == TwoFactorAuthMode.Certificate;
        }

        public static bool RequiresTwoFactorAuthCodeToSignIn(this UserAccount account)
        {
            if (account == null) throw new ArgumentException("account");
            return
                account.AccountTwoFactorAuthMode == TwoFactorAuthMode.Mobile &&
                account.CurrentTwoFactorAuthStatus == TwoFactorAuthMode.Mobile;
        }

        public static bool HasPassword(this UserAccount account)
        {
            if (account == null) throw new ArgumentException("account");
            return !String.IsNullOrWhiteSpace(account.HashedPassword);
        }
        
        public static bool IsNew(this UserAccount account)
        {
            if (account == null) throw new ArgumentException("account");
            return account.LastLogin == null;
        }
    }
}
