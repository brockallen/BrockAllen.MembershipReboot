﻿/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


using BrockAllen.MembershipReboot.Hierarchical;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Linq;
using System;

namespace BrockAllen.MembershipReboot.RavenDb
{
    public class RavenUserAccountRepository
        : QueryableUserAccountRepository<HierarchicalUserAccount>, IUserAccountRepository, IDisposable
    {
        public RavenUserAccountRepository(string connectionStringName)
            : this((DocumentStore) new RavenMembershipRebootDatabase(connectionStringName).DocumentStore)
        {
        }

        public RavenUserAccountRepository(IDocumentStore documentStore)
        {
            this.documentStore = documentStore;
            documentSession = documentStore.OpenSession();
            items = documentSession.Query<UserAccount>();
        }
        
        private readonly IDocumentStore documentStore;
        private readonly IDocumentSession documentSession;
        private readonly IRavenQueryable<UserAccount> items;

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

        public override HierarchicalUserAccount Create()
        {
            return new HierarchicalUserAccount();
        }
        public override void Add(HierarchicalUserAccount item)
        {
            this.items.AddQueryInput
        }

        


        //IUserAccountRepository<HierarchicalUserAccount> This { get { return (IUserAccountRepository<HierarchicalUserAccount>)this; } }

        //public new UserAccount Create()
        //{
        //    return This.Create();
        //}

        //public void Add(UserAccount item)
        //{
        //    This.Add((HierarchicalUserAccount)item);
        //}

        //public void Remove(UserAccount item)
        //{
        //    This.Remove((HierarchicalUserAccount)item);
        //}

        //public void Update(UserAccount item)
        //{
        //    This.Update((HierarchicalUserAccount)item);
        //}

        //public new UserAccount GetByID(System.Guid id)
        //{
        //    return This.GetByID(id);
        //}

        //public new UserAccount GetByUsername(string username)
        //{
        //    return This.GetByUsername(username);
        //}

        //UserAccount IUserAccountRepository<UserAccount>.GetByUsername(string tenant, string username)
        //{
        //    return This.GetByUsername(tenant, username);
        //}

        //public new UserAccount GetByEmail(string tenant, string email)
        //{
        //    return This.GetByEmail(tenant, email);
        //}

        //public new UserAccount GetByMobilePhone(string tenant, string phone)
        //{
        //    return This.GetByMobilePhone(tenant, phone);
        //}

        //public new UserAccount GetByVerificationKey(string key)
        //{
        //    return This.GetByVerificationKey(key);
        //}

        //public new UserAccount GetByLinkedAccount(string tenant, string provider, string id)
        //{
        //    return This.GetByLinkedAccount(tenant, provider, id);
        //}

        //public new UserAccount GetByCertificate(string tenant, string thumbprint)
        //{
        //    return This.GetByCertificate(tenant, thumbprint);
        //}
    }
}
