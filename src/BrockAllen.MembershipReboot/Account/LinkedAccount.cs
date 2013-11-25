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
    public class LinkedAccount
    {
        internal protected LinkedAccount()
        {
            this.Claims = new HashSet<LinkedAccountClaim>();
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

        public virtual DateTime LastLogin { get; internal set; }

        public virtual ICollection<LinkedAccountClaim> Claims { get; internal set; }
    }
}

