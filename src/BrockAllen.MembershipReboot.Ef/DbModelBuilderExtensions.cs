using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Relational;
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
            where TAccount : RelationalUserAccount
        {
            modelBuilder.Entity<TAccount>()
                .HasKey(x => x.ID).ToTable("UserAccounts");

            modelBuilder.Entity<TAccount>()
                .HasMany(x => x.PasswordResetSecretCollection).WithRequired().HasForeignKey(x=>x.UserAccountID);
            modelBuilder.Entity<RelationalPasswordResetSecret>()
                .HasKey(x => new { x.UserAccountID, x.PasswordResetSecretID }).ToTable("PasswordResetSecrets");

            modelBuilder.Entity<TAccount>()
                .HasMany(x => x.TwoFactorAuthTokenCollection).WithRequired().HasForeignKey(x=>x.UserAccountID);
            modelBuilder.Entity<RelationalTwoFactorAuthToken>()
                .HasKey(x => new { x.UserAccountID, x.Token }).ToTable("TwoFactorAuthTokens");

            modelBuilder.Entity<TAccount>()
                .HasMany(x => x.UserCertificateCollection).WithRequired().HasForeignKey(x=>x.UserAccountID);
            modelBuilder.Entity<RelationalUserCertificate>()
                .HasKey(x => new { x.UserAccountID, x.Thumbprint }).ToTable("UserCertificates");

            modelBuilder.Entity<TAccount>()
                .HasMany(x => x.ClaimCollection).WithRequired().HasForeignKey(x=>x.UserAccountID);
            modelBuilder.Entity<RelationalUserClaim>()
                .HasKey(x => new { x.UserAccountID, x.Type, x.Value }).ToTable("UserClaims");

            modelBuilder.Entity<TAccount>()
                .HasMany(x => x.LinkedAccountCollection).WithRequired().HasForeignKey(x=>x.UserAccountID);
            modelBuilder.Entity<RelationalLinkedAccount>()
                .HasKey(x => new { x.UserAccountID, x.ProviderName, x.ProviderAccountID }).ToTable("LinkedAccounts");

            modelBuilder.Entity<TAccount>()
                .HasMany(x => x.LinkedAccountClaimCollection).WithRequired().HasForeignKey(x => x.UserAccountID);
            modelBuilder.Entity<RelationalLinkedAccountClaim>()
                .HasKey(x => new { x.UserAccountID, x.ProviderName, x.Type, x.Value }).ToTable("LinkedAccountClaims");
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
