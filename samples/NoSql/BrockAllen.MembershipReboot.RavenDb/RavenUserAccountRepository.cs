/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


using System.Collections.Generic;
using System.Linq;
using BrockAllen.MembershipReboot.Hierarchical;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Linq;
using System;

namespace BrockAllen.MembershipReboot.RavenDb
{
    public class RavenUserAccountRepository :
        IUserAccountRepository<HierarchicalUserAccount>,
        IUserAccountQuery
    {
        public bool UseEqualsOrdinalIgnoreCaseForQueries { get; set; }

        public Func<IRavenQueryable<HierarchicalUserAccount>, string, IRavenQueryable<HierarchicalUserAccount>> QueryFilter { get; set; }
        public Func<IRavenQueryable<HierarchicalUserAccount>, IRavenQueryable<HierarchicalUserAccount>> QuerySort { get; set; }

        public RavenUserAccountRepository(string connectionStringName)
            : this((DocumentStore)new RavenMembershipRebootDatabase(connectionStringName).DocumentStore)
        {
        }

        public RavenUserAccountRepository(IDocumentStore documentStore)
        {
            this.documentStore = documentStore;
            documentSession = documentStore.OpenSession();
            items = documentSession.Query<HierarchicalUserAccount>();
        }

        private readonly IDocumentStore documentStore;
        private readonly IDocumentSession documentSession;
        private readonly IRavenQueryable<HierarchicalUserAccount> items;

        protected void CheckDisposed()
        {
            if (documentStore == null || documentSession == null)
            {
                throw new ObjectDisposedException("RavenDbRepository<T>");
            }
        }

        public void Dispose()
        {
            documentSession.TryDispose();
        }

        public HierarchicalUserAccount Create()
        {
            return new HierarchicalUserAccount();
        }

        public void Add(HierarchicalUserAccount item)
        {
            CheckDisposed();
            documentSession.Store(item);
            documentSession.SaveChanges();
        }

        public void Remove(HierarchicalUserAccount item)
        {
            CheckDisposed();
            documentSession.Delete(item);
            documentSession.SaveChanges();
        }

        public void Update(HierarchicalUserAccount item)
        {
            CheckDisposed();
            documentSession.Store(item);
            documentSession.SaveChanges();
        }

        public HierarchicalUserAccount GetByID(Guid id)
        {
            return items.Customize(x => x.WaitForNonStaleResultsAsOfNow())
                                  .Where(x => x.ID == id)
                                  .SingleOrDefault();
        }

        public HierarchicalUserAccount GetByLinkedAccount(string tenant, string provider, string id)
        {
            return items.Where(x => x.Tenant == tenant && x.LinkedAccountCollection.Any(
                                         y => y.ProviderName == provider && y.ProviderAccountID == id))
                                         .SingleOrDefault();
        }

        public HierarchicalUserAccount GetByCertificate(string tenant, string thumbprint)
        {
            throw new NotImplementedException();
        }

        public HierarchicalUserAccount GetByUsername(string username)
        {
            if (String.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            if (UseEqualsOrdinalIgnoreCaseForQueries)
            {
                return items
                    .Customize(x => x.WaitForNonStaleResultsAsOfNow())
                    .Where(x => username.Equals(x.Username, StringComparison.OrdinalIgnoreCase))
                    .SingleOrDefault();
            }
            else
            {
                return items
                       .Customize(x => x.WaitForNonStaleResultsAsOfNow())
                       .Where(x => username == x.Username)
                       .SingleOrDefault();
            }
        }

        public HierarchicalUserAccount GetByUsername(string tenant, string username)
        {
            if (String.IsNullOrWhiteSpace(tenant) ||
                String.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            if (UseEqualsOrdinalIgnoreCaseForQueries)
            {
                return
                    items
                    .Customize(x => x.WaitForNonStaleResultsAsOfNow())
                    .Where(x =>
                        tenant.Equals(x.Tenant, StringComparison.OrdinalIgnoreCase) &&
                        username.Equals(x.Username, StringComparison.OrdinalIgnoreCase))
                    .SingleOrDefault();
            }
            else
            {
                return
                    items
                    .Customize(x => x.WaitForNonStaleResultsAsOfNow())
                    .Where(x =>
                        tenant == x.Tenant &&
                        username == x.Username)
                    .SingleOrDefault();
            }
        }

        public HierarchicalUserAccount GetByEmail(string tenant, string email)
        {
            if (String.IsNullOrWhiteSpace(tenant) ||
                String.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            if (UseEqualsOrdinalIgnoreCaseForQueries)
            {
                return
                    items
                    .Customize(x => x.WaitForNonStaleResultsAsOfNow())
                    .Where(x =>
                        tenant.Equals(x.Tenant, StringComparison.OrdinalIgnoreCase) &&
                        email.Equals(x.Email, StringComparison.OrdinalIgnoreCase))
                    .SingleOrDefault();
            }
            else
            {
                return
                    items
                    .Customize(x => x.WaitForNonStaleResultsAsOfNow())
                    .Where(x =>
                        tenant == x.Tenant &&
                        email == x.Email)
                    .SingleOrDefault();
            }
        }

        public HierarchicalUserAccount GetByMobilePhone(string tenant, string phone)
        {
            throw new NotImplementedException();
        }

        public HierarchicalUserAccount GetByVerificationKey(string key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetAllTenants()
        {
            return items.Select(x => x.Tenant).Distinct();
        }


        public System.Collections.Generic.IEnumerable<UserAccountQueryResult> Query(string filter)
        {
            var query =
                from a in items
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

            return result;
        }

        public System.Collections.Generic.IEnumerable<UserAccountQueryResult> Query(string tenant, string filter)
        {
            var query =
                from a in items
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

            return result;
        }

        public System.Collections.Generic.IEnumerable<UserAccountQueryResult> Query(string filter, int skip, int count, out int totalCount)
        {
            var query =
                from a in items
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
            return result.Skip(skip).Take(count);
        }

        public System.Collections.Generic.IEnumerable<UserAccountQueryResult> Query(string tenant, string filter, int skip, int count, out int totalCount)
        {
            var query =
                from a in items
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
            return result.Skip(skip).Take(count);
        }
    }
}
