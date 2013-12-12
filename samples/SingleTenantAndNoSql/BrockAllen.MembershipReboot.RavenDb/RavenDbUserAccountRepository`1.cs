/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Raven.Client;
using System;
using System.Linq;

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
        
        public override UserAccount Get(Guid key)
        {
            CheckDisposed();
            IUserAccountRepository r = this;
            return r.GetAll().Where(x => x.ID == key).SingleOrDefault();
        }
    }
}
