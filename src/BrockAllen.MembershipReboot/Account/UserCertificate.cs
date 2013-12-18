/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrockAllen.MembershipReboot
{
    public class UserCertificate
    {
        public virtual Guid UserAccountID { get; internal set; }

        [StringLength(150)]
        public virtual string Thumbprint { get; protected internal set; }

        [StringLength(250)]
        public virtual string Subject { get; protected internal set; }
    }
}
