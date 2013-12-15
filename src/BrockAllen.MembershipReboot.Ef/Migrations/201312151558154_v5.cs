namespace BrockAllen.MembershipReboot.Ef.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v5 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserCertificates", "UserAccountID", "dbo.UserAccounts");
            DropForeignKey("dbo.UserClaims", "UserAccountID", "dbo.UserAccounts");
            DropForeignKey("dbo.LinkedAccounts", "UserAccountID", "dbo.UserAccounts");
            DropForeignKey("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" }, "dbo.LinkedAccounts");
            
            DropIndex("dbo.UserCertificates", new[] { "UserAccountID" });
            DropIndex("dbo.UserClaims", new[] { "UserAccountID" });
            DropIndex("dbo.LinkedAccounts", new[] { "UserAccountID" });
            DropIndex("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" });
            
            CreateTable(
                "dbo.PasswordResetSecrets",
                c => new
                    {
                        PasswordResetSecretID = c.Guid(nullable: false),
                        Question = c.String(nullable: false, maxLength: 150),
                        Answer = c.String(nullable: false, maxLength: 150),
                        UserAccount_ID = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.PasswordResetSecretID)
                .ForeignKey("dbo.UserAccounts", t => t.UserAccount_ID, cascadeDelete: true)
                .Index(t => t.UserAccount_ID);
            
            CreateTable(
                "dbo.TwoFactorAuthTokens",
                c => new
                    {
                        Token = c.String(nullable: false, maxLength: 100),
                        Issued = c.DateTime(nullable: false),
                        UserAccount_ID = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Token)
                .ForeignKey("dbo.UserAccounts", t => t.UserAccount_ID, cascadeDelete: true)
                .Index(t => t.UserAccount_ID);

            DropPrimaryKey("dbo.LinkedAccounts");
            DropPrimaryKey("dbo.UserCertificates");
            DropPrimaryKey("dbo.UserClaims");
            DropPrimaryKey("dbo.LinkedAccountClaims");

            AddColumn("dbo.UserAccounts", "LastFailedPasswordReset", c => c.DateTime());
            AddColumn("dbo.UserAccounts", "FailedPasswordResetCount", c => c.Int(nullable: false));
            AddColumn("dbo.UserAccounts", "MobilePhoneNumberChanged", c => c.DateTime());
            AddColumn("dbo.UserAccounts", "VerificationStorage", c => c.String(maxLength: 100));

            RenameColumn("dbo.UserCertificates", "UserAccountID", "UserAccount_ID");
            //AddColumn("dbo.UserCertificates", "UserAccount_ID", c => c.Guid(nullable: false));
            RenameColumn("dbo.UserClaims", "UserAccountID", "UserAccount_ID");
            //AddColumn("dbo.UserClaims", "UserAccount_ID", c => c.Guid(nullable: false));
            RenameColumn("dbo.LinkedAccounts", "UserAccountID", "UserAccount_ID");
            //AddColumn("dbo.LinkedAccounts", "UserAccount_ID", c => c.Guid(nullable: false));

            AlterColumn("dbo.LinkedAccountClaims", "ProviderName", c => c.String(nullable: false, maxLength: 30)); 
            RenameColumn("dbo.LinkedAccountClaims", "ProviderName", "LinkedAccount_ProviderName");
            //AddColumn("dbo.LinkedAccountClaims", "LinkedAccount_ProviderName", c => c.String(nullable: false, maxLength: 30));
            RenameColumn("dbo.LinkedAccountClaims", "ProviderAccountID", "LinkedAccount_ProviderAccountID");
            //AddColumn("dbo.LinkedAccountClaims", "LinkedAccount_ProviderAccountID", c => c.String(nullable: false, maxLength: 100));
            
            AlterColumn("dbo.UserAccounts", "Email", c => c.String(maxLength: 100));
            AlterColumn("dbo.UserAccounts", "PasswordChanged", c => c.DateTime());
            AlterColumn("dbo.UserAccounts", "MobileCode", c => c.String(maxLength: 100));
            AlterColumn("dbo.UserAccounts", "MobilePhoneNumber", c => c.String(maxLength: 20));
            AlterColumn("dbo.UserAccounts", "HashedPassword", c => c.String(maxLength: 200));
            AlterColumn("dbo.LinkedAccounts", "ProviderName", c => c.String(nullable: false, maxLength: 30));
            
            AddPrimaryKey("dbo.UserCertificates", "Thumbprint");
            AddPrimaryKey("dbo.UserClaims", new[] { "Type", "Value" });
            AddPrimaryKey("dbo.LinkedAccounts", new[] { "ProviderName", "ProviderAccountID" });
            AddPrimaryKey("dbo.LinkedAccountClaims", new[] { "Type", "Value" });
            
            CreateIndex("dbo.UserCertificates", "UserAccount_ID");
            CreateIndex("dbo.UserClaims", "UserAccount_ID");
            CreateIndex("dbo.LinkedAccounts", "UserAccount_ID");
            CreateIndex("dbo.LinkedAccountClaims", new[] { "LinkedAccount_ProviderName", "LinkedAccount_ProviderAccountID" });
            
            AddForeignKey("dbo.UserCertificates", "UserAccount_ID", "dbo.UserAccounts", "ID", cascadeDelete: true);
            AddForeignKey("dbo.UserClaims", "UserAccount_ID", "dbo.UserAccounts", "ID", cascadeDelete: true);
            AddForeignKey("dbo.LinkedAccounts", "UserAccount_ID", "dbo.UserAccounts", "ID", cascadeDelete: true);
            AddForeignKey("dbo.LinkedAccountClaims", new[] { "LinkedAccount_ProviderName", "LinkedAccount_ProviderAccountID" }, "dbo.LinkedAccounts", new[] { "ProviderName", "ProviderAccountID" }, cascadeDelete: true);

            DropColumn("dbo.LinkedAccountClaims", "UserAccountID");
            //DropColumn("dbo.UserCertificates", "UserAccountID");
            //DropColumn("dbo.UserClaims", "UserAccountID");
            //DropColumn("dbo.LinkedAccounts", "UserAccountID");
            //DropColumn("dbo.LinkedAccountClaims", "ProviderName");
            //DropColumn("dbo.LinkedAccountClaims", "ProviderAccountID");        
        }
        
        public override void Down()
        {
            AddColumn("dbo.LinkedAccountClaims", "ProviderAccountID", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.LinkedAccountClaims", "ProviderName", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.LinkedAccountClaims", "UserAccountID", c => c.Guid(nullable: false));
            AddColumn("dbo.LinkedAccounts", "UserAccountID", c => c.Guid(nullable: false));
            AddColumn("dbo.UserClaims", "UserAccountID", c => c.Guid(nullable: false));
            AddColumn("dbo.UserCertificates", "UserAccountID", c => c.Guid(nullable: false));
            DropForeignKey("dbo.LinkedAccountClaims", new[] { "LinkedAccount_ProviderName", "LinkedAccount_ProviderAccountID" }, "dbo.LinkedAccounts");
            DropForeignKey("dbo.LinkedAccounts", "UserAccount_ID", "dbo.UserAccounts");
            DropForeignKey("dbo.UserClaims", "UserAccount_ID", "dbo.UserAccounts");
            DropForeignKey("dbo.UserCertificates", "UserAccount_ID", "dbo.UserAccounts");
            DropForeignKey("dbo.TwoFactorAuthTokens", "UserAccount_ID", "dbo.UserAccounts");
            DropForeignKey("dbo.PasswordResetSecrets", "UserAccount_ID", "dbo.UserAccounts");
            DropIndex("dbo.LinkedAccountClaims", new[] { "LinkedAccount_ProviderName", "LinkedAccount_ProviderAccountID" });
            DropIndex("dbo.LinkedAccounts", new[] { "UserAccount_ID" });
            DropIndex("dbo.UserClaims", new[] { "UserAccount_ID" });
            DropIndex("dbo.UserCertificates", new[] { "UserAccount_ID" });
            DropIndex("dbo.TwoFactorAuthTokens", new[] { "UserAccount_ID" });
            DropIndex("dbo.PasswordResetSecrets", new[] { "UserAccount_ID" });
            DropPrimaryKey("dbo.LinkedAccountClaims");
            AddPrimaryKey("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID", "Type", "Value" });
            DropPrimaryKey("dbo.LinkedAccounts");
            AddPrimaryKey("dbo.LinkedAccounts", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" });
            DropPrimaryKey("dbo.UserClaims");
            AddPrimaryKey("dbo.UserClaims", new[] { "UserAccountID", "Type", "Value" });
            DropPrimaryKey("dbo.UserCertificates");
            AddPrimaryKey("dbo.UserCertificates", new[] { "UserAccountID", "Thumbprint" });
            AlterColumn("dbo.LinkedAccounts", "ProviderName", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.UserAccounts", "HashedPassword", c => c.String(nullable: false, maxLength: 200));
            AlterColumn("dbo.UserAccounts", "MobilePhoneNumber", c => c.String());
            AlterColumn("dbo.UserAccounts", "MobileCode", c => c.String());
            AlterColumn("dbo.UserAccounts", "PasswordChanged", c => c.DateTime(nullable: false));
            AlterColumn("dbo.UserAccounts", "Email", c => c.String(nullable: false, maxLength: 100));
            DropColumn("dbo.LinkedAccountClaims", "LinkedAccount_ProviderAccountID");
            DropColumn("dbo.LinkedAccountClaims", "LinkedAccount_ProviderName");
            DropColumn("dbo.LinkedAccounts", "UserAccount_ID");
            DropColumn("dbo.UserClaims", "UserAccount_ID");
            DropColumn("dbo.UserCertificates", "UserAccount_ID");
            DropColumn("dbo.UserAccounts", "VerificationStorage");
            DropColumn("dbo.UserAccounts", "MobilePhoneNumberChanged");
            DropColumn("dbo.UserAccounts", "FailedPasswordResetCount");
            DropColumn("dbo.UserAccounts", "LastFailedPasswordReset");
            DropTable("dbo.TwoFactorAuthTokens");
            DropTable("dbo.PasswordResetSecrets");
            CreateIndex("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" });
            CreateIndex("dbo.LinkedAccounts", "UserAccountID");
            CreateIndex("dbo.UserClaims", "UserAccountID");
            CreateIndex("dbo.UserCertificates", "UserAccountID");
            AddForeignKey("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" }, "dbo.LinkedAccounts", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" }, cascadeDelete: true);
            AddForeignKey("dbo.LinkedAccounts", "UserAccountID", "dbo.UserAccounts", "ID", cascadeDelete: true);
            AddForeignKey("dbo.UserClaims", "UserAccountID", "dbo.UserAccounts", "ID", cascadeDelete: true);
            AddForeignKey("dbo.UserCertificates", "UserAccountID", "dbo.UserAccounts", "ID", cascadeDelete: true);
        }
    }
}
