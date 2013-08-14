/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Raven.Client;

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
    }
}
