/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot
{
    public class PasswordResetSecret
    {
        public virtual Guid PasswordResetSecretID { get; protected internal set; }

        [StringLength(150)]
        [Required]
        public virtual string Question { get; protected internal set; }

        [StringLength(150)]
        [Required]
        public virtual string Answer { get; protected internal set; }
    }
}
