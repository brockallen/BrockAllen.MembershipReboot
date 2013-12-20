/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot.Relational
{
    public class RelationalUserCertificate : UserCertificate
    {
        public Guid UserAccountID { get; set; }
    }
    public class RelationalUserCertificateInt : UserCertificate
    {
        public int UserAccountID { get; set; }
    }
}
