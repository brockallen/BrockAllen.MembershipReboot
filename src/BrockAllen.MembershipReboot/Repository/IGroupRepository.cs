/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
namespace BrockAllen.MembershipReboot
{
    public interface IGroupRepository<TGroup>
        where TGroup : Group
    {
        TGroup Create();
        void Add(TGroup item);
        void Remove(TGroup item);
        void Update(TGroup item);

        TGroup GetByID(Guid id);
        TGroup GetByName(string tenant, string name);

        IEnumerable<TGroup> GetByIDs(Guid[] ids);
        IEnumerable<TGroup> GetByChildID(Guid childGroupID);
    }
    
    public interface IGroupRepository : IGroupRepository<Group> { }
}
