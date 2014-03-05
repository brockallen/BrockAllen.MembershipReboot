namespace BrockAllen.MembershipReboot.Ef.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v7_InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Groups",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        ID = c.Guid(nullable: false),
                        Tenant = c.String(nullable: false, maxLength: 50),
                        Name = c.String(nullable: false, maxLength: 100),
                        Created = c.DateTime(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Key);
            
            CreateTable(
                "dbo.GroupChilds",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        ParentKey = c.Int(nullable: false),
                        ChildGroupID = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("dbo.Groups", t => t.ParentKey, cascadeDelete: true)
                .Index(t => t.ParentKey);
            
            CreateTable(
                "dbo.UserAccounts",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        ID = c.Guid(nullable: false),
                        Tenant = c.String(nullable: false, maxLength: 50),
                        Username = c.String(nullable: false, maxLength: 254),
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
                        Email = c.String(maxLength: 254),
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
                .PrimaryKey(t => t.Key);
            
            CreateTable(
                "dbo.UserClaims",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        ParentKey = c.Int(nullable: false),
                        Type = c.String(nullable: false, maxLength: 150),
                        Value = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("dbo.UserAccounts", t => t.ParentKey, cascadeDelete: true)
                .Index(t => t.ParentKey);
            
            CreateTable(
                "dbo.LinkedAccountClaims",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        ParentKey = c.Int(nullable: false),
                        ProviderName = c.String(nullable: false, maxLength: 30),
                        ProviderAccountID = c.String(nullable: false, maxLength: 100),
                        Type = c.String(nullable: false, maxLength: 150),
                        Value = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("dbo.UserAccounts", t => t.ParentKey, cascadeDelete: true)
                .Index(t => t.ParentKey);
            
            CreateTable(
                "dbo.LinkedAccounts",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        ParentKey = c.Int(nullable: false),
                        ProviderName = c.String(nullable: false, maxLength: 30),
                        ProviderAccountID = c.String(nullable: false, maxLength: 100),
                        LastLogin = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("dbo.UserAccounts", t => t.ParentKey, cascadeDelete: true)
                .Index(t => t.ParentKey);
            
            CreateTable(
                "dbo.PasswordResetSecrets",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        ParentKey = c.Int(nullable: false),
                        PasswordResetSecretID = c.Guid(nullable: false),
                        Question = c.String(nullable: false, maxLength: 150),
                        Answer = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("dbo.UserAccounts", t => t.ParentKey, cascadeDelete: true)
                .Index(t => t.ParentKey);
            
            CreateTable(
                "dbo.TwoFactorAuthTokens",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        ParentKey = c.Int(nullable: false),
                        Token = c.String(nullable: false, maxLength: 100),
                        Issued = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("dbo.UserAccounts", t => t.ParentKey, cascadeDelete: true)
                .Index(t => t.ParentKey);
            
            CreateTable(
                "dbo.UserCertificates",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        ParentKey = c.Int(nullable: false),
                        Thumbprint = c.String(nullable: false, maxLength: 150),
                        Subject = c.String(maxLength: 250),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("dbo.UserAccounts", t => t.ParentKey, cascadeDelete: true)
                .Index(t => t.ParentKey);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserCertificates", "ParentKey", "dbo.UserAccounts");
            DropForeignKey("dbo.TwoFactorAuthTokens", "ParentKey", "dbo.UserAccounts");
            DropForeignKey("dbo.PasswordResetSecrets", "ParentKey", "dbo.UserAccounts");
            DropForeignKey("dbo.LinkedAccounts", "ParentKey", "dbo.UserAccounts");
            DropForeignKey("dbo.LinkedAccountClaims", "ParentKey", "dbo.UserAccounts");
            DropForeignKey("dbo.UserClaims", "ParentKey", "dbo.UserAccounts");
            DropForeignKey("dbo.GroupChilds", "ParentKey", "dbo.Groups");
            DropIndex("dbo.UserCertificates", new[] { "ParentKey" });
            DropIndex("dbo.TwoFactorAuthTokens", new[] { "ParentKey" });
            DropIndex("dbo.PasswordResetSecrets", new[] { "ParentKey" });
            DropIndex("dbo.LinkedAccounts", new[] { "ParentKey" });
            DropIndex("dbo.LinkedAccountClaims", new[] { "ParentKey" });
            DropIndex("dbo.UserClaims", new[] { "ParentKey" });
            DropIndex("dbo.GroupChilds", new[] { "ParentKey" });
            DropTable("dbo.UserCertificates");
            DropTable("dbo.TwoFactorAuthTokens");
            DropTable("dbo.PasswordResetSecrets");
            DropTable("dbo.LinkedAccounts");
            DropTable("dbo.LinkedAccountClaims");
            DropTable("dbo.UserClaims");
            DropTable("dbo.UserAccounts");
            DropTable("dbo.GroupChilds");
            DropTable("dbo.Groups");
        }
    }
}
