/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Data.Entity;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    public class DbContextRepository<T, Ctx> : IRepository<T>, IDisposable
        where T : class
        where Ctx : DbContext, new()
    {
        protected DbContext db;
        DbSet<T> items;

        public DbContextRepository()
            : this(new Ctx())
        {
        }

        public DbContextRepository(DbContext db)
        {
            this.db = db;
            this.items = db.Set<T>();
        }

        void CheckDisposed()
        {
            if (db == null)
            {
                throw new ObjectDisposedException("DbContextRepository<T>");
            }
        }

        IQueryable<T> IRepository<T>.GetAll()
        {
            CheckDisposed();
            return items;
        }

        T IRepository<T>.Get(params object[] keys)
        {
            CheckDisposed();
            return items.Find(keys);
        }

        T IRepository<T>.Create()
        {
            CheckDisposed();
            return items.Create();
        }

        void IRepository<T>.Add(T item)
        {
            CheckDisposed();
            items.Add(item);
            db.SaveChanges();
        }

        void IRepository<T>.Remove(T item)
        {
            CheckDisposed();
            items.Remove(item);
            db.SaveChanges();
        }
        
        void IRepository<T>.Update(T item)
        {
            CheckDisposed();

            var entry = db.Entry(item);
            if (entry.State == System.Data.EntityState.Detached)
            {
                items.Attach(item);
                entry.State = System.Data.EntityState.Modified;
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
