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
        QueryableUserAccountRepository<TAccount>
        where Ctx : DbContext
        where TAccount : RelationalUserAccount
    {
        protected DbContext db;
        DbSet<TAccount> items;

        public DbContextUserAccountRepository(Ctx ctx)
        {
            this.db = ctx;
            this.items = db.Set<TAccount>();
        }

        protected override IQueryable<TAccount> Queryable
        {
            get
            {
                return this.items;
            }
        }

        protected virtual void SaveChanges()
        {
            db.SaveChanges();
        }

        public override TAccount Create()
        {
            return items.Create();
        }

        public override void Add(TAccount item)
        {
            items.Add(item);
            SaveChanges();
        }

        public override void Remove(TAccount item)
        {
            items.Remove(item);
            SaveChanges();
        }

        public override void Update(TAccount item)
        {
            var entry = db.Entry(item);
            if (entry.State == EntityState.Detached)
            {
                items.Attach(item);
                entry.State = EntityState.Modified;
            }
            SaveChanges();
        }

        public override TAccount GetByLinkedAccount(string tenant, string provider, string id)
        {
            var q =
                from a in items
                from la in a.LinkedAccountCollection
                where la.ProviderName == provider && la.ProviderAccountID == id && a.Tenant == tenant
                select a;
            return q.SingleOrDefault();
        }

        public override TAccount GetByCertificate(string tenant, string thumbprint)
        {
            var q =
                from a in items
                from c in a.UserCertificateCollection
                where c.Thumbprint == thumbprint && a.Tenant == tenant
                select a;
            return q.SingleOrDefault();
        }
    }
}
