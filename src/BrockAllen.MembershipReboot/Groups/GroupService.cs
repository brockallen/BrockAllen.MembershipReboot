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
    public class GroupService
    {
        MembershipRebootConfiguration<UserAccount> configuration;
        IGroupRepository groupRepository;

        public GroupService(IGroupRepository groupRepository)
            : this(new MembershipRebootConfiguration<UserAccount>(), groupRepository)
        {
        }

        public GroupService(MembershipRebootConfiguration<UserAccount> configuration, IGroupRepository groupRepository)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (groupRepository == null) throw new ArgumentNullException("groupRepository");

            this.configuration = configuration;
            this.groupRepository = groupRepository;
        }

        public IQueryable<Group> GetAll()
        {
            return GetAll(null);
        }
        
        public IQueryable<Group> GetAll(string tenant)
        {
            if (!configuration.MultiTenant)
            {
                tenant = configuration.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) throw new ArgumentNullException("tenant");

            return this.groupRepository.GetAll().Where(x=>x.Tenant == tenant);
        }

        public Group Get(Guid groupID)
        {
            return this.groupRepository.Get(groupID);
        }

        public Group Create(string name)
        {
            return Create(null, name);
        }

        bool NameAlreadyExists(string tenant, string name, Guid? exclude = null)
        {
            var query = GetAll(tenant).Where(x => x.Name == name);
            if (exclude.HasValue)
            {
                query = query.Where(x => x.ID != exclude.Value);
            }
            return query.Any();
        }

        public Group Create(string tenant, string name)
        {
            if (!configuration.MultiTenant)
            {
                tenant = configuration.DefaultTenant;
            }

            if (NameAlreadyExists(tenant, name))
            {
                throw new ValidationException(Resources.ValidationMessages.NameAlreadyInUse);
            }

            if (String.IsNullOrWhiteSpace(tenant)) throw new ValidationException(Resources.ValidationMessages.TenantRequired);
            if (String.IsNullOrWhiteSpace(name)) throw new ValidationException(Resources.ValidationMessages.NameRequired);
            
            var grp = this.groupRepository.Create();
            
            if (grp.Children == null) grp.Children = new HashSet<GroupChild>();
            
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
            var query =
                from g in this.groupRepository.GetAll()
                from c in g.Children
                where c.ChildGroupID == childGroupID
                select g;
            foreach (var group in query.ToArray())
            {
                RemoveChildGroup(group, childGroupID);
                Update(group);
            }
        }

        private void Update(Group group)
        {
            group.LastUpdated = UtcNow;
            this.groupRepository.Update(group);
        }

        public void ChangeName(Guid groupID, string name)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ValidationException(Resources.ValidationMessages.InvalidName);

            var group = Get(groupID);
            if (group == null) throw new ArgumentException("Invalid GroupID");
            
            if (NameAlreadyExists(group.Tenant, name, groupID))
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
                group.Children.Add(new GroupChild { GroupID = group.ID, ChildGroupID = childGroupID });
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
                group.Children.Remove(child);
            }
        }

        public virtual IEnumerable<Group> GetDescendants(Guid groupID)
        {
            var group = Get(groupID);
            if (group == null) throw new ArgumentException("Invalid GroupID");

            return GetDescendants(group);
        }
        
        public virtual IEnumerable<Group> GetDescendants(Group grp)
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
        
        public virtual IEnumerable<Group> GetChildren(Group grp)
        {
            if (grp == null) throw new ArgumentNullException("group");
            var ids = grp.Children.Select(x=>x.ChildGroupID).ToArray();
            if (ids.Length == 0) return Enumerable.Empty<Group>();
            return this.groupRepository.GetAll().Where(x => ids.Contains(x.ID));
        }

        protected virtual DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }
    }
}
