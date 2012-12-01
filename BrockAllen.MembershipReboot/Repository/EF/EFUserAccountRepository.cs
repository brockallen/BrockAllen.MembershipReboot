using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.EF
{
    public class UserAccountRepository : IUserAccountRepository, IDisposable
    {
        EFMembershipRebootDatabase db;

        public UserAccountRepository()
        {
            db = new EFMembershipRebootDatabase();
        }
        public UserAccountRepository(string nameOrConnectionString)
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
            db.Users.Remove(item);
        }

        public void SaveChanges()
        {
            db.SaveChanges();
        }

        public UserAccount GetByUsername(string username)
        {
            return GetAll().Where(x => x.Username == username).SingleOrDefault();
        }

        public UserAccount GetByEmail(string email)
        {
            return GetAll().Where(x => x.Email == email).SingleOrDefault();
        }

        public UserAccount GetByVerificationKey(string key)
        {
            return GetAll().Where(x => x.VerificationKey == key).SingleOrDefault();
        }
    }
}
