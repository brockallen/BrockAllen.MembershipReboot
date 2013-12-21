/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot.Relational
{
    public class RelationalUserClaim : UserClaim
    {
        public Guid UserAccountID { get; set; }
    }
    public class RelationalUserClaimInt : UserClaim
    {
        public int UserAccountID { get; set; }
    }
}
