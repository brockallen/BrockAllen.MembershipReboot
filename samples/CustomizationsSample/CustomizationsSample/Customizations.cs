using BrockAllen.MembershipReboot.Ef;
using System;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations.Schema;
using BrockAllen.MembershipReboot.Relational;

namespace BrockAllen.MembershipReboot.Mvc
{
    // shows custom validation
    public class PasswordValidator : IValidator<CustomUserAccount>
    {
        public ValidationResult Validate(UserAccountService<CustomUserAccount> service, CustomUserAccount account, string value)
        {
            if (value.Contains("R"))
            {
                return new ValidationResult("You can't use an 'R' in your password (for some reason)");
            }

            return null;
        }
    }

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

    public class PasswordHistory
    {
        public int ID { get; set; }
        public Guid UserID { get; set; }
        public DateTime DateChanged { get; set; }
        [Required]
        public string PasswordHash { get; set; }
    }

    public class CustomUserAccount : RelationalUserAccount
    {
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        
        [NotMapped]
        public string OtherFirstName
        {
            get
            {
                return this.GetClaimValue(ClaimTypes.GivenName);
            }
        }
    }

    public class CustomDatabase : DbContext
    {
        static CustomDatabase()
        {
            Database.SetInitializer<CustomDatabase>(new DropCreateDatabaseIfModelChanges<CustomDatabase>());
        }

        public CustomDatabase()
            : this("name=MyDbConnectionString")
        {
            this.RegisterUserAccountChildTablesForDelete<CustomUserAccount>();
        }
        
        public CustomDatabase(string connectionName)
            : base(connectionName)
        {
            this.RegisterUserAccountChildTablesForDelete<CustomUserAccount>();
        }
        
        public DbSet<SomeOtherEntity> OtherStuff { get; set; }
        public DbSet<CustomUserAccount> UserAccountsTableWithSomeOtherName { get; set; }
        public DbSet<AuthenticationAudit> Audits { get; set; }
        public DbSet<PasswordHistory> PasswordHistory { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureMembershipRebootUserAccounts<CustomUserAccount>();
        }
    }

    public class CustomRepository : DbContextUserAccountRepository<CustomDatabase, CustomUserAccount>, IUserAccountRepository<CustomUserAccount>
    {
        // you can do either style ctor (or none) -- depends how much control 
        // you want over instantiating the CustomDatabase instance
        public CustomRepository(CustomDatabase ctx)
            : base(ctx)
        {
        }

        protected override IQueryable<CustomUserAccount> DefaultQueryFilter(IQueryable<CustomUserAccount> query, string filter)
        {
            if (query == null) throw new ArgumentNullException("query");
            if (filter == null) throw new ArgumentNullException("filter");

            return
                from a in query
                from c in a.ClaimCollection
                    where c.Value.Contains(filter)
                select a;
        }
    }

    // this shows the extensibility point of being notified of account activity
    public class AuthenticationAuditEventHandler :
        IEventHandler<SuccessfulLoginEvent<CustomUserAccount>>,
        IEventHandler<FailedLoginEvent<CustomUserAccount>>
    {
        public void Handle(SuccessfulLoginEvent<CustomUserAccount> evt)
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

        public void Handle(FailedLoginEvent<CustomUserAccount> evt)
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
        : IEventHandler<TooManyRecentPasswordFailuresEvent<CustomUserAccount>>
    {
        public void Handle(TooManyRecentPasswordFailuresEvent<CustomUserAccount> evt)
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

    public class PasswordChanging :
        IEventHandler<PasswordChangedEvent<CustomUserAccount>>
    {
        public void Handle(PasswordChangedEvent<CustomUserAccount> evt)
        {
            using (var db = new CustomDatabase())
            {
                var oldEntires =
                    db.PasswordHistory.Where(x => x.UserID == evt.Account.ID).OrderByDescending(x => x.DateChanged).ToArray();
                for (var i = 0; i < 3 && oldEntires.Length > i; i++)
                {
                    var oldHash = oldEntires[i].PasswordHash;
                    if (new DefaultCrypto().VerifyHashedPassword(oldHash, evt.NewPassword))
                    {
                        throw new ValidationException("New Password must not be same as the past three");
                    }
                }
            }
        }
    }

    public class PasswordChanged :
        IEventHandler<AccountCreatedEvent<CustomUserAccount>>,
        IEventHandler<PasswordChangedEvent<CustomUserAccount>>
    {
        public void Handle(AccountCreatedEvent<CustomUserAccount> evt)
        {
            if (evt.InitialPassword != null)
            {
                AddPasswordHistoryEntry(evt.Account.ID, evt.InitialPassword);
            }
        }
       
        public void Handle(PasswordChangedEvent<CustomUserAccount> evt)
        {
            AddPasswordHistoryEntry(evt.Account.ID, evt.NewPassword);
        }

        private static void AddPasswordHistoryEntry(Guid accountID, string password)
        {
            using (var db = new CustomDatabase())
            {
                var pw = new PasswordHistory
                {
                    UserID = accountID,
                    DateChanged = DateTime.UtcNow,
                    PasswordHash = new DefaultCrypto().HashPassword(password, 1000)
                };
                db.PasswordHistory.Add(pw);
                db.SaveChanges();
            }
        }
    }

    // customize default email messages
    public class CustomEmailMessageFormatter : EmailMessageFormatter<CustomUserAccount>
    {
        public CustomEmailMessageFormatter(ApplicationInformation info)
            : base(info)
        {
        }

        protected override string GetBody(UserAccountEvent<CustomUserAccount> evt, IDictionary<string, string> values)
        {
            if (evt is EmailVerifiedEvent<CustomUserAccount>)
            {
                return "your account was verified with " + this.ApplicationInformation.ApplicationName + ". good for you.";
            }

            if (evt is AccountClosedEvent<CustomUserAccount>)
            {
                return FormatValue(evt, "your account was closed with {applicationName}. good riddance.", values);
            }

            return base.GetBody(evt, values);
        }
    }

    public class CustomClaimsMapper : ICommandHandler<MapClaimsFromAccount<CustomUserAccount>>
    {
        public void Handle(MapClaimsFromAccount<CustomUserAccount> cmd)
        {
            cmd.MappedClaims = new System.Security.Claims.Claim[]
            {
                new System.Security.Claims.Claim(ClaimTypes.GivenName, cmd.Account.FirstName),
                new System.Security.Claims.Claim(ClaimTypes.Surname, cmd.Account.LastName),
            };
        }
    }


    public class CustomValidationMessages : ICommandHandler<GetValidationMessage>
    {
        public void Handle(GetValidationMessage cmd)
        {
            if (cmd.ID == MembershipRebootConstants.ValidationMessages.UsernameRequired)
            {
                cmd.Message = "username required, duh!";
            }
        }
    }

}