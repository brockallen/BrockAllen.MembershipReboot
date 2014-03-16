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
        IUserAccountQuery
        where TAccount : UserAccount
    {
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

            return Queryable.SingleOrDefault(x => 
                username.Equals(x.Username, StringComparison.OrdinalIgnoreCase));
        }

        public TAccount GetByUsername(string tenant, string username)
        {
            if (String.IsNullOrWhiteSpace(tenant) || 
                String.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            return Queryable.SingleOrDefault(x => 
                tenant.Equals(x.Tenant, StringComparison.OrdinalIgnoreCase) && 
                username.Equals(x.Username, StringComparison.OrdinalIgnoreCase));
        }

        public TAccount GetByEmail(string tenant, string email)
        {
            if (String.IsNullOrWhiteSpace(tenant) ||
                String.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            return Queryable.SingleOrDefault(x => 
                tenant.Equals(x.Tenant, StringComparison.OrdinalIgnoreCase) &&
                email.Equals(x.Email, StringComparison.OrdinalIgnoreCase));
        }

        public TAccount GetByMobilePhone(string tenant, string phone)
        {
            if (String.IsNullOrWhiteSpace(tenant) ||
                String.IsNullOrWhiteSpace(phone))
            {
                return null;
            }
            
            return Queryable.SingleOrDefault(x => 
                tenant.Equals(x.Tenant, StringComparison.OrdinalIgnoreCase) &&
                phone.Equals(x.MobilePhoneNumber, StringComparison.OrdinalIgnoreCase));
        }

        public TAccount GetByVerificationKey(string key)
        {
            return Queryable.SingleOrDefault(x => x.VerificationKey == key);
        }

        public abstract TAccount GetByLinkedAccount(string tenant, string provider, string id);
        public abstract TAccount GetByCertificate(string tenant, string thumbprint);

        protected virtual IQueryable<TAccount> SortedQueryable
        {
            get
            {
                return Queryable.OrderBy(x => x.Tenant).ThenBy(x => x.Username);
            }
        }

        // IUserAccountQuery
        public System.Collections.Generic.IEnumerable<string> GetAllTenants()
        {
            return Queryable.Select(x => x.Tenant).Distinct();
        }

        public System.Collections.Generic.IEnumerable<UserAccountQueryResult> Query(string filter)
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
                        a.Username.Contains(filter) ||
                        a.Email.Contains(filter)
                    select a;
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

            return result;
        }

        public System.Collections.Generic.IEnumerable<UserAccountQueryResult> Query(string tenant, string filter)
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
                        a.Username.Contains(filter) ||
                        a.Email.Contains(filter)
                    select a;
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

            return result;
        }

        public System.Collections.Generic.IEnumerable<UserAccountQueryResult> Query(string filter, int skip, int count, out int totalCount)
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
                        a.Username.Contains(filter) ||
                        a.Email.Contains(filter)
                    select a;
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
            return result.Skip(skip).Take(count);
        }

        public System.Collections.Generic.IEnumerable<UserAccountQueryResult> Query(string tenant, string filter, int skip, int count, out int totalCount)
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
                        a.Username.Contains(filter) ||
                        a.Email.Contains(filter)
                    select a;
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
            return result.Skip(skip).Take(count);
        }
    }
}
