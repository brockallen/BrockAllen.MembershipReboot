/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot
{
    public class LinkedAccountClaim
    {
        [StringLength(30)]
        public virtual string ProviderName { get; protected internal set; }
        [StringLength(100)]
        public virtual string ProviderAccountID { get; protected internal set; }
        [StringLength(150)]
        public virtual string Type { get; protected internal set; }
        [StringLength(150)]
        public virtual string Value { get; protected internal set; }
    }
}
