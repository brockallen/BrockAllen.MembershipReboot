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
    public class RavenGroupRepository : IGroupRepository, IDisposable
    {
        public RavenGroupRepository(string connectionStringName)
            : this((DocumentStore) new RavenMembershipRebootDatabase(connectionStringName).DocumentStore)
        {
        }

        public RavenGroupRepository(IDocumentStore documentStore)
        {
            this.documentStore = documentStore;
            documentSession = documentStore.OpenSession();
            items = documentSession.Query<Group>();
        }

        private readonly IDocumentStore documentStore;
        private readonly IDocumentSession documentSession;
        private readonly IRavenQueryable<Group> items;

        protected void CheckDisposed()
        {
            if (documentStore == null || documentSession == null)
            {
                throw new ObjectDisposedException("RavenDbRepository<T>");
            }
        }

        IQueryable<Group> IGroupRepository.GetAll()
        {
            CheckDisposed();
            return items;
        }

        public Group Get(Guid key)
        {
            CheckDisposed();
            return items.Where(x => x.ID == key).SingleOrDefault();
        }

        Group IGroupRepository.Create()
        {
            CheckDisposed();
            return new Group();
        }

        void IGroupRepository.Add(Group item)
        {
            CheckDisposed();
            documentSession.Store(item);
            documentSession.SaveChanges();
        }

        void IGroupRepository.Remove(Group item)
        {
            CheckDisposed();
            documentSession.Delete(item);
            documentSession.SaveChanges();
        }
        
        void IGroupRepository.Update(Group item)
        {
            CheckDisposed();
            documentSession.Store(item);
            documentSession.SaveChanges();
        }

        public void Dispose()
        {
            documentSession.TryDispose();
        }
    }
}
