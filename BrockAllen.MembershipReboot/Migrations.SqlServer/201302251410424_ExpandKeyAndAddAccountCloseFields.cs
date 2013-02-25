namespace BrockAllen.MembershipReboot.Migrations.SqlServer
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExpandKeyAndAddAccountCloseFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserAccounts", "AccountClosed", c => c.DateTime());
            AlterColumn("dbo.UserAccounts", "VerificationKey", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.UserAccounts", "VerificationKey", c => c.String(maxLength: 50));
            DropColumn("dbo.UserAccounts", "AccountClosed");
        }
    }
}
