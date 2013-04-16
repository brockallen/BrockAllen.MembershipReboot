namespace BrockAllen.MembershipReboot.Migrations.SqlCe
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VerificationPurpose : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserAccounts", "VerificationPurpose", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserAccounts", "VerificationPurpose");
        }
    }
}
