using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public DbContextRepository(Ctx db)
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
