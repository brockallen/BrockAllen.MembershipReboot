namespace BrockAllen.MembershipReboot.Migrations.SqlCe
{
    using System.Data.Entity.Migrations;

    internal sealed class SqlCeConfig : DbMigrationsConfiguration<BrockAllen.MembershipReboot.EFMembershipRebootDatabase>
    {
        public SqlCeConfig()
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
