namespace BrockAllen.MembershipReboot.Migrations.SqlCe
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserAccounts",
                c => new
                    {
                        Tenant = c.String(nullable: false, maxLength: 50),
                        Username = c.String(nullable: false, maxLength: 100),
                        Email = c.String(maxLength: 100),
                        Created = c.DateTime(nullable: false),
                        PasswordChanged = c.DateTime(nullable: false),
                        IsAccountVerified = c.Boolean(nullable: false),
                        IsLoginAllowed = c.Boolean(nullable: false),
                        IsAccountClosed = c.Boolean(nullable: false),
                        LastLogin = c.DateTime(),
                        LastFailedLogin = c.DateTime(),
                        FailedLoginCount = c.Int(nullable: false),
                        VerificationKey = c.String(maxLength: 4000),
                        VerificationKeySent = c.DateTime(),
                        HashedPassword = c.String(nullable: false, maxLength: 4000),
                    })
                .PrimaryKey(t => new { t.Tenant, t.Username });
            
            CreateTable(
                "dbo.UserClaims",
                c => new
                    {
                        Tenant = c.String(nullable: false, maxLength: 50),
                        Username = c.String(nullable: false, maxLength: 100),
                        Type = c.String(nullable: false, maxLength: 150),
                        Value = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => new { t.Tenant, t.Username, t.Type, t.Value })
                .ForeignKey("dbo.UserAccounts", t => new { t.Tenant, t.Username }, cascadeDelete: true)
                .Index(t => new { t.Tenant, t.Username });
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.UserClaims", new[] { "Tenant", "Username" });
            DropForeignKey("dbo.UserClaims", new[] { "Tenant", "Username" }, "dbo.UserAccounts");
            DropTable("dbo.UserClaims");
            DropTable("dbo.UserAccounts");
        }
    }
}
