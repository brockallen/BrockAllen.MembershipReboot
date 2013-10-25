/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


using Raven.Client;
using Raven.Client.Document;

namespace BrockAllen.MembershipReboot.RavenDb
{
    public class RavenGroupRepository
        : RavenDbGroupRepository<DocumentStore>
    {
        public RavenGroupRepository(string connectionStringName)
            : base((DocumentStore) new RavenMembershipRebootDatabase(connectionStringName).DocumentStore)
        {
        }

        public RavenGroupRepository(IDocumentStore documentStore)
            : base((DocumentStore) new RavenMembershipRebootDatabase(documentStore).DocumentStore)
        {
        }
    }
}
