namespace BrockAllen.MembershipReboot.Ef.Migrations
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
            CreateTable(
                "dbo.UserAccounts",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        Tenant = c.String(nullable: false, maxLength: 50),
                        Username = c.String(nullable: false, maxLength: 100),
                        Created = c.DateTime(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                        IsAccountClosed = c.Boolean(nullable: false),
                        AccountClosed = c.DateTime(),
                        IsLoginAllowed = c.Boolean(nullable: false),
                        LastLogin = c.DateTime(),
                        LastFailedLogin = c.DateTime(),
                        FailedLoginCount = c.Int(nullable: false),
                        PasswordChanged = c.DateTime(),
                        RequiresPasswordReset = c.Boolean(nullable: false),
                        Email = c.String(maxLength: 100),
                        IsAccountVerified = c.Boolean(nullable: false),
                        LastFailedPasswordReset = c.DateTime(),
                        FailedPasswordResetCount = c.Int(nullable: false),
                        MobileCode = c.String(maxLength: 100),
                        MobileCodeSent = c.DateTime(),
                        MobilePhoneNumber = c.String(maxLength: 20),
                        MobilePhoneNumberChanged = c.DateTime(),
                        AccountTwoFactorAuthMode = c.Int(nullable: false),
                        CurrentTwoFactorAuthStatus = c.Int(nullable: false),
                        VerificationKey = c.String(maxLength: 100),
                        VerificationPurpose = c.Int(),
                        VerificationKeySent = c.DateTime(),
                        VerificationStorage = c.String(maxLength: 100),
                        HashedPassword = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.UserClaims",
                c => new
                    {
                        UserAccountID = c.Guid(nullable: false),
                        Type = c.String(nullable: false, maxLength: 150),
                        Value = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => new { t.UserAccountID, t.Type, t.Value })
                .ForeignKey("dbo.UserAccounts", t => t.UserAccountID, cascadeDelete: true)
                .Index(t => t.UserAccountID);
            
            CreateTable(
                "dbo.LinkedAccountClaims",
                c => new
                    {
                        UserAccountID = c.Guid(nullable: false),
                        ProviderName = c.String(nullable: false, maxLength: 30),
                        Type = c.String(nullable: false, maxLength: 150),
                        Value = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => new { t.UserAccountID, t.ProviderName, t.Type, t.Value })
                .ForeignKey("dbo.UserAccounts", t => t.UserAccountID, cascadeDelete: true)
                .Index(t => t.UserAccountID);
            
            CreateTable(
                "dbo.LinkedAccounts",
                c => new
                    {
                        UserAccountID = c.Guid(nullable: false),
                        ProviderName = c.String(nullable: false, maxLength: 30),
                        ProviderAccountID = c.String(nullable: false, maxLength: 100),
                        LastLogin = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserAccountID, t.ProviderName, t.ProviderAccountID })
                .ForeignKey("dbo.UserAccounts", t => t.UserAccountID, cascadeDelete: true)
                .Index(t => t.UserAccountID);
            
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
            
            CreateTable(
                "dbo.UserCertificates",
                c => new
                    {
                        UserAccountID = c.Guid(nullable: false),
                        Thumbprint = c.String(nullable: false, maxLength: 150),
                        Subject = c.String(maxLength: 250),
                    })
                .PrimaryKey(t => new { t.UserAccountID, t.Thumbprint })
                .ForeignKey("dbo.UserAccounts", t => t.UserAccountID, cascadeDelete: true)
                .Index(t => t.UserAccountID);
            
            DropTable("dbo.UserAccounts");
            DropTable("dbo.UserCertificates");
            DropTable("dbo.UserClaims");
            DropTable("dbo.LinkedAccounts");
            DropTable("dbo.LinkedAccountClaims");
            DropTable("dbo.PasswordResetSecrets");
            DropTable("dbo.TwoFactorAuthTokens");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.TwoFactorAuthTokens",
                c => new
                    {
                        UserAccountID = c.Guid(nullable: false),
                        Token = c.String(nullable: false, maxLength: 100),
                        Issued = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserAccountID, t.Token });
            
            CreateTable(
                "dbo.PasswordResetSecrets",
                c => new
                    {
                        PasswordResetSecretID = c.Guid(nullable: false),
                        UserAccountID = c.Guid(nullable: false),
                        Question = c.String(nullable: false, maxLength: 150),
                        Answer = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => t.PasswordResetSecretID);
            
            CreateTable(
                "dbo.LinkedAccountClaims",
                c => new
                    {
                        UserAccountID = c.Guid(nullable: false),
                        ProviderName = c.String(nullable: false, maxLength: 30),
                        ProviderAccountID = c.String(nullable: false, maxLength: 100),
                        Type = c.String(nullable: false, maxLength: 150),
                        Value = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => new { t.UserAccountID, t.ProviderName, t.ProviderAccountID, t.Type, t.Value });
            
            CreateTable(
                "dbo.LinkedAccounts",
                c => new
                    {
                        UserAccountID = c.Guid(nullable: false),
                        ProviderName = c.String(nullable: false, maxLength: 30),
                        ProviderAccountID = c.String(nullable: false, maxLength: 100),
                        LastLogin = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserAccountID, t.ProviderName, t.ProviderAccountID });
            
            CreateTable(
                "dbo.UserClaims",
                c => new
                    {
                        UserAccountID = c.Guid(nullable: false),
                        Type = c.String(nullable: false, maxLength: 150),
                        Value = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => new { t.UserAccountID, t.Type, t.Value });
            
            CreateTable(
                "dbo.UserCertificates",
                c => new
                    {
                        UserAccountID = c.Guid(nullable: false),
                        Thumbprint = c.String(nullable: false, maxLength: 150),
                        Subject = c.String(maxLength: 250),
                    })
                .PrimaryKey(t => new { t.UserAccountID, t.Thumbprint });
            
            CreateTable(
                "dbo.UserAccounts",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        Tenant = c.String(nullable: false, maxLength: 50),
                        Username = c.String(nullable: false, maxLength: 100),
                        Created = c.DateTime(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                        IsAccountClosed = c.Boolean(nullable: false),
                        AccountClosed = c.DateTime(),
                        IsLoginAllowed = c.Boolean(nullable: false),
                        LastLogin = c.DateTime(),
                        LastFailedLogin = c.DateTime(),
                        FailedLoginCount = c.Int(nullable: false),
                        PasswordChanged = c.DateTime(),
                        RequiresPasswordReset = c.Boolean(nullable: false),
                        Email = c.String(maxLength: 100),
                        IsAccountVerified = c.Boolean(nullable: false),
                        LastFailedPasswordReset = c.DateTime(),
                        FailedPasswordResetCount = c.Int(nullable: false),
                        MobileCode = c.String(maxLength: 100),
                        MobileCodeSent = c.DateTime(),
                        MobilePhoneNumber = c.String(maxLength: 20),
                        MobilePhoneNumberChanged = c.DateTime(),
                        AccountTwoFactorAuthMode = c.Int(nullable: false),
                        CurrentTwoFactorAuthStatus = c.Int(nullable: false),
                        VerificationKey = c.String(maxLength: 100),
                        VerificationPurpose = c.Int(),
                        VerificationKeySent = c.DateTime(),
                        VerificationStorage = c.String(maxLength: 100),
                        HashedPassword = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.ID);
            
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
            DropTable("dbo.UserCertificates");
            DropTable("dbo.TwoFactorAuthTokens");
            DropTable("dbo.PasswordResetSecrets");
            DropTable("dbo.LinkedAccounts");
            DropTable("dbo.LinkedAccountClaims");
            DropTable("dbo.UserClaims");
            DropTable("dbo.UserAccounts");
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
