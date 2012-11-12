using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.EF
{
    public class EFUserAccountRepository : IUserAccountRepository
    {
        EFMembershipRebootDatabase db = new EFMembershipRebootDatabase();

        public UserAccount GetByUsername(string username)
        {
            if (String.IsNullOrWhiteSpace(username)) throw new ArgumentException("username");

            return db.UserAcounts.SingleOrDefault(x => x.Username == username);
        }

        public UserAccount GetByEmail(string email)
        {
            if (String.IsNullOrWhiteSpace(email)) throw new ArgumentException("email");

            return db.UserAcounts.SingleOrDefault(x => x.Email == email);
        }

        public UserAccount GetByResetKey(string key)
        {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentException("key");
            
            return db.UserAcounts.SingleOrDefault(x => x.ResetKey == key && x.ResetKey != null);
        }

        public void Create(UserAccount account)
        {
            if (account == null) throw new ArgumentException("account");
            
            db.UserAcounts.Add(account);
            
            db.SaveChanges();
        }

        public void Update(UserAccount account)
        {
            if (account == null) throw new ArgumentException("account");
            
            if (db.Entry(account).State == System.Data.EntityState.Detached)
            {
                db.UserAcounts.Attach(account);
                db.Entry(account).State = System.Data.EntityState.Modified;
            }
            
            db.SaveChanges();
        }

        public void Delete(UserAccount account)
        {
            if (account == null) throw new ArgumentException("account");

            if (db.Entry(account).State == System.Data.EntityState.Detached)
            {
                db.UserAcounts.Attach(account);
            }
            
            db.UserAcounts.Remove(account);
            
            db.SaveChanges();
        }
    }
}
