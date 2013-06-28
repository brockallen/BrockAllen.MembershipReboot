/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.Security.Claims;

namespace BrockAllen.MembershipReboot
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool HasClaim(this ClaimsPrincipal user, string type)
        {
            if (user != null)
            {
                return user.HasClaim(x => x.Type == type);
            }
            return false;
        }
    }
}
