/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Linq;
using System;
using System.Linq;

namespace BrockAllen.MembershipReboot.RavenDb
{
    public class RavenGroupRepository :
        QueryableGroupRepository<HierarchicalGroup>, IDisposable
    {
        public RavenGroupRepository(string connectionStringName)
            : this((DocumentStore) new RavenMembershipRebootDatabase(connectionStringName).DocumentStore)
        {
        }

        public RavenGroupRepository(IDocumentStore documentStore)
        {
            this.documentStore = documentStore;
            documentSession = documentStore.OpenSession();
            items = documentSession.Query<HierarchicalGroup>();
        }

        private readonly IDocumentStore documentStore;
        private readonly IDocumentSession documentSession;
        private readonly IRavenQueryable<HierarchicalGroup> items;

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

        protected override IQueryable<HierarchicalGroup> Queryable
        {
            get
            {
                CheckDisposed();
                return items;
            }
        }

        public override HierarchicalGroup Create()
        {
            CheckDisposed();
            return new HierarchicalGroup();
        }

        public override void Add(HierarchicalGroup item)
        {
            CheckDisposed();
            documentSession.Store(item);
            documentSession.SaveChanges();
        }

        public override void Remove(HierarchicalGroup item)
        {
            CheckDisposed();
            documentSession.Delete(item);
            documentSession.SaveChanges();
        }

        public override void Update(HierarchicalGroup item)
        {
            CheckDisposed();
            documentSession.Store(item);
            documentSession.SaveChanges();
        }

        public override System.Collections.Generic.IEnumerable<HierarchicalGroup> GetByChildID(Guid childGroupID)
        {
            var q =
                from g in Queryable
                from c in g.Children
                where c.ChildGroupID == childGroupID
                select g;
            return q;
        }
    }
}
