/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

namespace BrockAllen.MembershipReboot.Ef.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProviderName : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" }, "dbo.LinkedAccounts");
            DropForeignKey("dbo.LinkedAccounts", "UserAccountID", "dbo.UserAccounts");

            DropIndex("dbo.LinkedAccountClaims", new[] { "UserAccountID", "ProviderName", "ProviderAccountID" });
            DropIndex("dbo.LinkedAccounts", "UserAccountID");

            DropPrimaryKey("dbo.LinkedAccountClaims");
            DropPrimaryKey("dbo.LinkedAccounts");

            AlterColumn("dbo.LinkedAccountClaims", "ProviderName", c => c.String(nullable: false, maxLength: 30));
            AlterColumn("dbo.LinkedAccounts", "ProviderName", c => c.String(nullable: false, maxLength: 30));

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

            AlterColumn("dbo.LinkedAccountClaims", "ProviderName", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.LinkedAccounts", "ProviderName", c => c.String(nullable: false, maxLength: 50));

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
