namespace BrockAllen.MembershipReboot.Ef.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Indexes : DbMigration
    {
        public override void Up()
        {
            this.CreateIndex("dbo.UserAccounts", "ID", unique: true);
            this.CreateIndex("dbo.UserAccounts", new[] { "Tenant", "Username" }, unique: true);
            this.CreateIndex("dbo.UserAccounts", new[] { "Tenant", "Email" }, unique: true);
            this.CreateIndex("dbo.UserAccounts", new[] { "Tenant", "MobilePhoneNumber" }, unique: true);
            this.CreateIndex("dbo.UserAccounts", "VerificationKey", unique: true);
            this.CreateIndex("dbo.UserAccounts", "Username", unique: false);
            this.CreateIndex("dbo.LinkedAccounts", new[] { "ProviderName", "ProviderAccountID" }, unique: false);
            this.CreateIndex("dbo.UserCertificates", "Thumbprint", unique: false);
            this.Sql("ALTER TABLE dbo.LinkedAccounts ADD CONSTRAINT UK_ParentKey_ProviderName_ProviderAccountID UNIQUE NONCLUSTERED (ParentKey, ProviderName, ProviderAccountID)");
            this.Sql("ALTER TABLE dbo.LinkedAccountClaims ADD CONSTRAINT UK_ParentKey_ProviderName_ProviderAccountID_Type_Value UNIQUE NONCLUSTERED (ParentKey, ProviderName, ProviderAccountID, Type, Value)");
            this.Sql("ALTER TABLE dbo.PasswordResetSecrets ADD CONSTRAINT UK_ParentKey_Question UNIQUE NONCLUSTERED (ParentKey, Question)");
            this.Sql("ALTER TABLE dbo.UserCertificates ADD CONSTRAINT UK_ParentKey_Thumbprint UNIQUE NONCLUSTERED (ParentKey, Thumbprint)");
            this.Sql("ALTER TABLE dbo.UserClaims ADD CONSTRAINT UK_ParentKey_Type_Value UNIQUE NONCLUSTERED (ParentKey, Type, Value)");
        }
        
        public override void Down()
        {
            this.DropIndex("dbo.UserAccounts", "ID");
            this.DropIndex("dbo.UserAccounts", new[] { "Tenant", "Username" });
            this.DropIndex("dbo.UserAccounts", new[] { "Tenant", "Email" });
            this.DropIndex("dbo.UserAccounts", new[] { "Tenant", "MobilePhoneNumber" });
            this.DropIndex("dbo.UserAccounts", "VerificationKey");
            this.DropIndex("dbo.UserAccounts", "Username");
            this.DropIndex("dbo.LinkedAccounts", new[] { "ProviderName", "ProviderAccountID" });
            this.DropIndex("dbo.UserCertificates", "Thumbprint");
            this.Sql("ALTER TABLE dbo.LinkedAccounts DROP CONSTRAINT UK_ParentKey_ProviderName_ProviderAccountID");
            this.Sql("ALTER TABLE dbo.LinkedAccountClaims ADD CONSTRAINT UK_ParentKey_ProviderName_ProviderAccountID_Type_Value");
            this.Sql("ALTER TABLE dbo.PasswordResetSecrets ADD CONSTRAINT UK_ParentKey_Question");
            this.Sql("ALTER TABLE dbo.UserCertificates ADD CONSTRAINT UK_ParentKey_Thumbprint");
            this.Sql("ALTER TABLE dbo.UserClaims ADD CONSTRAINT UK_ParentKey_Type_Value");
        }
    }
}
