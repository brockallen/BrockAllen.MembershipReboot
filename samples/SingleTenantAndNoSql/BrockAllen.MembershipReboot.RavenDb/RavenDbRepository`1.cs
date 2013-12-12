/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Raven.Client;
using Raven.Client.Linq;

namespace BrockAllen.MembershipReboot.RavenDb
{
    public class RavenDbRepository<T> : IRepository<T>, IDisposable
        where T : class, new()
    {
        private readonly IDocumentStore documentStore;
        private readonly IDocumentSession documentSession;
        private readonly IRavenQueryable<T> items;

        public RavenDbRepository(IDocumentStore documentStore)
        {
            this.documentStore = documentStore;
            documentSession = documentStore.OpenSession();
            items = documentSession.Query<T>();
        }

        protected void CheckDisposed()
        {
            if (documentStore == null || documentSession == null)
            {
                throw new ObjectDisposedException("RavenDbRepository<T>");
            }
        }

        IQueryable<T> IRepository<T>.GetAll()
        {
            CheckDisposed();
            return items;
        }

        public virtual T Get(Guid key)
        {
            CheckDisposed();
            return items.SingleOrDefault(x => x.In(new object[]{key}));
        }

        T IRepository<T>.Create()
        {
            CheckDisposed();
            return new T();
        }

        void IRepository<T>.Add(T item)
        {
            CheckDisposed();
            documentSession.Store(item);
            documentSession.SaveChanges();
        }

        void IRepository<T>.Remove(T item)
        {
            CheckDisposed();
            documentSession.Delete(item);
            documentSession.SaveChanges();
        }
        
        void IRepository<T>.Update(T item)
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
