/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot.Relational
{
    public class RelationalLinkedAccountClaim<TKey> : LinkedAccountClaim
    {
        public virtual TKey Key { get; set; }
        public virtual TKey ParentKey { get; set; }
    }
    public class RelationalLinkedAccountClaim : RelationalLinkedAccountClaim<int> { }
}
