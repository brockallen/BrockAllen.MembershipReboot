using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public interface IUserAccountRepository : IDisposable
    {
        IQueryable<UserAccount> GetAll();
        UserAccount GetByUsername(string tenant, string username);
        UserAccount GetByEmail(string tenant, string email);
        UserAccount GetByVerificationKey(string key);
        void Add(UserAccount item);
        void Remove(UserAccount item);
        void SaveChanges();
    }
}
