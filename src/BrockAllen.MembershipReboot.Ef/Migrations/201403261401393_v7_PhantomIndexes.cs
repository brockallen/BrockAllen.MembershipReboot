namespace BrockAllen.MembershipReboot.Ef.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v7_PhantomIndexes : DbMigration
    {
        public override void Up()
        {
            //CreateIndex("dbo.GroupChilds", "ParentKey");
            //CreateIndex("dbo.UserClaims", "ParentKey");
            //CreateIndex("dbo.LinkedAccountClaims", "ParentKey");
            //CreateIndex("dbo.LinkedAccounts", "ParentKey");
            //CreateIndex("dbo.PasswordResetSecrets", "ParentKey");
            //CreateIndex("dbo.TwoFactorAuthTokens", "ParentKey");
            //CreateIndex("dbo.UserCertificates", "ParentKey");
        }
        
        public override void Down()
        {
            //DropIndex("dbo.UserCertificates", new[] { "ParentKey" });
            //DropIndex("dbo.TwoFactorAuthTokens", new[] { "ParentKey" });
            //DropIndex("dbo.PasswordResetSecrets", new[] { "ParentKey" });
            //DropIndex("dbo.LinkedAccounts", new[] { "ParentKey" });
            //DropIndex("dbo.LinkedAccountClaims", new[] { "ParentKey" });
            //DropIndex("dbo.UserClaims", new[] { "ParentKey" });
            //DropIndex("dbo.GroupChilds", new[] { "ParentKey" });
        }
    }
}
