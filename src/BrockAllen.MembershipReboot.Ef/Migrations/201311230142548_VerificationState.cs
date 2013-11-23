namespace BrockAllen.MembershipReboot.Ef.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VerificationState : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserAccounts", "VerificationStorage", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserAccounts", "VerificationStorage");
        }
    }
}
