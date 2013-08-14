/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Raven.Client;
using Raven.Client.Document;

namespace BrockAllen.MembershipReboot.RavenDb
{
    public class DefaultMembershipRebootDatabase
    {
        public IDocumentStore DocumentStore { get; private set; }

        public DefaultMembershipRebootDatabase(string connectionStringName)
        {
            DocumentStore = new DocumentStore
                {
                    ConnectionStringName = connectionStringName
                }.Initialize();
        }

        public DefaultMembershipRebootDatabase(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
        }
    }
}
