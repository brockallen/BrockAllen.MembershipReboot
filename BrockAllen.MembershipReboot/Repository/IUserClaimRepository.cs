using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public interface IUserClaimRepository
    {
        IEnumerable<UserClaim> Get(string username);
        void Add(string username, UserClaim[] claims);
        void Remove(string username, UserClaim[] claims);
        void Clear(string username);
    }
}
