/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Raven.Client;

namespace BrockAllen.MembershipReboot.RavenDb
{
    public class RavenDbUserAccountRepository<Ctx>
        : RavenDbRepository<UserAccount>, IUserAccountRepository
        where Ctx : IDocumentStore, new()
    {
        public RavenDbUserAccountRepository(Ctx ctx)
            : base(ctx)
        {
        }
    }
}
