using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class GroupService
    {
        IGroupRepository groupRepository;
        public GroupService(IGroupRepository groupRepository)
        {
            if (groupRepository == null) throw new ArgumentNullException("groupRepository");

            this.groupRepository = groupRepository;
        }


    }
}
