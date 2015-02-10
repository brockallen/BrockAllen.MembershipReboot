/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace BrockAllen.MembershipReboot.Test.GroupService
{
    public class MyGroup : HierarchicalGroup
    {
    }

    public class FakeGroupRepository : QueryableGroupRepository<Group>, IGroupRepository
    {
        public List<Group> Groups = new List<Group>();

        protected override IQueryable<Group> Queryable
        {
            get { return Groups.AsQueryable(); }
        }

        public override Group Create()
        {
            return new MyGroup();
        }

        public override void Add(Group item)
        {
            Groups.Add(item);
        }

        public override void Remove(Group item)
        {
            Groups.Remove(item);
        }

        public override void Update(Group item)
        {
        }

        public override IEnumerable<Group> GetByChildID(Guid childGroupID)
        {
            var query =
                from g in Groups
                from c in g.Children
                where c.ChildGroupID == childGroupID
                select g;
            return query.ToList();
        }
    }
}