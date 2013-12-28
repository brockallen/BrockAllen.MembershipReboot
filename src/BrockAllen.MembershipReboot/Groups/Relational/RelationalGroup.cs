using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class RelationalGroup : Group
    {
        public virtual ICollection<RelationalGroupChild> ChildrenCollection { get; set; }
        public override IEnumerable<GroupChild> Children
        {
            get { return ChildrenCollection; }
        }

        protected internal override void AddChild(GroupChild child)
        {
            ChildrenCollection.Add(new RelationalGroupChild{GroupID = this.ID, ChildGroupID = child.ChildGroupID});
        }

        protected internal override void RemoveChild(GroupChild child)
        {
            ChildrenCollection.Remove((RelationalGroupChild)child);
        }
    }

    public class RelationalGroupChild : GroupChild
    {
        public Guid GroupID { get; set; }
    }
}
