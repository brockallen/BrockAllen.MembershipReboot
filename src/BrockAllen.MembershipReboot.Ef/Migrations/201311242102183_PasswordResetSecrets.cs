/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

namespace BrockAllen.MembershipReboot.Ef.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PasswordResetSecrets : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PasswordResetSecrets",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        UserAccountID = c.Guid(nullable: false),
                        Question = c.String(nullable: false, maxLength: 150),
                        Answer = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => new { t.ID, t.UserAccountID })
                .ForeignKey("dbo.UserAccounts", t => t.UserAccountID, cascadeDelete: true)
                .Index(t => t.UserAccountID);
            
            AddColumn("dbo.UserAccounts", "LastFailedPasswordReset", c => c.DateTime());
            AddColumn("dbo.UserAccounts", "FailedPasswordResetCount", c => c.Int(nullable: false));
            AlterColumn("dbo.UserAccounts", "MobileCode", c => c.String(maxLength: 100));
            AlterColumn("dbo.UserAccounts", "MobilePhoneNumber", c => c.String(maxLength: 20));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PasswordResetSecrets", "UserAccountID", "dbo.UserAccounts");
            DropIndex("dbo.PasswordResetSecrets", new[] { "UserAccountID" });
            AlterColumn("dbo.UserAccounts", "MobilePhoneNumber", c => c.String());
            AlterColumn("dbo.UserAccounts", "MobileCode", c => c.String());
            DropColumn("dbo.UserAccounts", "FailedPasswordResetCount");
            DropColumn("dbo.UserAccounts", "LastFailedPasswordReset");
            DropTable("dbo.PasswordResetSecrets");
        }
    }
}
