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
        public Group()
        {
        }

        public Group(string name)
            : this(SecuritySettings.Instance.DefaultTenant, name)
        {
        }
        
        public Group(string tenant, string name)
        {
            this.ID = Guid.NewGuid();
            this.Tenant = tenant;
            this.Name = name;
            this.Children = new HashSet<GroupChild>();
        }

        [Key]
        public virtual Guid ID { get; internal set; }

        [StringLength(50)]
        [Required]
        public virtual string Tenant { get; internal set; }
        
        [StringLength(100)]
        [Required]
        public virtual string Name { get; set; }

        public ICollection<GroupChild> Children { get; set; }
    }
}
