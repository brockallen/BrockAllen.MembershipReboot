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
        public virtual Guid UserAccountID { get; internal set; }
        public virtual string ProviderName { get; internal set; }
        public virtual string ProviderAccountID { get; internal set; }
        
        [StringLength(150)]
        public virtual string Type { get; protected internal set; }
        [StringLength(150)]
        public virtual string Value { get; protected internal set; }
    }
}
