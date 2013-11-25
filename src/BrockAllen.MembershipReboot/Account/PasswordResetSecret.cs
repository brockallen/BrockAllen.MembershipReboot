/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrockAllen.MembershipReboot
{
    public class PasswordResetSecret
    {
        internal protected PasswordResetSecret()
        {
        }

        [Key]
        [Column(Order = 1)]
        public virtual Guid PasswordResetSecretID { get; internal set; }

        [Key]
        [Column(Order = 2)]
        public virtual Guid UserAccountID { get; internal set; }

        [StringLength(150)]
        [Required]
        public virtual string Question { get; internal set; }

        [StringLength(150)]
        [Required]
        public virtual string Answer { get; internal set; }
    }
}
