/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.Linq;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace BrockAllen.MembershipReboot.RavenDb
{
    public class RavenMembershipRebootDatabase
    {
        public IDocumentStore DocumentStore { get; private set; }

        public RavenMembershipRebootDatabase(string connectionStringName)
        {
            DocumentStore = new DocumentStore
                {
                    ConnectionStringName = connectionStringName
                }.Initialize();
            IndexCreation.CreateIndexes(typeof(UserAccount_AccountByProviderAndId).Assembly, DocumentStore);
        }

        public RavenMembershipRebootDatabase(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
        }
    }

    public class UserAccount_AccountByProviderAndId : AbstractIndexCreationTask<UserAccount> 
    {
        public UserAccount_AccountByProviderAndId() 
        {
            Map = accounts => from account in accounts
                              from linkedAccount in account.LinkedAccounts
                              select new 
                              {
                                  Provider = linkedAccount.ProviderName,
                                  Id = linkedAccount.ProviderAccountID
                              };
      }
    }
}
