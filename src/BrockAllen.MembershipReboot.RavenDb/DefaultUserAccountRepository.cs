/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


using Raven.Client;
using Raven.Client.Document;

namespace BrockAllen.MembershipReboot.RavenDb
{
    public class DefaultUserAccountRepository
        : RavenDbUserAccountRepository<DocumentStore>
    {
        public DefaultUserAccountRepository(string connectionStringName)
            : base((DocumentStore) new DefaultMembershipRebootDatabase(connectionStringName).DocumentStore)
        {
        }

        public DefaultUserAccountRepository(IDocumentStore documentStore)
            : base((DocumentStore) new DefaultMembershipRebootDatabase(documentStore).DocumentStore)
        {
        }
    }
}
