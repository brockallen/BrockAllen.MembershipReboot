/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Security.Claims;
using System.Security.Principal;

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

        public static Guid GetUserID(this IPrincipal p)
        {
            var cp = p as ClaimsPrincipal;
            if (cp != null)
            {
                var id = cp.Claims.GetValue(ClaimTypes.NameIdentifier);
                Guid g;
                if (Guid.TryParse(id, out g))
                {
                    return g;
                }
            }
            throw new Exception("Invalid NameIdentifier");
        }

        public static PartialAuthReason GetPartialAuthReason(this IPrincipal p)
        {
            var cp = p as ClaimsPrincipal;
            if (cp != null)
            {
                var rawValue = cp.Claims.GetValue(MembershipRebootConstants.ClaimTypes.PartialAuthReason);
                PartialAuthReason value;
                if (Enum.TryParse(rawValue, out value))
                {
                    return value;
                }
            }
            return PartialAuthReason.None;
        }

        public static bool HasUserID(this IPrincipal p)
        {
            var cp = p as ClaimsPrincipal;
            if (cp != null)
            {
                var id = cp.Claims.GetValue(ClaimTypes.NameIdentifier);
                Guid g;
                if (Guid.TryParse(id, out g))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
