namespace BrockAllen.MembershipReboot.Ef.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v5 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" }, "dbo.LinkedAccounts");
            DropForeignKey("dbo.LinkedAccounts", "UserAccountID", "dbo.UserAccounts");

            DropIndex("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" });
            DropIndex("dbo.LinkedAccounts", "UserAccountID");

            DropPrimaryKey("dbo.LinkedAccountClaims");
            DropPrimaryKey("dbo.LinkedAccounts"); 
            
            CreateTable(
                "dbo.PasswordResetSecrets",
                c => new
                    {
                        PasswordResetSecretID = c.Guid(nullable: false),
                        UserAccountID = c.Guid(nullable: false),
                        Question = c.String(nullable: false, maxLength: 150),
                        Answer = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => new { t.PasswordResetSecretID, t.UserAccountID })
                .ForeignKey("dbo.UserAccounts", t => t.UserAccountID, cascadeDelete: true)
                .Index(t => t.UserAccountID);
            
            CreateTable(
                "dbo.TwoFactorAuthTokens",
                c => new
                    {
                        UserAccountID = c.Guid(nullable: false),
                        Token = c.String(nullable: false, maxLength: 100),
                        Issued = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserAccountID, t.Token })
                .ForeignKey("dbo.UserAccounts", t => t.UserAccountID, cascadeDelete: true)
                .Index(t => t.UserAccountID);
            
            AddColumn("dbo.UserAccounts", "LastFailedPasswordReset", c => c.DateTime());
            AddColumn("dbo.UserAccounts", "FailedPasswordResetCount", c => c.Int(nullable: false));
            AddColumn("dbo.UserAccounts", "MobilePhoneNumberChanged", c => c.DateTime());
            AddColumn("dbo.UserAccounts", "VerificationStorage", c => c.String(maxLength: 100));
            AlterColumn("dbo.UserAccounts", "Email", c => c.String(maxLength: 100));
            AlterColumn("dbo.UserAccounts", "PasswordChanged", c => c.DateTime());
            AlterColumn("dbo.UserAccounts", "MobileCode", c => c.String(maxLength: 100));
            AlterColumn("dbo.UserAccounts", "MobilePhoneNumber", c => c.String(maxLength: 20));
            AlterColumn("dbo.UserAccounts", "HashedPassword", c => c.String(maxLength: 200));
            AlterColumn("dbo.LinkedAccounts", "ProviderName", c => c.String(nullable: false, maxLength: 30));
            AlterColumn("dbo.LinkedAccountClaims", "ProviderName", c => c.String(nullable: false, maxLength: 30));




            AddPrimaryKey("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID", "Type", "Value" });
            AddPrimaryKey("dbo.LinkedAccounts", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" });

            CreateIndex("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" });
            // for some reason this index gets auto-added and so this line isn't needed (and fails)
            //CreateIndex("dbo.LinkedAccounts", "UserAccountID");

            AddForeignKey("dbo.LinkedAccounts", "UserAccountID", "dbo.UserAccounts", cascadeDelete: true);
            AddForeignKey("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" }, "dbo.LinkedAccounts", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" }, "dbo.LinkedAccounts");
            DropForeignKey("dbo.LinkedAccounts", "UserAccountID", "dbo.UserAccounts");

            DropIndex("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" });
            DropIndex("dbo.LinkedAccounts", "UserAccountID");

            DropPrimaryKey("dbo.LinkedAccountClaims");
            DropPrimaryKey("dbo.LinkedAccounts");

            
            DropForeignKey("dbo.TwoFactorAuthTokens", "UserAccountID", "dbo.UserAccounts");
            DropForeignKey("dbo.PasswordResetSecrets", "UserAccountID", "dbo.UserAccounts");
            DropIndex("dbo.TwoFactorAuthTokens", new[] { "UserAccountID" });
            DropIndex("dbo.PasswordResetSecrets", new[] { "UserAccountID" });
            AlterColumn("dbo.LinkedAccountClaims", "ProviderName", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.LinkedAccounts", "ProviderName", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.UserAccounts", "HashedPassword", c => c.String(nullable: false, maxLength: 200));
            AlterColumn("dbo.UserAccounts", "MobilePhoneNumber", c => c.String());
            AlterColumn("dbo.UserAccounts", "MobileCode", c => c.String());
            AlterColumn("dbo.UserAccounts", "PasswordChanged", c => c.DateTime(nullable: false));
            AlterColumn("dbo.UserAccounts", "Email", c => c.String(nullable: false, maxLength: 100));
            DropColumn("dbo.UserAccounts", "VerificationStorage");
            DropColumn("dbo.UserAccounts", "MobilePhoneNumberChanged");
            DropColumn("dbo.UserAccounts", "FailedPasswordResetCount");
            DropColumn("dbo.UserAccounts", "LastFailedPasswordReset");
            DropTable("dbo.TwoFactorAuthTokens");
            DropTable("dbo.PasswordResetSecrets");



            AddPrimaryKey("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID", "Type", "Value" });
            AddPrimaryKey("dbo.LinkedAccounts", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" });

            CreateIndex("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" });
            // same as Up 
            // CreateIndex("dbo.LinkedAccounts", "UserAccountID");

            AddForeignKey("dbo.LinkedAccounts", "UserAccountID", "dbo.UserAccounts", cascadeDelete: true);
            AddForeignKey("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" }, "dbo.LinkedAccounts", cascadeDelete: true);
        }
    }
}
