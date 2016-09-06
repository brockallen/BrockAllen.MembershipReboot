namespace BrockAllen.MembershipReboot.Ef.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAccountApproval : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserAccounts", "AccountApproved", c => c.DateTime());
            AddColumn("dbo.UserAccounts", "AccountRejected", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserAccounts", "AccountRejected");
            DropColumn("dbo.UserAccounts", "AccountApproved");
        }
    }
}
