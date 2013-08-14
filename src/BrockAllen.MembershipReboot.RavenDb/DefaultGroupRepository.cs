/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


using Raven.Client;
using Raven.Client.Document;

namespace BrockAllen.MembershipReboot.RavenDb
{
    public class DefaultGroupRepository
        : RavenDbGroupRepository<DocumentStore>
    {
        public DefaultGroupRepository(string connectionStringName)
            : base((DocumentStore) new DefaultMembershipRebootDatabase(connectionStringName).DocumentStore)
        {
        }

        public DefaultGroupRepository(IDocumentStore documentStore)
            : base((DocumentStore) new DefaultMembershipRebootDatabase(documentStore).DocumentStore)
        {
        }
    }
}
