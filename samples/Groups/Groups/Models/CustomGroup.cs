using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Ef;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace RolesAdmin.Models
{
    public class CustomGroup : RelationalGroup
    {
        public string Description { get; set; }
    }

    public class CustomGroupDbContext : DbContext
    {
        public CustomGroupDbContext ()
            : base("CustomGroup")
	    {
	    }

        public DbSet<CustomGroup> Groups { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ConfigureMembershipRebootGroups<CustomGroup>();
        }
    }

    public class CustomGroupRepository : DbContextGroupRepository<CustomGroupDbContext, CustomGroup>
    {
        public CustomGroupRepository(CustomGroupDbContext db)
            : base(db)
        {

        }
    }

    public static class CustomGroupTest
    {
        public static void Test()
        {
            using (var db = new CustomGroupDbContext())
            {
                var repo = new CustomGroupRepository(db);
                var grpSvc = new GroupService<CustomGroup>("tenant", repo);
                var g = grpSvc.Create("Foo");
                g.Description = "Bar";
                grpSvc.Update(g);
            }
        }
    }
}