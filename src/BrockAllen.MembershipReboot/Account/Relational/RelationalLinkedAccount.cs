/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot.Relational
{
    public class RelationalLinkedAccount<TKey> : LinkedAccount
    {
        public virtual TKey Key { get; set; }
        public virtual TKey ParentKey { get; set; }
    }
    public class RelationalLinkedAccount : RelationalLinkedAccount<int> { }
}
