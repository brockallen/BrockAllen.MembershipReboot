namespace BrockAllen.MembershipReboot.Ef.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v5 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                  "dbo.PasswordResetSecrets",
                  c => new
                  {
                      PasswordResetSecretID = c.Guid(nullable: false),
                      UserAccountID = c.Guid(nullable: false),
                      Question = c.String(nullable: false, maxLength: 150),
                      Answer = c.String(nullable: false, maxLength: 150),
                  })
                  .PrimaryKey(t => t.PasswordResetSecretID)
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

            DropForeignKey("dbo.LinkedAccounts", "UserAccountID", "dbo.UserAccounts");
            DropIndex("dbo.LinkedAccounts", "UserAccountID");
            DropForeignKey("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" }, "dbo.LinkedAccounts");
            DropIndex("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" });

            CreateTable(
                   "dbo.LinkedAccounts2",
                   c => new
                   {
                       UserAccountID = c.Guid(nullable: false),
                       ProviderName = c.String(nullable: false, maxLength: 30),
                       ProviderAccountID = c.String(nullable: false, maxLength: 100),
                       LastLogin = c.DateTime(nullable: false),
                   })
                   .PrimaryKey(t => new { t.UserAccountID, t.ProviderName, t.ProviderAccountID })
                   //.ForeignKey("dbo.UserAccounts", t => t.UserAccountID, cascadeDelete: true)
                   .Index(t => t.UserAccountID);
            this.Sql("insert into dbo.LinkedAccounts2 (UserAccountID, ProviderName, ProviderAccountID, LastLogin) select UserAccountID, ProviderName, ProviderAccountID, LastLogin from dbo.LinkedAccounts");
            DropTable("dbo.LinkedAccounts");
            RenameTable("LinkedAccounts2", "LinkedAccounts");
            this.Sql("exec sp_rename '[dbo].[PK_dbo.LinkedAccounts2]', 'PK_dbo.LinkedAccounts', 'OBJECT'");

            CreateTable(
                "dbo.LinkedAccountClaims2",
                c => new
                {
                    UserAccountID = c.Guid(nullable: false),
                    ProviderName = c.String(nullable: false, maxLength: 30),
                    ProviderAccountID = c.String(nullable: false, maxLength: 100),
                    Type = c.String(nullable: false, maxLength: 150),
                    Value = c.String(nullable: false, maxLength: 150),
                })
                .PrimaryKey(t => new { t.UserAccountID, t.ProviderName, t.ProviderAccountID, t.Type, t.Value })
                //.ForeignKey("dbo.UserAccounts", t => t.UserAccountID, cascadeDelete: true)
                .Index(t => t.UserAccountID);
            this.Sql("insert into dbo.LinkedAccountClaims2 (UserAccountID, ProviderName, ProviderAccountID, Type, Value) select UserAccountID, ProviderName, ProviderAccountID, Type, Value from dbo.LinkedAccountClaims");
            DropTable("dbo.LinkedAccountClaims");
            RenameTable("LinkedAccountClaims2", "LinkedAccountClaims");
            this.Sql("exec sp_rename '[dbo].[PK_dbo.LinkedAccountClaims2]', 'PK_dbo.LinkedAccountClaims', 'OBJECT'");

            AddForeignKey("dbo.LinkedAccounts", "UserAccountID", "dbo.UserAccounts", cascadeDelete:true);
            AddForeignKey("dbo.LinkedAccountClaims", "UserAccountID", "dbo.UserAccounts", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TwoFactorAuthTokens", "UserAccountID", "dbo.UserAccounts");
            DropForeignKey("dbo.PasswordResetSecrets", "UserAccountID", "dbo.UserAccounts");
            DropIndex("dbo.TwoFactorAuthTokens", new[] { "UserAccountID" });
            DropIndex("dbo.PasswordResetSecrets", new[] { "UserAccountID" });
            DropTable("dbo.TwoFactorAuthTokens");
            DropTable("dbo.PasswordResetSecrets");
            
            AlterColumn("dbo.UserAccounts", "HashedPassword", c => c.String(nullable: false, maxLength: 200));
            AlterColumn("dbo.UserAccounts", "MobilePhoneNumber", c => c.String());
            AlterColumn("dbo.UserAccounts", "MobileCode", c => c.String());
            AlterColumn("dbo.UserAccounts", "PasswordChanged", c => c.DateTime(nullable: false));
            AlterColumn("dbo.UserAccounts", "Email", c => c.String(nullable: false, maxLength: 100));
            DropColumn("dbo.UserAccounts", "VerificationStorage");
            DropColumn("dbo.UserAccounts", "MobilePhoneNumberChanged");
            DropColumn("dbo.UserAccounts", "FailedPasswordResetCount");
            DropColumn("dbo.UserAccounts", "LastFailedPasswordReset");

            DropForeignKey("dbo.LinkedAccounts", "UserAccountID", "dbo.UserAccounts");
            DropIndex("dbo.LinkedAccounts", "UserAccountID");
            DropForeignKey("dbo.LinkedAccountClaims", "UserAccountID", "dbo.UserAccounts");
            DropIndex("dbo.LinkedAccountClaims", "UserAccountID");

            CreateTable(
                "dbo.LinkedAccounts2",
                c => new
                {
                    UserAccountID = c.Guid(nullable: false),
                    ProviderName = c.String(nullable: false, maxLength: 50),
                    ProviderAccountID = c.String(nullable: false, maxLength: 100),
                    LastLogin = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => new { t.UserAccountID, t.ProviderName, t.ProviderAccountID })
                //.ForeignKey("dbo.UserAccounts", t => t.UserAccountID, cascadeDelete: true)
                .Index(t => t.UserAccountID);
            this.Sql("insert into dbo.LinkedAccounts2 (UserAccountID, ProviderName, ProviderAccountID, LastLogin) select UserAccountID, ProviderName, ProviderAccountID, LastLogin from dbo.LinkedAccounts");
            DropTable("dbo.LinkedAccounts");
            RenameTable("LinkedAccounts2", "LinkedAccounts");
            this.Sql("exec sp_rename '[dbo].[PK_dbo.LinkedAccounts2]', 'PK_dbo.LinkedAccounts', 'OBJECT'");

            CreateTable(
                "dbo.LinkedAccountClaims2",
                c => new
                {
                    UserAccountID = c.Guid(nullable: false),
                    ProviderName = c.String(nullable: false, maxLength: 50),
                    ProviderAccountID = c.String(nullable: false, maxLength: 100),
                    Type = c.String(nullable: false, maxLength: 150),
                    Value = c.String(nullable: false, maxLength: 150),
                })
                .PrimaryKey(t => new { t.UserAccountID, t.ProviderName, t.ProviderAccountID, t.Type, t.Value })
                //.ForeignKey("dbo.LinkedAccounts", t => new { t.UserAccountID, t.ProviderName, t.ProviderAccountID }, cascadeDelete: true)
                .Index(t => new { t.UserAccountID, t.ProviderName, t.ProviderAccountID });
            this.Sql("insert into dbo.LinkedAccountClaims2 (UserAccountID, ProviderName, ProviderAccountID, Type, Value) select UserAccountID, ProviderName, ProviderAccountID, Type, Value from dbo.LinkedAccountClaims");
            DropTable("dbo.LinkedAccountClaims");
            RenameTable("LinkedAccountClaims2", "LinkedAccountClaims");
            this.Sql("exec sp_rename '[dbo].[PK_dbo.LinkedAccountClaims2]', 'PK_dbo.LinkedAccountClaims', 'OBJECT'");

            AddForeignKey("dbo.LinkedAccounts", "UserAccountID", "dbo.UserAccounts", cascadeDelete: true);
            AddForeignKey("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" }, "dbo.LinkedAccounts", cascadeDelete: true);
        }
    }
}
