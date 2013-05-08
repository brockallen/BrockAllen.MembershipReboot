using System;
using System.Data.Entity;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    public class DbContextRepository<T> : IRepository<T>, IDisposable
        where T : class
    {
        protected DbContext db;
        DbSet<T> items;

        public DbContextRepository(DbContext db)
        {
            this.db = db;
            this.items = db.Set<T>();
        }

        IQueryable<T> IRepository<T>.GetAll()
        {
            return items;
        }

        T IRepository<T>.Get(params object[] keys)
        {
            return items.Find(keys);
        }

        void IRepository<T>.Add(T item)
        {
            items.Add(item);
        }

        void IRepository<T>.Remove(params object[] keys)
        {
            var item = items.Find(keys);
            if (item != null) items.Remove(item);
        }

        void IRepository<T>.Remove(T item)
        {
            items.Remove(item);
        }

        void IRepository<T>.SaveChanges()
        {
            db.SaveChanges();
        }

        public void Dispose()
        {
        }
    }
}
