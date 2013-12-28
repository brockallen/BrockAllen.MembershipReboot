/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using BrockAllen.MembershipReboot.Relational;
using System;
using System.Data.Entity;
using System.Linq;
namespace BrockAllen.MembershipReboot.Ef
{
    public class DbContextUserAccountRepository<Ctx, TAccount> :
        QueryableUserAccountRepository<TAccount>, IDisposable
        where Ctx : DbContext, new()
        where TAccount : RelationalUserAccount
    {
        protected DbContext db;
        DbSet<TAccount> items;

        public DbContextUserAccountRepository()
            : this(new Ctx())
        {
        }

        public DbContextUserAccountRepository(Ctx ctx)
        {
            this.db = ctx;
            this.items = db.Set<TAccount>();
        }

        void CheckDisposed()
        {
            if (db == null)
            {
                throw new ObjectDisposedException("DbContextRepository<T>");
            }
        }

        public void Dispose()
        {
            if (db.TryDispose())
            {
                db = null;
                items = null;
            }
        }

        protected override IQueryable<TAccount> Queryable
        {
            get
            {
                CheckDisposed();
                return this.items;
            }
        }

        public override TAccount Create()
        {
            CheckDisposed();
            return items.Create();
        }

        public override void Add(TAccount item)
        {
            CheckDisposed();
            items.Add(item);
            db.SaveChanges();
        }

        public override void Remove(TAccount item)
        {
            CheckDisposed();
            items.Remove(item);
            db.SaveChanges();
        }

        public override void Update(TAccount item)
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

        public override TAccount GetByLinkedAccount(string tenant, string provider, string id)
        {
            var q =
                from a in items
                where a.Tenant == tenant
                from la in a.LinkedAccountCollection
                where la.ProviderName == provider && la.ProviderAccountID == id
                select a;
            return q.SingleOrDefault();
        }

        public override TAccount GetByCertificate(string tenant, string thumbprint)
        {
            var q =
                from a in items
                where a.Tenant == tenant
                from c in a.UserCertificateCollection
                where c.Thumbprint == thumbprint
                select a;
            return q.SingleOrDefault();
        }
    }
    public class DbContextUserAccountRepositoryInt<Ctx, TAccount> :
        QueryableUserAccountRepository<TAccount>, IDisposable
        where Ctx : DbContext, new()
        where TAccount : RelationalUserAccountInt
    {
        protected DbContext db;
        DbSet<TAccount> items;

        public DbContextUserAccountRepositoryInt()
            : this(new Ctx())
        {
        }

        public DbContextUserAccountRepositoryInt(Ctx ctx)
        {
            this.db = ctx;
            this.items = db.Set<TAccount>();
        }

        void CheckDisposed()
        {
            if (db == null)
            {
                throw new ObjectDisposedException("DbContextRepository<T>");
            }
        }

        public void Dispose()
        {
            if (db.TryDispose())
            {
                db = null;
                items = null;
            }
        }

        protected override IQueryable<TAccount> Queryable
        {
            get
            {
                CheckDisposed();
                return this.items;
            }
        }

        public override TAccount Create()
        {
            CheckDisposed();
            return items.Create();
        }

        public override void Add(TAccount item)
        {
            CheckDisposed();
            items.Add(item);
            db.SaveChanges();
        }

        public override void Remove(TAccount item)
        {
            CheckDisposed();
            items.Remove(item);
            db.SaveChanges();
        }

        public override void Update(TAccount item)
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

        public override TAccount GetByLinkedAccount(string tenant, string provider, string id)
        {
            var q =
                from a in items
                where a.Tenant == tenant
                from la in a.LinkedAccountCollection
                where la.ProviderName == provider && la.ProviderAccountID == id
                select a;
            return q.SingleOrDefault();
        }

        public override TAccount GetByCertificate(string tenant, string thumbprint)
        {
            var q =
                from a in items
                where a.Tenant == tenant
                from c in a.UserCertificateCollection
                where c.Thumbprint == thumbprint
                select a;
            return q.SingleOrDefault();
        }
    }
}
