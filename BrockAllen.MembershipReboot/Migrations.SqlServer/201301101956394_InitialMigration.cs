namespace BrockAllen.MembershipReboot.Migrations.SqlServer
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
                        ID = c.Int(nullable: false, identity: true),
                        Tenant = c.String(nullable: false, maxLength: 50),
                        Username = c.String(nullable: false, maxLength: 100),
                        Email = c.String(nullable: false, maxLength: 100),
                        Created = c.DateTime(nullable: false),
                        PasswordChanged = c.DateTime(nullable: false),
                        IsAccountVerified = c.Boolean(nullable: false),
                        IsLoginAllowed = c.Boolean(nullable: false),
                        IsAccountClosed = c.Boolean(nullable: false),
                        LastLogin = c.DateTime(),
                        LastFailedLogin = c.DateTime(),
                        FailedLoginCount = c.Int(nullable: false),
                        VerificationKey = c.String(maxLength: 50),
                        VerificationKeySent = c.DateTime(),
                        HashedPassword = c.String(nullable: false, maxLength: 200),
                    })
                .PrimaryKey(t => t.ID)
                .Index(t => new { t.Tenant, t.Username }, unique: true)
                .Index(t => t.VerificationKey);
            
            CreateTable(
                "dbo.UserClaims",
                c => new
                    {
                        UserAccountID = c.Int(nullable: false),
                        Type = c.String(nullable: false, maxLength: 150),
                        Value = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => new { t.UserAccountID, t.Type, t.Value })
                .ForeignKey("dbo.UserAccounts", t => t.UserAccountID, cascadeDelete: true)
                .Index(t => t.UserAccountID);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.UserClaims", new[] { "UserAccountID" });
            DropForeignKey("dbo.UserClaims", "UserAccountID", "dbo.UserAccounts");
            DropTable("dbo.UserClaims");
            DropTable("dbo.UserAccounts");
        }
    }
}
