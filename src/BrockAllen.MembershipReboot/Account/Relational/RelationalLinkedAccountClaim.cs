/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot.Relational
{
    public class RelationalLinkedAccountClaim : LinkedAccountClaim
    {
        public Guid UserAccountID { get; set; }
    }
    public class RelationalLinkedAccountClaimInt : LinkedAccountClaim
    {
        public int UserAccountID { get; set; }
    }
}
