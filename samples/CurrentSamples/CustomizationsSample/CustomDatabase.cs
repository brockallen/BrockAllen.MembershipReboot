using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BrockAllen.MembershipReboot.Mvc
{
    public class SomeOtherEntity
    {
        public int ID { get; set; }
        public string Val1 { get; set; }
        public string Val2 { get; set; }
    }

    public class CustomDatabase : DbContext
    {
        public CustomDatabase()
            : this("name=MyDbConnectionString")
        {
        }
        
        public CustomDatabase(string connectionName)
            : base(connectionName)
        {
        }
        
        public DbSet<SomeOtherEntity> OtherStuff { get; set; }
        public DbSet<UserAccount> UserAccountsTableWithSomeOtherName { get; set; }
    }

    public class CustomRepository : DbContextUserAccountRepository<CustomDatabase>
    {
        // you can do either style ctor (or none) -- depends how much control 
        // you want over instantiating the CustomDatabase instance

        //public CustomRepository()
        //{
        //}

        //public CustomRepository()
        //    : base(new CustomDatabase())
        //{
        //}
    }
}