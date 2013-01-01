namespace BrockAllen.MembershipReboot.Migrations.SqlServer
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class SqlServerConfig : DbMigrationsConfiguration<BrockAllen.MembershipReboot.EFMembershipRebootDatabase>
    {
        public SqlServerConfig()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(BrockAllen.MembershipReboot.EFMembershipRebootDatabase context)
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
