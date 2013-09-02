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
        
        public override UserAccount Get(params object[] keys)
        {
            CheckDisposed();
            IUserAccountRepository r = this;
            return r.GetAll().Where(x => x.ID == (Guid)keys[0]).SingleOrDefault();
        }

        public UserAccount FindByLinkedAccount(string tenant, string provider, string id) 
        {
            if (String.IsNullOrWhiteSpace(tenant)) return null;
            if (String.IsNullOrWhiteSpace(provider)) return null;
            if (String.IsNullOrWhiteSpace(id)) return null;

            IUserAccountRepository me = this;

            var results = documentSession.Advanced
                .LuceneQuery<UserAccount, UserAccount_AccountByProviderAndId>()
                .Search("Provider", provider)
                .Search("Id", id);
            
            return results.SingleOrDefault();
        }
    }

}
