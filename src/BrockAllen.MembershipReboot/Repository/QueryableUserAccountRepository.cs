/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    public abstract class QueryableUserAccountRepository<TAccount> : 
        IUserAccountRepository<TAccount>,
        IUserAccountQuery,
        IUserAccountQuery<TAccount>
        where TAccount : UserAccount
    {
        public bool UseEqualsOrdinalIgnoreCaseForQueries { get; set; }

        public Func<IQueryable<TAccount>, string, IQueryable<TAccount>> QueryFilter { get; set; }
        public Func<IQueryable<TAccount>, IQueryable<TAccount>> QuerySort { get; set; }

        public QueryableUserAccountRepository()
        {
            QueryFilter = DefaultQueryFilter;
            QuerySort = DefaultQuerySort;
        }

        protected abstract IQueryable<TAccount> Queryable { get; }
        public abstract TAccount Create();
        public abstract void Add(TAccount item);
        public abstract void Remove(TAccount item);
        public abstract void Update(TAccount item);

        public TAccount GetByID(Guid id)
        {
            return Queryable.SingleOrDefault(x => x.ID == id);
        }

        public TAccount GetByUsername(string username)
        {
            if (String.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            if (UseEqualsOrdinalIgnoreCaseForQueries)
            {
                return Queryable.SingleOrDefault(x =>
                    username.Equals(x.Username, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                return Queryable.SingleOrDefault(x => username == x.Username);
            }
        }

        public TAccount GetByUsername(string tenant, string username)
        {
            if (String.IsNullOrWhiteSpace(tenant) || 
                String.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            if (UseEqualsOrdinalIgnoreCaseForQueries)
            {
                return Queryable.SingleOrDefault(x =>
                    tenant.Equals(x.Tenant, StringComparison.OrdinalIgnoreCase) &&
                    username.Equals(x.Username, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                return Queryable.SingleOrDefault(x => 
                    tenant == x.Tenant &&
                    username == x.Username);
            }
        }

        public TAccount GetByEmail(string tenant, string email)
        {
            if (String.IsNullOrWhiteSpace(tenant) ||
                String.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            if (UseEqualsOrdinalIgnoreCaseForQueries)
            {
                return Queryable.SingleOrDefault(x =>
                    tenant.Equals(x.Tenant, StringComparison.OrdinalIgnoreCase) &&
                    email.Equals(x.Email, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                return Queryable.SingleOrDefault(x =>
                    tenant == x.Tenant &&
                    email == x.Email);
            }
        }

        public TAccount GetByMobilePhone(string tenant, string phone)
        {
            if (String.IsNullOrWhiteSpace(tenant) ||
                String.IsNullOrWhiteSpace(phone))
            {
                return null;
            }

            if (UseEqualsOrdinalIgnoreCaseForQueries)
            {
                return Queryable.SingleOrDefault(x =>
                    tenant.Equals(x.Tenant, StringComparison.OrdinalIgnoreCase) &&
                    phone.Equals(x.MobilePhoneNumber, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                return Queryable.SingleOrDefault(x =>
                    tenant == x.Tenant &&
                    phone == x.MobilePhoneNumber);
            }
        }

        public TAccount GetByVerificationKey(string key)
        {
            return Queryable.SingleOrDefault(x => x.VerificationKey == key);
        }

        public abstract TAccount GetByLinkedAccount(string tenant, string provider, string id);
        public abstract TAccount GetByCertificate(string tenant, string thumbprint);

        protected virtual IQueryable<TAccount> DefaultQuerySort(IQueryable<TAccount> query)
        {
            return query.OrderBy(x => x.Tenant).ThenBy(x => x.Username);
        }
        
        protected virtual IQueryable<TAccount> DefaultQueryFilter(IQueryable<TAccount> query, string filter)
        {
            if (query == null) throw new ArgumentNullException("query");
            if (filter == null) throw new ArgumentNullException("filter");

            return
                from a in query
                where
                    a.Username.Contains(filter) ||
                    a.Email.Contains(filter)
                select a;
        }


        // IUserAccountQuery
        public System.Collections.Generic.IEnumerable<string> GetAllTenants()
        {
            return Queryable.Select(x => x.Tenant).Distinct().ToArray();
        }

        public System.Collections.Generic.IEnumerable<UserAccountQueryResult> Query(string filter)
        {
            var query =
                from a in Queryable
                select a;

            if (!String.IsNullOrWhiteSpace(filter) && QueryFilter != null)
            {
                query = QueryFilter(query, filter);
            }

            if (QuerySort != null)
            {
                query = QuerySort(query);
            }
            
            var result =
                from a in query
                select new UserAccountQueryResult
                {
                    ID = a.ID,
                    Tenant = a.Tenant,
                    Username = a.Username,
                    Email = a.Email
                };

            return result.ToArray();
        }

        public System.Collections.Generic.IEnumerable<UserAccountQueryResult> Query(string tenant, string filter)
        {
            var query =
                from a in Queryable
                where a.Tenant == tenant
                select a;

            if (!String.IsNullOrWhiteSpace(filter) && QueryFilter != null)
            {
                query = QueryFilter(query, filter);
            }

            if (QuerySort != null)
            {
                query = QuerySort(query);
            }

            var result =
                from a in query
                select new UserAccountQueryResult
                {
                    ID = a.ID,
                    Tenant = a.Tenant,
                    Username = a.Username,
                    Email = a.Email
                };

            return result.ToArray();
        }

        public System.Collections.Generic.IEnumerable<UserAccountQueryResult> Query(string filter, int skip, int count, out int totalCount)
        {
            var query =
                from a in Queryable
                select a;

            if (!String.IsNullOrWhiteSpace(filter) && QueryFilter != null)
            {
                query = QueryFilter(query, filter);
            }

            if (QuerySort != null)
            {
                query = QuerySort(query);
            }

            var result =
                from a in query
                select new UserAccountQueryResult
                {
                    ID = a.ID,
                    Tenant = a.Tenant,
                    Username = a.Username,
                    Email = a.Email
                };

            totalCount = result.Count();
            return result.Skip(skip).Take(count).ToArray();
        }

        public System.Collections.Generic.IEnumerable<UserAccountQueryResult> Query(string tenant, string filter, int skip, int count, out int totalCount)
        {
            var query =
                from a in Queryable
                where a.Tenant == tenant
                select a;

            if (!String.IsNullOrWhiteSpace(filter) && QueryFilter != null)
            {
                query = QueryFilter(query, filter);
            }

            if (QuerySort != null)
            {
                query = QuerySort(query);
            }

            var result =
                from a in query
                select new UserAccountQueryResult
                {
                    ID = a.ID,
                    Tenant = a.Tenant,
                    Username = a.Username,
                    Email = a.Email
                };

            totalCount = result.Count();
            return result.Skip(skip).Take(count).ToArray();
        }

        public System.Collections.Generic.IEnumerable<UserAccountQueryResult> Query(Func<IQueryable<TAccount>, IQueryable<TAccount>> filter)
        {
            var query =
                from a in Queryable
                select a;

            if (filter != null) query = filter(query);

            var result =
                from a in query
                select new UserAccountQueryResult
                {
                    ID = a.ID,
                    Tenant = a.Tenant,
                    Username = a.Username,
                    Email = a.Email
                };

            return result.ToArray();
        }

        public System.Collections.Generic.IEnumerable<UserAccountQueryResult> Query(
            Func<IQueryable<TAccount>, IQueryable<TAccount>> filter, 
            Func<IQueryable<TAccount>, IQueryable<TAccount>> sort,
            int skip, int count, out int totalCount)
        {
            var query =
                from a in Queryable
                select a;

            if (filter != null) query = filter(query);
            var sorted = (sort ?? QuerySort)(query);

            var result =
                from a in sorted
                select new UserAccountQueryResult
                {
                    ID = a.ID,
                    Tenant = a.Tenant,
                    Username = a.Username,
                    Email = a.Email
                };

            totalCount = result.Count();
            return result.Skip(skip).Take(count).ToArray();
        }
    }
}
