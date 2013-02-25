using System;
using System.Data.Entity;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    public class DbContextRepository<T, Key> : IRepository<T, Key>, IDisposable
        where T : class
    {
        protected DbContext db;
        DbSet<T> items;
        
        public DbContextRepository(DbContext db)
        {
            this.db = db;
            this.items = db.Set<T>();
        }
        
        IQueryable<T> IRepository<T, Key>.GetAll()
        {
            return items;
        }

        T IRepository<T, Key>.Get(Key key)
        {
            return items.Find(key);
        }
        void IRepository<T, Key>.Add(T item)
        {
            items.Add(item);
        }

        void IRepository<T, Key>.Remove(T item)
        {
            items.Remove(item);
        }

        void IRepository<T, Key>.SaveChanges()
        {
            db.SaveChanges();
        }

        public void Dispose()
        {
        }
    }
}
