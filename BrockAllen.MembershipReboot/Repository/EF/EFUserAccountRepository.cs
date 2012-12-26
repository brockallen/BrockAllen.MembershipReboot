using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class EFUserAccountRepository : IUserAccountRepository, IDisposable
    {
        EFMembershipRebootDatabase db;

        public EFUserAccountRepository()
        {
            db = new EFMembershipRebootDatabase();
        }
        public EFUserAccountRepository(string nameOrConnectionString)
        {
            db = new EFMembershipRebootDatabase(nameOrConnectionString);
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
            return db.Users.Where(x=>x.IsAccountClosed == false);
        }

        public void Add(UserAccount item)
        {
            db.Users.Add(item);
        }

        public void Remove(UserAccount item)
        {
            foreach (var claim in item.Claims.ToArray())
            {
                item.Claims.Remove(claim);
            }
            db.Users.Remove(item);
        }

        public void SaveChanges()
        {
            db.SaveChanges();
        }
    }
}
