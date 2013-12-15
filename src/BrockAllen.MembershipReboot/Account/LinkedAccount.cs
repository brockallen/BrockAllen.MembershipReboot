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
        [StringLength(30)]
        public virtual string ProviderName { get; protected internal set; }
        [StringLength(100)]
        public virtual string ProviderAccountID { get; protected internal set; }

        public virtual DateTime LastLogin { get; protected internal set; }

        public virtual ICollection<LinkedAccountClaim> Claims { get; protected internal set; }
    }
}

