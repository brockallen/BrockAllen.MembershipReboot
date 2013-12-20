/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot.Relational
{
    public class RelationalPasswordResetSecret : PasswordResetSecret
    {
        public Guid UserAccountID { get; set; }
    }
    public class RelationalPasswordResetSecretInt : PasswordResetSecret
    {
        public int UserAccountID { get; set; }
    }
}
