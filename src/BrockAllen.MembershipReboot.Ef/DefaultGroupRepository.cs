/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


namespace BrockAllen.MembershipReboot.Ef
{
    public class DefaultGroupRepository
        : DbContextGroupRepository<DefaultMembershipRebootDatabase, RelationalGroup>,
          IGroupRepository
    {
        public DefaultGroupRepository()
        {
        }

        public DefaultGroupRepository(string name)
            : base(new DefaultMembershipRebootDatabase(name))
        {
        }

        public DefaultGroupRepository(string name, string schemaName)
            : base(new DefaultMembershipRebootDatabase(name, schemaName))
        {
        }

        IGroupRepository<RelationalGroup> This { get { return (IGroupRepository<RelationalGroup>)this; } }

        public new Group Create()
        {
            return This.Create();
        }

        public void Add(Group item)
        {
            This.Add((RelationalGroup)item);
        }

        public void Remove(Group item)
        {
            This.Remove((RelationalGroup)item);
        }

        public void Update(Group item)
        {
            This.Update((RelationalGroup)item);
        }

        public new Group GetByID(System.Guid id)
        {
            return This.GetByID(id);
        }

        public new Group GetByName(string tenant, string name)
        {
            return This.GetByName(tenant, name);
        }

        public new System.Collections.Generic.IEnumerable<Group> GetByIDs(System.Guid[] ids)
        {
            return This.GetByIDs(ids);
        }

        public new System.Collections.Generic.IEnumerable<Group> GetByChildID(System.Guid childGroupID)
        {
            return This.GetByChildID(childGroupID);
        }
    }
}
