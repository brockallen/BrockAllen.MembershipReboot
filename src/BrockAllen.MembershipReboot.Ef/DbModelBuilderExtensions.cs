using BrockAllen.MembershipReboot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.Entity
{
    public static class DbModelBuilderExtensions
    {
        public static void ConfigureMembershipRebootUserAccounts<TAccount>(this DbModelBuilder modelBuilder)
            where TAccount : UserAccount
        {
            modelBuilder.Entity<TAccount>().HasKey(x => x.ID);

            modelBuilder.Entity<TAccount>().HasMany(x => x.PasswordResetSecrets).WithRequired();
            modelBuilder.Entity<PasswordResetSecret>().HasKey(x => x.PasswordResetSecretID);

            modelBuilder.Entity<TAccount>().HasMany(x => x.TwoFactorAuthTokens).WithRequired();
            modelBuilder.Entity<TwoFactorAuthToken>().HasKey(x => new { x.UserAccountID, x.Token });

            modelBuilder.Entity<TAccount>().HasMany(x => x.Certificates).WithRequired();
            modelBuilder.Entity<UserCertificate>().HasKey(x => new { x.UserAccountID, x.Thumbprint });

            modelBuilder.Entity<TAccount>().HasMany(x => x.Claims).WithRequired();
            modelBuilder.Entity<UserClaim>().HasKey(x => new { x.UserAccountID, x.Type, x.Value });

            modelBuilder.Entity<TAccount>().HasMany(x => x.LinkedAccounts).WithRequired();
            modelBuilder.Entity<LinkedAccount>().HasKey(x => new { x.UserAccountID, x.ProviderName, x.ProviderAccountID });
            
            modelBuilder.Entity<LinkedAccount>().HasMany(x => x.Claims).WithRequired();
            modelBuilder.Entity<LinkedAccountClaim>().HasKey(x => new { x.UserAccountID, x.ProviderName, x.ProviderAccountID, x.Type, x.Value });
        }

        public static void ConfigureMembershipRebootGroups<TGroup>(this DbModelBuilder modelBuilder)
            where TGroup : Group
        {
            modelBuilder.Entity<TGroup>().HasKey(x => x.ID);

            modelBuilder.Entity<TGroup>().HasMany(x => x.Children).WithRequired();
            modelBuilder.Entity<GroupChild>().HasKey(x => new { x.GroupID, x.ChildGroupID });
        }
    }
}
