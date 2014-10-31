/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    public class GroupService<TGroup>
        where TGroup : Group
    {
        public string DefaultTenant { get; private set; }
        
        IGroupRepository<TGroup> groupRepository;

        public GroupService(IGroupRepository<TGroup> groupRepository)
            : this(null, groupRepository)
        {
        }

        public GroupService(string defaultTenant, IGroupRepository<TGroup> groupRepository)
        {
            if (groupRepository == null) throw new ArgumentNullException("groupRepository");

            this.DefaultTenant = defaultTenant;
            this.groupRepository = groupRepository;
        }

        public virtual IGroupQuery Query
        {
            get
            {
                return groupRepository as IGroupQuery;
            }
        }

        public TGroup Get(Guid groupID)
        {
            return this.groupRepository.GetByID(groupID);
        }

        public TGroup Get(string name)
        {
            return this.groupRepository.GetByName(DefaultTenant, name);
        }
        
        public TGroup Get(string tenant, string name)
        {
            return this.groupRepository.GetByName(tenant, name);
        }

        bool NameAlreadyExists(string tenant, string name)
        {
            var grp = this.groupRepository.GetByName(tenant, name);
            return grp != null;
        }
        
        public TGroup Create(string name)
        {
            return Create(DefaultTenant, name);
        }

        public TGroup Create(string tenant, string name)
        {
            if (String.IsNullOrWhiteSpace(tenant)) throw new ValidationException(Resources.ValidationMessages.TenantRequired);
            if (String.IsNullOrWhiteSpace(name)) throw new ValidationException(Resources.ValidationMessages.NameRequired);

            if (NameAlreadyExists(tenant, name))
            {
                throw new ValidationException(Resources.ValidationMessages.NameAlreadyInUse);
            }

            var grp = this.groupRepository.Create();
            
            grp.ID = Guid.NewGuid();
            grp.Tenant = tenant;
            grp.Name = name;
            grp.Created = grp.LastUpdated = UtcNow;
            
            groupRepository.Add(grp);
            
            return grp;
        }

        public void Delete(Guid groupID)
        {
            var grp = Get(groupID);
            if (grp == null) throw new ArgumentException("Invalid GroupID");

            this.groupRepository.Remove(grp);
            RemoveChildGroupFromOtherGroups(groupID);
        }

        private void RemoveChildGroupFromOtherGroups(Guid childGroupID)
        {
            var groups = this.groupRepository.GetByChildID(childGroupID);
            foreach (var group in groups)
            {
                RemoveChildGroup(group, childGroupID);
                Update(group);
            }
        }

        public void Update(TGroup group)
        {
            group.LastUpdated = UtcNow;
            this.groupRepository.Update(group);
        }

        public void ChangeName(Guid groupID, string name)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ValidationException(Resources.ValidationMessages.InvalidName);

            var group = Get(groupID);
            if (group == null) throw new ArgumentException("Invalid GroupID");

            if (NameAlreadyExists(group.Tenant, name))
            {
                throw new ValidationException(Resources.ValidationMessages.NameAlreadyInUse);
            }

            group.Name = name;
            Update(group);
        }

        public void AddChildGroup(Guid groupID, Guid childGroupID)
        {
            var group = Get(groupID);
            if (group == null) throw new ArgumentException("Invalid GroupID");

            var childGroup = Get(childGroupID);
            if (childGroup == null) throw new ArgumentException("Invalid ChildGroupID");

            // can't add self as child
            if (group.ID == childGroupID)
            {
                throw new ValidationException(Resources.ValidationMessages.ParentGroupSameAsChild);
            }

            // can't add child to group if group is already inside of child
            var descendants = GetDescendants(childGroupID).Select(x => x.ID);
            if (descendants.Contains(groupID))
            {
                throw new ValidationException(Resources.ValidationMessages.ParentGroupAlreadyChild);
            } 

            var child = group.Children.SingleOrDefault(x => x.ChildGroupID == childGroupID);
            if (child == null)
            {
                group.AddChild(new GroupChild { ChildGroupID = childGroupID });
            }
            
            Update(group);
        }

        public void RemoveChildGroup(Guid groupID, Guid childGroupID)
        {
            var group = Get(groupID);
            if (group == null) throw new ArgumentException("Invalid GroupID");

            RemoveChildGroup(group, childGroupID);

            Update(group);
        }
        
        void RemoveChildGroup(Group group, Guid childGroupID)
        {
            if (group == null) throw new ArgumentNullException("group");

            if (group.ID == childGroupID) return;

            var child = group.Children.SingleOrDefault(x => x.ChildGroupID == childGroupID);
            if (child != null)
            {
                group.RemoveChild(child);
            }
        }

        public virtual IEnumerable<TGroup> GetDescendants(Guid groupID)
        {
            var group = Get(groupID);
            if (group == null) throw new ArgumentException("Invalid GroupID");

            return GetDescendants(group);
        }
        
        public virtual IEnumerable<TGroup> GetDescendants(Group grp)
        {
            if (grp == null) throw new ArgumentNullException("group");

            var children = GetChildren(grp);
            var result = children;
            foreach (var child in children)
            {
                result = result.Union(GetDescendants(child));
            }
            return result;
        }
        
        public virtual IEnumerable<TGroup> GetChildren(Group grp)
        {
            if (grp == null) throw new ArgumentNullException("group");
            var ids = grp.Children.Select(x=>x.ChildGroupID).ToArray();
            if (ids.Length == 0) return Enumerable.Empty<TGroup>();
            return this.groupRepository.GetByIDs(ids);
        }

        protected virtual DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }
    }

    public class GroupService : GroupService<Group> 
    {
        public GroupService(IGroupRepository<Group> groupRepository)
            : base(null, groupRepository)
        {
        }

        public GroupService(string defaultTenant, IGroupRepository<Group> groupRepository)
            : base(defaultTenant, groupRepository)
        {
        }
    }
}
