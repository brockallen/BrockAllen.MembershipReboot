using System;
using System.Data.Entity;

namespace BrockAllen.MembershipReboot
{
    public class DbContextRepository<T, Key, Ctx> : DbContextRepository<T, Key>, IDisposable
        where T : class
        where Ctx : DbContext, new()
    {
        public DbContextRepository()
            : base(new Ctx())
        {
        }

        public DbContextRepository(DbContext db)
            : base(db)
        {
        }
        
        public new void Dispose()
        {
            base.Dispose();

            if (db.TryDispose())
            {
                db = null;
            }
        }
    }
}
