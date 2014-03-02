using System;
using System.Collections.Generic;

namespace BrockAllen.MembershipReboot
{
    public class RelationalGroup : Group
    {
        public virtual int Key { get; set; }

        public virtual ICollection<RelationalGroupChild> ChildrenCollection { get; set; }
        public override IEnumerable<GroupChild> Children
        {
            get { return ChildrenCollection; }
        }

        protected internal override void AddChild(GroupChild child)
        {
            ChildrenCollection.Add(new RelationalGroupChild{ParentKey = this.Key, ChildGroupID = child.ChildGroupID});
        }

        protected internal override void RemoveChild(GroupChild child)
        {
            ChildrenCollection.Remove((RelationalGroupChild)child);
        }
    }

    public class RelationalGroupChild : GroupChild
    {
        public virtual int Key { get; set; }
        public virtual int ParentKey { get; set; }
    }
}
