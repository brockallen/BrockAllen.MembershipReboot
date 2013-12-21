/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Linq;
namespace BrockAllen.MembershipReboot
{
    public interface IGroupRepository
    {
        IQueryable<Group> GetAll();
        Group Get(Guid key);
        Group Create();
        void Add(Group item);
        void Remove(Group item);
        void Update(Group item);
    }
}
