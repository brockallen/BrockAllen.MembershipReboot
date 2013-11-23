/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

namespace BrockAllen.MembershipReboot.Ef.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Groups",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        Tenant = c.String(nullable: false, maxLength: 50),
                        Name = c.String(nullable: false, maxLength: 100),
                        Created = c.DateTime(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.GroupChilds",
                c => new
                    {
                        GroupID = c.Guid(nullable: false),
                        ChildGroupID = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.GroupID, t.ChildGroupID })
                .ForeignKey("dbo.Groups", t => t.GroupID, cascadeDelete: true)
                .Index(t => t.GroupID);
            
            CreateTable(
                "dbo.UserAccounts",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        Tenant = c.String(nullable: false, maxLength: 50),
                        Username = c.String(nullable: false, maxLength: 100),
                        Email = c.String(nullable: false, maxLength: 100),
                        Created = c.DateTime(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                        PasswordChanged = c.DateTime(nullable: false),
                        RequiresPasswordReset = c.Boolean(nullable: false),
                        MobileCode = c.String(),
                        MobileCodeSent = c.DateTime(),
                        MobilePhoneNumber = c.String(),
                        AccountTwoFactorAuthMode = c.Int(nullable: false),
                        CurrentTwoFactorAuthStatus = c.Int(nullable: false),
                        IsAccountVerified = c.Boolean(nullable: false),
                        IsLoginAllowed = c.Boolean(nullable: false),
                        IsAccountClosed = c.Boolean(nullable: false),
                        AccountClosed = c.DateTime(),
                        LastLogin = c.DateTime(),
                        LastFailedLogin = c.DateTime(),
                        FailedLoginCount = c.Int(nullable: false),
                        VerificationKey = c.String(maxLength: 100),
                        VerificationPurpose = c.Int(),
                        VerificationKeySent = c.DateTime(),
                        HashedPassword = c.String(nullable: false, maxLength: 200),
                    })
                .PrimaryKey(t => t.ID);
            
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
                "dbo.LinkedAccounts",
                c => new
                    {
                        UserAccountID = c.Guid(nullable: false),
                        ProviderName = c.String(nullable: false, maxLength: 50),
                        ProviderAccountID = c.String(nullable: false, maxLength: 100),
                        LastLogin = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserAccountID, t.ProviderName, t.ProviderAccountID })
                .ForeignKey("dbo.UserAccounts", t => t.UserAccountID, cascadeDelete: true)
                .Index(t => t.UserAccountID);
            
            CreateTable(
                "dbo.LinkedAccountClaims",
                c => new
                    {
                        UserAccountID = c.Guid(nullable: false),
                        ProviderName = c.String(nullable: false, maxLength: 50),
                        ProviderAccountID = c.String(nullable: false, maxLength: 100),
                        Type = c.String(nullable: false, maxLength: 150),
                        Value = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => new { t.UserAccountID, t.ProviderName, t.ProviderAccountID, t.Type, t.Value })
                .ForeignKey("dbo.LinkedAccounts", t => new { t.UserAccountID, t.ProviderName, t.ProviderAccountID }, cascadeDelete: true)
                .Index(t => new { t.UserAccountID, t.ProviderName, t.ProviderAccountID });
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LinkedAccounts", "UserAccountID", "dbo.UserAccounts");
            DropForeignKey("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" }, "dbo.LinkedAccounts");
            DropForeignKey("dbo.UserClaims", "UserAccountID", "dbo.UserAccounts");
            DropForeignKey("dbo.UserCertificates", "UserAccountID", "dbo.UserAccounts");
            DropForeignKey("dbo.GroupChilds", "GroupID", "dbo.Groups");
            DropIndex("dbo.LinkedAccounts", new[] { "UserAccountID" });
            DropIndex("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" });
            DropIndex("dbo.UserClaims", new[] { "UserAccountID" });
            DropIndex("dbo.UserCertificates", new[] { "UserAccountID" });
            DropIndex("dbo.GroupChilds", new[] { "GroupID" });
            DropTable("dbo.LinkedAccountClaims");
            DropTable("dbo.LinkedAccounts");
            DropTable("dbo.UserClaims");
            DropTable("dbo.UserCertificates");
            DropTable("dbo.UserAccounts");
            DropTable("dbo.GroupChilds");
            DropTable("dbo.Groups");
        }
    }
}
