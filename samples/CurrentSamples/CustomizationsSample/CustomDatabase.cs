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

    public class AuthenticationAudit
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public string Activity { get; set; }
        public string Detail { get; set; }
        public string ClientIP { get; set; }
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
        public DbSet<AuthenticationAudit> Audits { get; set; }
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

    public class AuthenticationAuditEventHandler :
        IEventHandler<SuccessfulLoginEvent>,
        IEventHandler<FailedLoginEvent>
    {
        public void Handle(SuccessfulLoginEvent evt)
        {
            using (var db = new CustomDatabase())
            {
                var audit = new AuthenticationAudit
                {
                    Date = DateTime.UtcNow,
                    Activity = "Login Success",
                    Detail = null,
                    ClientIP = HttpContext.Current.Request.UserHostAddress,
                };
                db.Audits.Add(audit);
                db.SaveChanges();
            }
        }

        public void Handle(FailedLoginEvent evt)
        {
            using (var db = new CustomDatabase())
            {
                var audit = new AuthenticationAudit
                {
                    Date = DateTime.UtcNow,
                    Activity = "Login Failure",
                    Detail = evt.GetType().Name + ", Failed Login Count: " + evt.Account.FailedLoginCount,
                    ClientIP = HttpContext.Current.Request.UserHostAddress,
                };
                db.Audits.Add(audit);
                db.SaveChanges();
            }
        }
    }
}