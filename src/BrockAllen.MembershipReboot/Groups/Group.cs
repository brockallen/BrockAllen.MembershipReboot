/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot
{
    public abstract class Group
    {
        public virtual Guid ID { get; protected internal set; }

        [StringLength(50)]
        [Required]
        public virtual string Tenant { get; protected internal set; }
        
        [StringLength(100)]
        [Required]
        public virtual string Name { get; protected internal set; }

        public virtual DateTime Created { get; protected internal set; }
        public virtual DateTime LastUpdated { get; protected internal set; }

        public abstract IEnumerable<GroupChild> Children { get; }
        protected internal abstract void AddChild(GroupChild child);
        protected internal abstract void RemoveChild(GroupChild child);
    }
}
