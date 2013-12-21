/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot.Relational
{
    public class RelationalLinkedAccount : LinkedAccount
    {
        public Guid UserAccountID { get; set; }
    }
    public class RelationalLinkedAccountInt : LinkedAccount
    {
        public int UserAccountID { get; set; }
    }
}
