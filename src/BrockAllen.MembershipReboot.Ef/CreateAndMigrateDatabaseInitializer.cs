using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.Ef
{
    public class CreateAndMigrateDatabaseInitializer<TContext, TConfiguration> : CreateDatabaseIfNotExists<TContext>, IDatabaseInitializer<TContext> 
    where TContext : DbContext
    where TConfiguration : DbMigrationsConfiguration<TContext>, new()
    {
        private readonly DbMigrationsConfiguration _configuration;
    
        public CreateAndMigrateDatabaseInitializer()
        {
            _configuration = new TConfiguration();
        }
        public CreateAndMigrateDatabaseInitializer(string connection)
        {
            Contract.Requires(!string.IsNullOrEmpty(connection), "connection");

            _configuration = new TConfiguration
            {
                TargetDatabase = new DbConnectionInfo(connection)
            };
        }
        void IDatabaseInitializer<TContext>.InitializeDatabase(TContext context)
        {
            Contract.Requires(context != null, "context");

            var migrator = new DbMigrator(_configuration);
            migrator.Update();

            // move on with the 'CreateDatabaseIfNotExists' for the 'Seed'
            base.InitializeDatabase(context);
        }

        protected override void Seed(TContext context)
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
