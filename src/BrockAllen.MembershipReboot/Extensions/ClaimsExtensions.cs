/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace BrockAllen.MembershipReboot
{
    public static class ClaimsExtensions
    {
        public static string GetValue(this IEnumerable<Claim> claims, string type)
        {
            if (claims != null)
            {
                var claim = claims.SingleOrDefault(x => x.Type == type);
                if (claim != null) return claim.Value;
            }

            return null;
        }
        
        public static IEnumerable<string> GetValues(this IEnumerable<Claim> claims, string claimType)
        {
            if (claims == null) return Enumerable.Empty<string>();

            var query =
                from claim in claims
                where claim.Type == claimType
                select claim.Value;
            return query.ToArray();
        }
    }
}
