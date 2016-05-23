/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot.Test.GroupService
{
    public class TestGroupService : GroupService<Group>
    {
        public DateTime? Now { get; set; }

        public TestGroupService(IGroupRepository groupRepository)
            : base(groupRepository)
        {
        }

        public TestGroupService(string defaultTenant, IGroupRepository groupRepository)
            : base(defaultTenant, groupRepository)
        {
        }

        protected override DateTime UtcNow
        {
            get
            {
                if (Now != null) return Now.Value;
                else return base.UtcNow;
            }
        }
    }
}
