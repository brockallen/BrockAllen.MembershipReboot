/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Raven.Client;
using Raven.Client.Document;

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
        }

        public RavenMembershipRebootDatabase(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
        }
    }
}
