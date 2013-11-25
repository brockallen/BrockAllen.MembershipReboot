/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


using System;
using System.Data.Entity;
using System.Linq;
namespace BrockAllen.MembershipReboot.Ef
{
    public class DefaultGroupRepository : IGroupRepository
    {
        public DefaultGroupRepository()
            : this(new DefaultMembershipRebootDatabase())
        {
        }

        public DefaultGroupRepository(string name)
            : this(new DefaultMembershipRebootDatabase(name))
        {
        }

        public DefaultGroupRepository(DbContext db)
        {
            this.db = db;
            this.items = db.Set<Group>();
        }

        protected DbContext db;
        DbSet<Group> items;

        void CheckDisposed()
        {
            if (db == null)
            {
                throw new ObjectDisposedException("DbContextRepository<T>");
            }
        }

        public IQueryable<Group> GetAll()
        {
            CheckDisposed();
            return items;
        }

        public Group Get(Guid key)
        {
            CheckDisposed();
            return items.Find(key);
        }

        public Group Create()
        {
            CheckDisposed();
            return items.Create();
        }

        public void Add(Group item)
        {
            CheckDisposed();
            items.Add(item);
            db.SaveChanges();
        }

        public void Remove(Group item)
        {
            CheckDisposed();
            items.Remove(item);
            db.SaveChanges();
        }

        public void Update(Group item)
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
