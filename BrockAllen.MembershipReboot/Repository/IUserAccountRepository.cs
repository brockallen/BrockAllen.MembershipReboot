using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public interface IUserAccountRepository
    {
        UserAccount GetByUsername(string username);
        UserAccount GetByEmail(string email);
        UserAccount GetByResetKey(string key);
        void Create(UserAccount account);
        void Update(UserAccount account);
        void Delete(UserAccount account);
    }
}
