/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot.Relational
{
    public class RelationalUserCertificate<TKey> : UserCertificate
    {
        public virtual TKey Key { get; set; }
        public virtual TKey ParentKey { get; set; }
    }
    public class RelationalUserCertificate : RelationalUserCertificate<int> { }
}
