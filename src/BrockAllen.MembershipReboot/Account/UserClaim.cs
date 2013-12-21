/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot
{
    public class UserClaim
    {
        [StringLength(150)]
        public virtual string Type { get; protected internal set; }
        [StringLength(150)]
        public virtual string Value { get; protected internal set; }
    }
}
