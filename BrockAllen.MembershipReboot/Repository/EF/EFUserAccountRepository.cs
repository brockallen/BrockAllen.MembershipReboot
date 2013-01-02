using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class EFUserAccountRepository : IUserAccountRepository, IDisposable
    {
        DbContext db;
        DbSet<UserAccount> users;

        public EFUserAccountRepository()
            : this(new EFMembershipRebootDatabase())
        {
        }

        public EFUserAccountRepository(string nameOrConnectionString)
            : this(new EFMembershipRebootDatabase(nameOrConnectionString))
        {
        }
        
        public EFUserAccountRepository(DbContext db)
        {
            this.db = db;
            this.users = db.Set<UserAccount>();
        }

        public void Dispose()
        {
            if (db != null)
            {
                db.Dispose();
                db = null;
            }
        }

        public IQueryable<UserAccount> GetAll()
        {
            return this.users;
        }

        public void Add(UserAccount item)
        {
            this.users.Add(item);
        }

        public void Remove(UserAccount item)
        {
            this.users.Remove(item);
        }

        public void SaveChanges()
        {
            db.SaveChanges();
        }
    }
}
