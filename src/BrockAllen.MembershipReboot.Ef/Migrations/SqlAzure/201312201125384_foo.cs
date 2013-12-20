namespace BrockAllen.MembershipReboot.Ef.Migrations.SqlAzure
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class foo : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserCertificates", "UserAccountID", "dbo.UserAccounts");
            DropForeignKey("dbo.UserClaims", "UserAccountID", "dbo.UserAccounts");
            DropForeignKey("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" }, "dbo.LinkedAccounts");
            DropForeignKey("dbo.LinkedAccounts", "UserAccountID", "dbo.UserAccounts");
            DropForeignKey("dbo.PasswordResetSecrets", "UserAccountID", "dbo.UserAccounts");
            DropForeignKey("dbo.TwoFactorAuthTokens", "UserAccountID", "dbo.UserAccounts");
            DropIndex("dbo.UserCertificates", new[] { "UserAccountID" });
            DropIndex("dbo.UserClaims", new[] { "UserAccountID" });
            DropIndex("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" });
            DropIndex("dbo.LinkedAccounts", new[] { "UserAccountID" });
            DropIndex("dbo.PasswordResetSecrets", new[] { "UserAccountID" });
            DropIndex("dbo.TwoFactorAuthTokens", new[] { "UserAccountID" });
            CreateIndex("dbo.UserClaims", "UserAccountID");
            CreateIndex("dbo.LinkedAccountClaims", "UserAccountID");
            CreateIndex("dbo.LinkedAccounts", "UserAccountID");
            CreateIndex("dbo.PasswordResetSecrets", "UserAccountID");
            CreateIndex("dbo.TwoFactorAuthTokens", "UserAccountID");
            CreateIndex("dbo.UserCertificates", "UserAccountID");
            AddForeignKey("dbo.UserClaims", "UserAccountID", "dbo.UserAccounts", "ID", cascadeDelete: true);
            AddForeignKey("dbo.LinkedAccountClaims", "UserAccountID", "dbo.UserAccounts", "ID", cascadeDelete: true);
            AddForeignKey("dbo.LinkedAccounts", "UserAccountID", "dbo.UserAccounts", "ID", cascadeDelete: true);
            AddForeignKey("dbo.PasswordResetSecrets", "UserAccountID", "dbo.UserAccounts", "ID", cascadeDelete: true);
            AddForeignKey("dbo.TwoFactorAuthTokens", "UserAccountID", "dbo.UserAccounts", "ID", cascadeDelete: true);
            AddForeignKey("dbo.UserCertificates", "UserAccountID", "dbo.UserAccounts", "ID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserCertificates", "UserAccountID", "dbo.UserAccounts");
            DropForeignKey("dbo.TwoFactorAuthTokens", "UserAccountID", "dbo.UserAccounts");
            DropForeignKey("dbo.PasswordResetSecrets", "UserAccountID", "dbo.UserAccounts");
            DropForeignKey("dbo.LinkedAccounts", "UserAccountID", "dbo.UserAccounts");
            DropForeignKey("dbo.LinkedAccountClaims", "UserAccountID", "dbo.UserAccounts");
            DropForeignKey("dbo.UserClaims", "UserAccountID", "dbo.UserAccounts");
            DropIndex("dbo.UserCertificates", new[] { "UserAccountID" });
            DropIndex("dbo.TwoFactorAuthTokens", new[] { "UserAccountID" });
            DropIndex("dbo.PasswordResetSecrets", new[] { "UserAccountID" });
            DropIndex("dbo.LinkedAccounts", new[] { "UserAccountID" });
            DropIndex("dbo.LinkedAccountClaims", new[] { "UserAccountID" });
            DropIndex("dbo.UserClaims", new[] { "UserAccountID" });
            CreateIndex("dbo.TwoFactorAuthTokens", "UserAccountID");
            CreateIndex("dbo.PasswordResetSecrets", "UserAccountID");
            CreateIndex("dbo.LinkedAccounts", "UserAccountID");
            CreateIndex("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" });
            CreateIndex("dbo.UserClaims", "UserAccountID");
            CreateIndex("dbo.UserCertificates", "UserAccountID");
            AddForeignKey("dbo.TwoFactorAuthTokens", "UserAccountID", "dbo.UserAccounts", "ID", cascadeDelete: true);
            AddForeignKey("dbo.PasswordResetSecrets", "UserAccountID", "dbo.UserAccounts", "ID", cascadeDelete: true);
            AddForeignKey("dbo.LinkedAccounts", "UserAccountID", "dbo.UserAccounts", "ID", cascadeDelete: true);
            AddForeignKey("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" }, "dbo.LinkedAccounts", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" }, cascadeDelete: true);
            AddForeignKey("dbo.UserClaims", "UserAccountID", "dbo.UserAccounts", "ID", cascadeDelete: true);
            AddForeignKey("dbo.UserCertificates", "UserAccountID", "dbo.UserAccounts", "ID", cascadeDelete: true);
        }
    }
}
