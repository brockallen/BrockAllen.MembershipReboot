namespace BrockAllen.MembershipReboot.Ef.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;


    public sealed class Configuration : DbMigrationsConfiguration<BrockAllen.MembershipReboot.Ef.DefaultMembershipRebootDatabase>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "BrockAllen.MembershipReboot.Ef.DefaultMembershipRebootDatabase";
        }


        protected override void Seed(BrockAllen.MembershipReboot.Ef.DefaultMembershipRebootDatabase context)
        {
            //  This method will be called after migrating to the latest version.


            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
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

        protected override void Seed(BrockAllen.MembershipReboot.Ef.DefaultMembershipRebootDatabase context)
        {
            //  This method will be called after migrating to the latest version.


            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }

}
