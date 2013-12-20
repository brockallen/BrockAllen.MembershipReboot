/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot
{
    public class Group
    {
        public virtual Guid ID { get; internal set; }

        [StringLength(50)]
        [Required]
        public virtual string Tenant { get; internal set; }
        
        [StringLength(100)]
        [Required]
        public virtual string Name { get; internal set; }

        public virtual DateTime Created { get; internal set; }
        public virtual DateTime LastUpdated { get; internal set; }

        public virtual ICollection<GroupChild> Children { get; internal set; }
    }
}
