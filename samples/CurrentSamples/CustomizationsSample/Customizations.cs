using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BrockAllen.MembershipReboot.Mvc
{
    // this shows the extensibility point of being able to use a custom database/dbcontext
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

    // this shows the extensibility point of being notified of account activity
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

    public class NotifyAccountOwnerWhenTooManyFailedLoginAttempts
        : IEventHandler<TooManyRecentPasswordFailuresEvent>
    {
        public void Handle(TooManyRecentPasswordFailuresEvent evt)
        {
            var smtp = new SmtpMessageDelivery();
            var msg = new Message
            {
                To = evt.Account.Email,
                Subject = "Your Account",
                Body = "It seems someone has tried to login too many times to your account. It's currently locked. Blah, blah..."
            };
            smtp.Send(msg);
        }
    }

    // customize default email messages
    public class CustomEmailMessageFormatter : EmailMessageFormatter
    {
        public CustomEmailMessageFormatter(Lazy<ApplicationInformation> info)
            : base(info)
        {
        }

        protected override string GetBody(UserAccountEvent evt)
        {
            if (evt is AccountVerifiedEvent)
            {
                return "your account was verified with " + this.ApplicationInformation.ApplicationName + ". good for you.";
            }

            if (evt is AccountClosedEvent)
            {
                return FormatValue(evt, "your account was closed with {applicationName}. good riddance.");
            }

            return base.GetBody(evt);
        }
    }
}