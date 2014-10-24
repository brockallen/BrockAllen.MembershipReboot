using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Relational;
using BrockAllen.MembershipReboot.Ef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace CustomUserAccount
{
    public class CustomUserAccount : RelationalUserAccount
    {
        // make sure the custom properties are all virtual
        public virtual int Age { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
    }

    public class CustomDb : MembershipRebootDbContext<CustomUserAccount>
    {
        public CustomDb()
            : base("CustomDatabase")
        {
        }
    }

    public class CustomUserAccountRepository : DbContextUserAccountRepository<CustomDb, CustomUserAccount>
    {
        public CustomUserAccountRepository(CustomDb db)
            : base(db)
        {
        }
    }

    class Program
    {
        static MembershipRebootConfiguration<CustomUserAccount> config;
        static Program()
        {
            config = new MembershipRebootConfiguration<CustomUserAccount>();
            config.PasswordHashingIterationCount = 50000;
            config.MultiTenant = false;
            // etc...
        }

        static UserAccountService<CustomUserAccount> GetUserAccountService()
        {
            var repo = new CustomUserAccountRepository();
            UserAccountService<CustomUserAccount> svc = new UserAccountService<CustomUserAccount>(config, repo);
            return svc;
        }

        static void Main(string[] args)
        {
            var svc = GetUserAccountService();
            var account = svc.GetByUsername("brock");
            if (account == null)
            {
                Console.WriteLine("Creating new account");
                account = svc.CreateAccount("brock", "pass123", "brockallen@gmail.com");
                
                account.FirstName = "Brock";
                account.LastName = "Allen";
                account.Age = 21;
                svc.Update(account);
            }
            else
            {
                Console.WriteLine("Updating existing account");
                account.Age++;
                svc.Update(account);
            }
        }
    }
}
