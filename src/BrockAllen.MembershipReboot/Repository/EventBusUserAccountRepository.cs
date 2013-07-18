using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class EventBusUserAccountRepository :
       EventBusRepository<UserAccount>, IUserAccountRepository
    {
        public EventBusUserAccountRepository(IUserAccountRepository inner, IEventBus eventBus)
            : base(inner, eventBus)
        {
        }
    }
}
