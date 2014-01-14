namespace BrockAllen.MembershipReboot.Ef.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v6 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.UserAccounts", "Username", c => c.String(nullable: false, maxLength: 254));
            AlterColumn("dbo.UserAccounts", "Email", c => c.String(maxLength: 254));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.UserAccounts", "Email", c => c.String(maxLength: 100));
            AlterColumn("dbo.UserAccounts", "Username", c => c.String(nullable: false, maxLength: 100));
        }
    }
}
