using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.Test
{
    public class FakeUserAccountRepository : IUserAccountRepository
    {
        public List<UserAccount> UserAccounts = new List<UserAccount>();

        public System.Linq.IQueryable<UserAccount> GetAll()
        {
            return UserAccounts.AsQueryable();
        }

        public UserAccount Get(Guid key)
        {
            return UserAccounts.SingleOrDefault(x => x.ID == key);
        }

        public UserAccount Create()
        {
            return new UserAccount();
        }

        public void Add(UserAccount item)
        {
            this.UserAccounts.Add(item);
        }

        public void Remove(UserAccount item)
        {
            this.UserAccounts.Remove(item);
        }

        public void Update(UserAccount item)
        {
            var other = Get(item.ID);
            if (other != item)
            {
                Remove(other);
                Add(item);
            }
        }

        public void Dispose()
        {
        }
    }
}