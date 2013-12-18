/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrockAllen.MembershipReboot
{
    public class UserClaim
    {
        public virtual Guid UserAccountID { get; internal set; }
        [StringLength(150)]
        public virtual string Type { get; protected internal set; }
        [StringLength(150)]
        public virtual string Value { get; protected internal set; }
    }
}
