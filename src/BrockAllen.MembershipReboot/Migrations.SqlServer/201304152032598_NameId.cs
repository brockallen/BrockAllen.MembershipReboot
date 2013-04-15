namespace BrockAllen.MembershipReboot.Migrations.SqlServer
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NameId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserAccounts", "NameID", c => c.Guid(nullable: false, defaultValueSql: "newid()"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserAccounts", "NameID");
        }
    }
}
