/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;

namespace BrockAllen.MembershipReboot
{
    public class LinkedAccountClaim
    {
        internal protected LinkedAccountClaim()
        {
        }

        [Key]
        [Column(Order = 1)]
        public virtual Guid UserAccountID { get; internal set; }
        [Key]
        [Column(Order = 2)]
        [StringLength(30)]
        public virtual string ProviderName { get; internal set; }
        [Key]
        [Column(Order = 3)]
        [StringLength(100)]
        public virtual string ProviderAccountID { get; internal set; }
        [Key]
        [Column(Order = 4)]
        [StringLength(150)]
        public virtual string Type { get; internal set; }
        [Key]
        [Column(Order = 5)]
        [StringLength(150)]
        public virtual string Value { get; internal set; }
    }
}
