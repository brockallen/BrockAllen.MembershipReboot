namespace BrockAllen.MembershipReboot.Nh
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class NhGroup : Group
    {
        /// <summary>
        ///     To help ensure hashcode uniqueness, a carefully selected random number multiplier 
        ///     is used within the calculation.  Goodrich and Tamassia's Data Structures and
        ///     Algorithms in Java asserts that 31, 33, 37, 39 and 41 will produce the fewest number
        ///     of collissions.  See http://computinglife.wordpress.com/2008/11/20/why-do-hash-functions-use-prime-numbers/
        ///     for more information.
        /// </summary>
        private const int HashMultiplier = 31;

        private int? cachedHashcode;

        public virtual long Version { get; protected set; }

        public static bool operator ==(NhGroup lhs, NhGroup rhs)
        {
            return Equals(lhs, rhs);
        }

        public static bool operator !=(NhGroup lhs, NhGroup rhs)
        {
            return !Equals(lhs, rhs);
        }

        public override bool Equals(object obj)
        {
            var other = obj as NhGroup;
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (!this.IsTransient() && !other.IsTransient())
            {
                if (this.ID == other.ID)
                {
                    var otherType = other.GetUnproxiedType();
                    var thisType = this.GetUnproxiedType();
                    return thisType.IsAssignableFrom(otherType) || otherType.IsAssignableFrom(thisType);
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            // once we have a hashcode we'll never change it
            if (this.cachedHashcode.HasValue)
            {
                return this.cachedHashcode.Value;
            }

            // when this instance is new we use the base hash code
            // and remember it, so an instance can NEVER change its
            // hash code.
            if (this.IsTransient())
            {
                this.cachedHashcode = base.GetHashCode();
            }
            else
            {
                unchecked
                {
                    // It's possible for two objects to return the same hash code based on 
                    // identically valued properties, even if they're of two different types, 
                    // so we include the object's type in the hash calculation
                    var hashCode = this.GetType().GetHashCode();
                    this.cachedHashcode = (hashCode * HashMultiplier) ^ this.ID.GetHashCode();
                }
            }

            return this.cachedHashcode.Value;
        }

        protected bool IsTransient()
        {
            return this.Version == default(long);
        }

        /// <summary>
        ///     When NHibernate proxies objects, it masks the type of the actual entity object.
        ///     This wrapper burrows into the proxied object to get its actual type.
        /// 
        ///     Although this assumes NHibernate is being used, it doesn't require any NHibernate
        ///     related dependencies and has no bad side effects if NHibernate isn't being used.
        /// 
        ///     Related discussion is at http://groups.google.com/group/sharp-architecture/browse_thread/thread/ddd05f9baede023a ...thanks Jay Oliver!
        /// </summary>
        protected virtual Type GetUnproxiedType()
        {
            return this.GetType();
        }

        public override IEnumerable<GroupChild> Children
        {
            get
            {
                return this.ChildrenCollection;
            }
        }

        protected override void AddChild(GroupChild child)
        {
            var groupChild = new NhGroupChild();
            groupChild.GetType().GetProperty("ChildGroupID").SetValue(groupChild, child.ChildGroupID);
            groupChild.GetType().GetProperty("Group").SetValue(groupChild, this);
            this.ChildrenCollection.Add(groupChild);
        }

        protected override void RemoveChild(GroupChild child)
        {
            var removed = this.ChildrenCollection.SingleOrDefault(x => x.ChildGroupID == child.ChildGroupID);
            this.ChildrenCollection.Remove(removed);
        }

        public virtual ICollection<NhGroupChild> ChildrenCollection { get; protected set; }
    }
}