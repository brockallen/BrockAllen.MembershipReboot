/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    public abstract class QueryableGroupRepository<TGroup> : IGroupRepository<TGroup>
        where TGroup : Group
    {
        protected abstract IQueryable<TGroup> Queryable { get; }

        public abstract TGroup Create();
        public abstract void Add(TGroup item);
        public abstract void Remove(TGroup item);
        public abstract void Update(TGroup item);

        public TGroup GetByID(Guid id)
        {
            return Queryable.SingleOrDefault(x => x.ID == id);
        }

        public TGroup GetByName(string tenant, string name)
        {
            return Queryable.SingleOrDefault(x => x.Tenant == tenant && x.Name == name);
        }

        public System.Collections.Generic.IEnumerable<TGroup> GetAll()
        {
            return Queryable;
        }

        public System.Collections.Generic.IEnumerable<TGroup> GetByIDs(Guid[] ids)
        {
            return Queryable.Where(x => ids.Contains(x.ID));
        }

        public abstract System.Collections.Generic.IEnumerable<TGroup> GetByChildID(Guid childGroupID);
    }
}
