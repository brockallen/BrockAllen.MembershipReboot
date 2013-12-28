using System.Collections.Generic;

namespace BrockAllen.MembershipReboot
{
    public class HierarchicalGroup : Group
    {
        public HashSet<GroupChild> GroupCollection = new HashSet<GroupChild>();
        public override IEnumerable<GroupChild> Children
        {
            get { return GroupCollection; }
        }

        protected internal override void AddChild(GroupChild child)
        {
            GroupCollection.Add(child);
        }

        protected internal override void RemoveChild(GroupChild child)
        {
            GroupCollection.Remove(child);
        }
    }
}
