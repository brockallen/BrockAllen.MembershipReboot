/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot.Relational
{
    public class RelationalUserClaim : UserClaim
    {
        public virtual int Key { get; set; }
        public virtual int ParentKey { get; set; }
    }
}
