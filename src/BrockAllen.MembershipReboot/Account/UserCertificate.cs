/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot
{
    public class UserCertificate
    {
        [StringLength(150)]
        public virtual string Thumbprint { get; protected internal set; }

        [StringLength(250)]
        public virtual string Subject { get; protected internal set; }
    }
}
