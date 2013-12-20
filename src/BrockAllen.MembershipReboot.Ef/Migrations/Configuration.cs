/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

namespace BrockAllen.MembershipReboot.Ef.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    public sealed class Configuration: DbMigrationsConfiguration<BrockAllen.MembershipReboot.Ef.DefaultMembershipRebootDatabase>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "BrockAllen.MembershipReboot.Ef.DefaultMembershipRebootDatabase";
            MigrationsDirectory = @"Migrations";
            MigrationsNamespace = "BrockAllen.MembershipReboot.Ef.Migrations";
        }

      
    }



    public sealed class ConfigurationAzure : DbMigrationsConfiguration<BrockAllen.MembershipReboot.Ef.DefaultMembershipRebootDatabase>
    {
        public ConfigurationAzure()
        {
            AutomaticMigrationsEnabled = true;
            //to change
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "BrockAllen.MembershipReboot.Ef.SqlAzureMembershipRebootDatabase";
            MigrationsDirectory = @"Migrations\SqlAzure";
            MigrationsNamespace = "BrockAllen.MembershipReboot.Ef.Migrations.SqlAzure";
        }

       


    }




}
