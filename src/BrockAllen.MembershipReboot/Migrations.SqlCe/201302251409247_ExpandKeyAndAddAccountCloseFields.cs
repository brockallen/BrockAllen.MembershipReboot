namespace BrockAllen.MembershipReboot.Migrations.SqlCe
{
    using System.Data.Entity.Migrations;
    
    public partial class ExpandKeyAndAddAccountCloseFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserAccounts", "AccountClosed", c => c.DateTime());
            DropIndex("dbo.UserAccounts", new string[] { "VerificationKey" });
            AlterColumn("dbo.UserAccounts", "VerificationKey", c => c.String(maxLength: 100));
            CreateIndex("dbo.UserAccounts", "VerificationKey");
        }
        
        public override void Down()
        {
            DropIndex("dbo.UserAccounts", new string[]{"VerificationKey"});
            AlterColumn("dbo.UserAccounts", "VerificationKey", c => c.String(maxLength: 50));
            CreateIndex("dbo.UserAccounts", "VerificationKey");
            DropColumn("dbo.UserAccounts", "AccountClosed");
        }
    }
}
