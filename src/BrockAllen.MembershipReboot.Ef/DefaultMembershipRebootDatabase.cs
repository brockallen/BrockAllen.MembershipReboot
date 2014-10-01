/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using BrockAllen.MembershipReboot.Relational;
using System.Data.Entity;

namespace BrockAllen.MembershipReboot.Ef
{
    public class DefaultMembershipRebootDatabase : MembershipRebootDbContext<RelationalUserAccount>
    {
        public DefaultMembershipRebootDatabase()
            : base()
        {
        }

        public DefaultMembershipRebootDatabase(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public DefaultMembershipRebootDatabase(string nameOrConnectionString, string schemaName)
            : base(nameOrConnectionString, schemaName)
        {
        }
    }
}
