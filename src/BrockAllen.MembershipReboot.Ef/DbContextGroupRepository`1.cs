/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Data.Entity;
using System.Linq;
namespace BrockAllen.MembershipReboot.Ef
{
    public class DbContextGroupRepository<Ctx> : IGroupRepository, IDisposable
        where Ctx : DbContext, new()
    {
        protected DbContext db;
        DbSet<Group> items;
        
        public DbContextGroupRepository()
            : this(new Ctx())
        {
        }
        public DbContextGroupRepository(Ctx ctx)
        {
            this.db = ctx;
            this.items = db.Set<Group>();
        }

        void CheckDisposed()
        {
            if (db == null)
            {
                throw new ObjectDisposedException("DbContextGroupRepository");
            }
        }

        IQueryable<Group> IGroupRepository.GetAll()
        {
            CheckDisposed();
            return items;
        }

        Group IGroupRepository.Get(Guid key)
        {
            CheckDisposed();
            return items.Find(key);
        }

        Group IGroupRepository.Create()
        {
            CheckDisposed();
            return items.Create();
        }

        void IGroupRepository.Add(Group item)
        {
            CheckDisposed();
            items.Add(item);
            db.SaveChanges();
        }

        void IGroupRepository.Remove(Group item)
        {
            CheckDisposed();
            items.Remove(item);
            db.SaveChanges();
        }

        void IGroupRepository.Update(Group item)
        {
            CheckDisposed();

            var entry = db.Entry(item);
            if (entry.State == EntityState.Detached)
            {
                items.Attach(item);
                entry.State = EntityState.Modified;
            }
            db.SaveChanges();
        }

        public void Dispose()
        {
            if (db.TryDispose())
            {
                db = null;
                items = null;
            }
        }
    }
}
