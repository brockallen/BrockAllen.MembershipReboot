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
        internal protected UserCertificate()
        {
        }

        [Key]
        [Column(Order = 1)]
        public virtual Guid UserAccountID { get; internal set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(150)]
        public virtual string Thumbprint { get; internal set; }

        [StringLength(250)]
        public virtual string Subject { get; internal set; }
    }
}
