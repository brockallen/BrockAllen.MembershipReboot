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
    public static class ILinkedAccountExtensions
    {
        public static bool HasClaim(this ILinkedAccount account, string type)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            return account.Claims.Any(x => x.Type == type);
        }

        public static bool HasClaim(this ILinkedAccount account, string type, string value)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");
            if (String.IsNullOrWhiteSpace(value)) throw new ArgumentException("value");

            return account.Claims.Any(x => x.Type == type && x.Value == value);
        }

        public static IEnumerable<string> GetClaimValues(this ILinkedAccount account, string type)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            var query =
                from claim in account.Claims
                where claim.Type == type
                select claim.Value;
            return query.ToArray();
        }

        public static string GetClaimValue(this ILinkedAccount account, string type)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            var query =
                from claim in account.Claims
                where claim.Type == type
                select claim.Value;
            return query.SingleOrDefault();
        }

        public static void AddClaim(this ILinkedAccount account, string type, string value)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");
            if (String.IsNullOrWhiteSpace(value)) throw new ArgumentException("value");

            if (!account.HasClaim(type, value))
            {
                var claim = account.CreateClaim();
                claim.Type = type;
                claim.Value = value;
                account.Claims.Add(claim);
            }
        }

        public static void RemoveClaim(this ILinkedAccount account, string type)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            var claimsToRemove =
                from claim in account.Claims
                where claim.Type == type
                select claim;
            foreach (var claim in claimsToRemove.ToArray())
            {
                account.Claims.Remove(claim);
            }
        }

        public static void RemoveClaim(this ILinkedAccount account, string type, string value)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");
            if (String.IsNullOrWhiteSpace(value)) throw new ArgumentException("value");

            var claimsToRemove =
                from claim in account.Claims
                where claim.Type == type && claim.Value == value
                select claim;
            foreach (var claim in claimsToRemove.ToArray())
            {
                account.Claims.Remove(claim);
            }
        }

        public static void UpdateClaims(this ILinkedAccount account, IEnumerable<Claim> claims)
        {
            if (account == null) throw new ArgumentNullException("account");
            claims = claims ?? Enumerable.Empty<Claim>();

            account.Claims.Clear();

            foreach (var c in claims)
            {
                var claim = account.CreateClaim();
                claim.Type = c.Type;
                claim.Value = c.Value;
                account.Claims.Add(claim);
            }
        }
    }

}
