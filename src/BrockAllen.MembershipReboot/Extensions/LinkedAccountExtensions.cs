/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public static class LinkedAccountExtensions
    {
        public static bool HasClaim(this LinkedAccount account, string type)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            return account.Claims.Any(x => x.Type == type);
        }

        public static bool HasClaim(this LinkedAccount account, string type, string value)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");
            if (String.IsNullOrWhiteSpace(value)) throw new ArgumentException("value");

            return account.Claims.Any(x => x.Type == type && x.Value == value);
        }

        public static IEnumerable<string> GetClaimValues(this LinkedAccount account, string type)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            var query =
                from claim in account.Claims
                where claim.Type == type
                select claim.Value;
            return query.ToArray();
        }

        public static string GetClaimValue(this LinkedAccount account, string type)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            var query =
                from claim in account.Claims
                where claim.Type == type
                select claim.Value;
            return query.SingleOrDefault();
        }
    }

}
