/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    public abstract class QueryableGroupRepository<TGroup> : 
        IGroupRepository<TGroup>,
        IGroupQuery
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
            if (String.IsNullOrWhiteSpace(tenant) || 
                String.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return Queryable.SingleOrDefault(x =>
                tenant.Equals(x.Tenant, StringComparison.OrdinalIgnoreCase) && 
                name.Equals(x.Name, StringComparison.OrdinalIgnoreCase));
        }

        public System.Collections.Generic.IEnumerable<TGroup> GetByIDs(Guid[] ids)
        {
            return Queryable.Where(x => ids.Contains(x.ID)).ToArray();
        }

        public abstract System.Collections.Generic.IEnumerable<TGroup> GetByChildID(Guid childGroupID);

        protected virtual IQueryable<TGroup> SortedQueryable
        {
            get
            {
                return Queryable.OrderBy(x => x.Tenant).ThenBy(x => x.Name);
            }
        }

        // IGroupQuery
        public System.Collections.Generic.IEnumerable<string> GetAllTenants()
        {
            return Queryable.Select(x => x.Tenant).Distinct().ToArray();
        }

        public System.Collections.Generic.IEnumerable<GroupQueryResult> Query(string filter)
        {
            var query =
                from a in SortedQueryable
                select a;

            if (!String.IsNullOrWhiteSpace(filter))
            {
                query =
                    from a in query
                    where
                        a.Tenant.Contains(filter) ||
                        a.Name.Contains(filter)
                    select a;
            }

            var result =
                from a in query
                select new GroupQueryResult
                {
                    ID = a.ID,
                    Tenant = a.Tenant,
                    Name = a.Name
                };

            return result.ToArray();
        }

        public System.Collections.Generic.IEnumerable<GroupQueryResult> Query(string tenant, string filter)
        {
            var query =
                from a in SortedQueryable
                where a.Tenant == tenant
                select a;

            if (!String.IsNullOrWhiteSpace(filter))
            {
                query =
                    from a in query
                    where
                        a.Name.Contains(filter)
                    select a;
            }

            var result =
                from a in query
                select new GroupQueryResult
                {
                    ID = a.ID,
                    Tenant = a.Tenant,
                    Name = a.Name
                };

            return result.ToArray();
        }

        public System.Collections.Generic.IEnumerable<GroupQueryResult> Query(string filter, int skip, int count, out int totalCount)
        {
            var query =
                from a in SortedQueryable
                select a;

            if (!String.IsNullOrWhiteSpace(filter))
            {
                query =
                    from a in query
                    where
                        a.Tenant.Contains(filter) ||
                        a.Name.Contains(filter)
                    select a;
            }

            var result =
                from a in query
                select new GroupQueryResult
                {
                    ID = a.ID,
                    Tenant = a.Tenant,
                    Name = a.Name
                };
            
            totalCount = query.Count();
            return result.Skip(skip).Take(count).ToArray();
        }

        public System.Collections.Generic.IEnumerable<GroupQueryResult> Query(string tenant, string filter, int skip, int count, out int totalCount)
        {
            var query =
                from a in SortedQueryable
                where a.Tenant == tenant
                select a;

            if (!String.IsNullOrWhiteSpace(filter))
            {
                query =
                    from a in query
                    where
                        a.Name.Contains(filter)
                    select a;
            }

            var result =
                from a in query
                select new GroupQueryResult
                {
                    ID = a.ID,
                    Tenant = a.Tenant,
                    Name = a.Name
                };

            totalCount = query.Count();
            return result.Skip(skip).Take(count).ToArray();
        }

        public System.Collections.Generic.IEnumerable<string> GetRoleNames(string tenant)
        {
            return Queryable.Where(x => x.Tenant == tenant).Select(x => x.Name).ToArray();
        }
    }
}
