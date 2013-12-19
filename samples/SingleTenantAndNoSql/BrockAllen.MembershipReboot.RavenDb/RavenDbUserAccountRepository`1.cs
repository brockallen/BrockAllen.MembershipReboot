/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using BrockAllen.MembershipReboot.Hierarchical;
using Raven.Client;
using System;
using System.Linq;

namespace BrockAllen.MembershipReboot.RavenDb
{
    public class RavenDbUserAccountRepository<Ctx>
        : RavenDbRepository<HierarchicalUserAccount>, IUserAccountRepository
        where Ctx : IDocumentStore, new()
    {
        public RavenDbUserAccountRepository(Ctx ctx)
            : base(ctx)
        {
        }

        public override HierarchicalUserAccount Get(Guid key)
        {
            CheckDisposed();
            return This.GetAll().Where(x => x.ID == key).SingleOrDefault();
        }

        IRepository<HierarchicalUserAccount> This { get { return (IRepository<HierarchicalUserAccount>)this; } }

        IQueryable<UserAccount> IRepository<UserAccount>.GetAll()
        {
            return This.GetAll();
        }

        UserAccount IRepository<UserAccount>.Get(Guid key)
        {
            return This.Get(key);
        }

        UserAccount IRepository<UserAccount>.Create()
        {
            return This.Create();
        }

        void IRepository<UserAccount>.Add(UserAccount item)
        {
            This.Add((HierarchicalUserAccount)item);
        }

        void IRepository<UserAccount>.Remove(UserAccount item)
        {
            This.Remove((HierarchicalUserAccount)item);
        }

        void IRepository<UserAccount>.Update(UserAccount item)
        {
            This.Update((HierarchicalUserAccount)item);
        }

        void IDisposable.Dispose()
        {
            base.Dispose();
        }
    }
}
