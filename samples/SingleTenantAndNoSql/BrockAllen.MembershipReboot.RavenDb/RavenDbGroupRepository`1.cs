/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Raven.Client;
using System;
using System.Linq;

namespace BrockAllen.MembershipReboot.RavenDb
{
    public class RavenDbGroupRepository<Ctx>
        : RavenDbRepository<Group>, IGroupRepository
        where Ctx : IDocumentStore, new()
    {
        public RavenDbGroupRepository(Ctx ctx)
            : base(ctx)
        {
        }
        
        public override Group Get(Guid key)
        {
            CheckDisposed();
            IGroupRepository r = this;
            return r.GetAll().Where(x => x.ID == key).SingleOrDefault();
        }
    }
}
