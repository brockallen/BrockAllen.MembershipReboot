namespace BrockAllen.MembershipReboot.Ef.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v7_Indexes : DbMigration
    {
        public override void Up()
        {
            this.CreateIndex("Groups", "ID", unique: true);
            this.CreateIndex("Groups", new[] { "Tenant", "Name" }, unique: true);
            this.Sql("ALTER TABLE GroupChilds ADD CONSTRAINT UK_ParentKey_ChildGroupID UNIQUE NONCLUSTERED (ParentKey, ChildGroupID)");

            this.CreateIndex("UserAccounts", "ID", unique: true);
            this.CreateIndex("UserAccounts", new[] { "Tenant", "Username" }, unique: true);
            this.CreateIndex("UserAccounts", new[] { "Tenant", "Email" }, unique: false);
            this.CreateIndex("UserAccounts", "VerificationKey", unique: false);
            this.CreateIndex("UserAccounts", "Username", unique: false);
            this.CreateIndex("LinkedAccounts", new[] { "ProviderName", "ProviderAccountID" }, unique: false);
            this.CreateIndex("UserCertificates", "Thumbprint", unique: false);
            this.Sql("ALTER TABLE LinkedAccounts ADD CONSTRAINT UK_ParentKey_ProviderName_ProviderAccountID UNIQUE NONCLUSTERED (ParentKey, ProviderName, ProviderAccountID)");
            this.Sql("ALTER TABLE LinkedAccountClaims ADD CONSTRAINT UK_ParentKey_ProviderName_ProviderAccountID_Type_Value UNIQUE NONCLUSTERED (ParentKey, ProviderName, ProviderAccountID, Type, Value)");
            this.Sql("ALTER TABLE PasswordResetSecrets ADD CONSTRAINT UK_ParentKey_Question UNIQUE NONCLUSTERED (ParentKey, Question)");
            this.Sql("ALTER TABLE UserCertificates ADD CONSTRAINT UK_ParentKey_Thumbprint UNIQUE NONCLUSTERED (ParentKey, Thumbprint)");
            this.Sql("ALTER TABLE UserClaims ADD CONSTRAINT UK_ParentKey_Type_Value UNIQUE NONCLUSTERED (ParentKey, Type, Value)");
        }

        public override void Down()
        {
            this.DropIndex("Groups", new[] { "ID" });
            this.DropIndex("Groups", new[] { "Tenant", "Name" });
            this.Sql("ALTER TABLE GroupChilds DROP CONSTRAINT UK_ParentKey_ChildGroupID");

            this.DropIndex("UserAccounts", new[] { "ID" });
            this.DropIndex("UserAccounts", new[] { "Tenant", "Username" });
            this.DropIndex("UserAccounts", new[] { "Tenant", "Email" });
            this.DropIndex("UserAccounts", new[] { "VerificationKey" });
            this.DropIndex("UserAccounts", new[] { "Username" });
            this.DropIndex("LinkedAccounts", new[] { "ProviderName", "ProviderAccountID" });
            this.DropIndex("UserCertificates", new[] { "Thumbprint" });
            this.Sql("ALTER TABLE LinkedAccounts DROP CONSTRAINT UK_ParentKey_ProviderName_ProviderAccountID");
            this.Sql("ALTER TABLE LinkedAccountClaims DROP CONSTRAINT UK_ParentKey_ProviderName_ProviderAccountID_Type_Value");
            this.Sql("ALTER TABLE PasswordResetSecrets DROP CONSTRAINT UK_ParentKey_Question");
            this.Sql("ALTER TABLE UserCertificates DROP CONSTRAINT UK_ParentKey_Thumbprint");
            this.Sql("ALTER TABLE UserClaims DROP CONSTRAINT UK_ParentKey_Type_Value");
        }
    }
}
