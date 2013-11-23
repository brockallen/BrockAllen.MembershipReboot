namespace BrockAllen.MembershipReboot.Ef.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TwoFactorAuthTokens : DbMigration
    {
        public override void Up()
        {
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TwoFactorAuthTokens", "UserAccountID", "dbo.UserAccounts");
            DropIndex("dbo.TwoFactorAuthTokens", new[] { "UserAccountID" });
            DropTable("dbo.TwoFactorAuthTokens");
        }
    }
}
