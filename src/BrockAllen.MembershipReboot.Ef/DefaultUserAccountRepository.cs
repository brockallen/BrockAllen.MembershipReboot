/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


using System;
using System.Data.Entity;
using System.Linq;
namespace BrockAllen.MembershipReboot.Ef
{
    public class DefaultUserAccountRepository : IUserAccountRepository
    {
        public DefaultUserAccountRepository()
            : this(new DefaultMembershipRebootDatabase())
        {
        }

        public DefaultUserAccountRepository(string name)
            : this(new DefaultMembershipRebootDatabase(name))
        {
        }

        public DefaultUserAccountRepository(DbContext db)
        {
            this.db = db;
            this.items = db.Set<UserAccount>();
        }

        protected DbContext db;
        DbSet<UserAccount> items;

        void CheckDisposed()
        {
            if (db == null)
            {
                throw new ObjectDisposedException("DbContextRepository<T>");
            }
        }

        public IQueryable<UserAccount> GetAll()
        {
            CheckDisposed();
            return items;
        }

        public UserAccount Get(Guid key)
        {
            CheckDisposed();
            return items.Find(key);
        }

        public UserAccount Create()
        {
            CheckDisposed();
            return items.Create();
        }

        public void Add(UserAccount item)
        {
            CheckDisposed();
            items.Add((UserAccount)item);
            db.SaveChanges();
        }

        public void Remove(UserAccount item)
        {
            CheckDisposed();
            items.Remove((UserAccount)item);
            db.SaveChanges();
        }

        public void Update(UserAccount item)
        {
            CheckDisposed();

            var entry = db.Entry(item);
            if (entry.State == EntityState.Detached)
            {
                items.Attach((UserAccount)item);
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
