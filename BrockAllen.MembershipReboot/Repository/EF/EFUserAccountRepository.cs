using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class EFUserAccountRepository 
        : DbContextRepository<UserAccount, int, EFMembershipRebootDatabase>, IUserAccountRepository
    {
        public EFUserAccountRepository()
        {
        }

        public EFUserAccountRepository(string name) : base(new EFMembershipRebootDatabase(name))
        {
        }
    }
}
