/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


using Raven.Client;
using Raven.Client.Document;

namespace BrockAllen.MembershipReboot.RavenDb
{
    public class RavenUserAccountRepository
        : RavenDbUserAccountRepository<DocumentStore>
    {
        public RavenUserAccountRepository(string connectionStringName)
            : base((DocumentStore) new RavenMembershipRebootDatabase(connectionStringName).DocumentStore)
        {
        }

        public RavenUserAccountRepository(IDocumentStore documentStore)
            : base((DocumentStore) new RavenMembershipRebootDatabase(documentStore).DocumentStore)
        {
        }
    }
}
