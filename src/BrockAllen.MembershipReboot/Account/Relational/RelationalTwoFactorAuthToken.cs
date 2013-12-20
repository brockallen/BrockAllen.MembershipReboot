/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot.Relational
{
    public class RelationalTwoFactorAuthToken : TwoFactorAuthToken
    {
        public Guid UserAccountID { get; set; }
    }
    public class RelationalTwoFactorAuthTokenInt : TwoFactorAuthToken
    {
        public int UserAccountID { get; set; }
    }
}
