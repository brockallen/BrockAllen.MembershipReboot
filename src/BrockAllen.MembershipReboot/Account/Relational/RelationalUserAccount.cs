/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.Collections.Generic;

namespace BrockAllen.MembershipReboot.Relational
{
    public class RelationalUserAccount : UserAccount
    {
        public virtual ICollection<RelationalUserClaim> ClaimCollection { get; set; }
        public override IEnumerable<UserClaim> Claims
        {
            get { return ClaimCollection; }
        }
        protected internal override void AddClaim(UserClaim item)
        {
            ClaimCollection.Add(new RelationalUserClaim { UserAccountID = this.ID, Type = item.Type, Value = item.Value });
        }
        protected internal override void RemoveClaim(UserClaim item)
        {
            ClaimCollection.Remove((RelationalUserClaim)item);
        }

        public virtual ICollection<RelationalLinkedAccount> LinkedAccountCollection { get; set; }
        public override IEnumerable<LinkedAccount> LinkedAccounts
        {
            get { return LinkedAccountCollection; }
        }
        protected internal override void AddLinkedAccount(LinkedAccount item)
        {
            LinkedAccountCollection.Add(new RelationalLinkedAccount { UserAccountID = this.ID, ProviderName = item.ProviderName, ProviderAccountID = item.ProviderAccountID, LastLogin = item.LastLogin });
        }
        protected internal override void RemoveLinkedAccount(LinkedAccount item)
        {
            LinkedAccountCollection.Remove((RelationalLinkedAccount)item);
        }

        public virtual ICollection<RelationalLinkedAccountClaim> LinkedAccountClaimCollection { get; set; }
        public override IEnumerable<LinkedAccountClaim> LinkedAccountClaims
        {
            get { return LinkedAccountClaimCollection; }
        }
        protected internal override void AddLinkedAccountClaim(LinkedAccountClaim item)
        {
            LinkedAccountClaimCollection.Add(new RelationalLinkedAccountClaim { UserAccountID = this.ID, ProviderName = item.ProviderName, ProviderAccountID = item.ProviderAccountID, Type = item.Type, Value = item.Value });
        }
        protected internal override void RemoveLinkedAccountClaim(LinkedAccountClaim item)
        {
            LinkedAccountClaimCollection.Remove((RelationalLinkedAccountClaim)item);
        }
        
        public virtual ICollection<RelationalUserCertificate> UserCertificateCollection { get; set; }
        public override IEnumerable<UserCertificate> Certificates
        {
            get { return UserCertificateCollection; }
        }
        protected internal override void AddCertificate(UserCertificate item)
        {
            UserCertificateCollection.Add(new RelationalUserCertificate { UserAccountID = this.ID, Thumbprint = item.Thumbprint, Subject = item.Subject });
        }
        protected internal override void RemoveCertificate(UserCertificate item)
        {
            UserCertificateCollection.Remove((RelationalUserCertificate)item);
        }

        public virtual ICollection<RelationalTwoFactorAuthToken> TwoFactorAuthTokenCollection { get; set; }
        public override IEnumerable<TwoFactorAuthToken> TwoFactorAuthTokens
        {
            get { return TwoFactorAuthTokenCollection; }
        }

        protected internal override void AddTwoFactorAuthToken(TwoFactorAuthToken item)
        {
            TwoFactorAuthTokenCollection.Add(new RelationalTwoFactorAuthToken { UserAccountID = this.ID, Token = item.Token, Issued = item.Issued });
        }
        protected internal override void RemoveTwoFactorAuthToken(TwoFactorAuthToken item)
        {
            TwoFactorAuthTokenCollection.Remove((RelationalTwoFactorAuthToken)item);
        }

        public virtual ICollection<RelationalPasswordResetSecret> PasswordResetSecretCollection { get; set; }
        public override IEnumerable<PasswordResetSecret> PasswordResetSecrets
        {
            get { return PasswordResetSecretCollection; }
        }
        protected internal override void AddPasswordResetSecret(PasswordResetSecret item)
        {
            PasswordResetSecretCollection.Add(new RelationalPasswordResetSecret { UserAccountID = this.ID, PasswordResetSecretID = item.PasswordResetSecretID, Question = item.Question, Answer = item.Answer });
        }
        protected internal override void RemovePasswordResetSecret(PasswordResetSecret item)
        {
            PasswordResetSecretCollection.Remove((RelationalPasswordResetSecret)item);
        }
    }

    // it's unfortunate to have to code an "Int" specific class. a generic type
    // is preferred, but EF currently can't cope with an entity that uses a
    // generic type as a primary key. so this approach is how we'll have to deal 
    // with it for the time being: copy & paste for your key type if you don't 
    // want Guid or Int. Sorry.
    public class RelationalUserAccountInt : UserAccount
    {
        public int Key { get; set; }

        public virtual ICollection<RelationalUserClaimInt> ClaimCollection { get; set; }
        public override IEnumerable<UserClaim> Claims
        {
            get { return ClaimCollection; }
        }
        protected internal override void AddClaim(UserClaim item)
        {
            ClaimCollection.Add(new RelationalUserClaimInt { UserAccountID = this.Key, Type = item.Type, Value = item.Value });
        }
        protected internal override void RemoveClaim(UserClaim item)
        {
            ClaimCollection.Remove((RelationalUserClaimInt)item);
        }

        public virtual ICollection<RelationalLinkedAccountInt> LinkedAccountCollection { get; set; }
        public override IEnumerable<LinkedAccount> LinkedAccounts
        {
            get { return LinkedAccountCollection; }
        }
        protected internal override void AddLinkedAccount(LinkedAccount item)
        {
            LinkedAccountCollection.Add(new RelationalLinkedAccountInt { UserAccountID = this.Key, ProviderName = item.ProviderName, ProviderAccountID = item.ProviderAccountID, LastLogin = item.LastLogin });
        }
        protected internal override void RemoveLinkedAccount(LinkedAccount item)
        {
            LinkedAccountCollection.Remove((RelationalLinkedAccountInt)item);
        }

        public virtual ICollection<RelationalLinkedAccountClaimInt> LinkedAccountClaimCollection { get; set; }
        public override IEnumerable<LinkedAccountClaim> LinkedAccountClaims
        {
            get { return LinkedAccountClaimCollection; }
        }
        protected internal override void AddLinkedAccountClaim(LinkedAccountClaim item)
        {
            LinkedAccountClaimCollection.Add(new RelationalLinkedAccountClaimInt { UserAccountID = this.Key, ProviderName = item.ProviderName, Type = item.Type, Value = item.Value });
        }
        protected internal override void RemoveLinkedAccountClaim(LinkedAccountClaim item)
        {
            LinkedAccountClaimCollection.Remove((RelationalLinkedAccountClaimInt)item);
        }

        public virtual ICollection<RelationalUserCertificateInt> UserCertificateCollection { get; set; }
        public override IEnumerable<UserCertificate> Certificates
        {
            get { return UserCertificateCollection; }
        }
        protected internal override void AddCertificate(UserCertificate item)
        {
            UserCertificateCollection.Add(new RelationalUserCertificateInt { UserAccountID = this.Key, Thumbprint = item.Thumbprint, Subject = item.Subject });
        }
        protected internal override void RemoveCertificate(UserCertificate item)
        {
            UserCertificateCollection.Remove((RelationalUserCertificateInt)item);
        }

        public virtual ICollection<RelationalTwoFactorAuthTokenInt> TwoFactorAuthTokenCollection { get; set; }
        public override IEnumerable<TwoFactorAuthToken> TwoFactorAuthTokens
        {
            get { return TwoFactorAuthTokenCollection; }
        }
        protected internal override void AddTwoFactorAuthToken(TwoFactorAuthToken item)
        {
            TwoFactorAuthTokenCollection.Add(new RelationalTwoFactorAuthTokenInt { UserAccountID = this.Key, Token = item.Token, Issued = item.Issued });
        }
        protected internal override void RemoveTwoFactorAuthToken(TwoFactorAuthToken item)
        {
            TwoFactorAuthTokenCollection.Remove((RelationalTwoFactorAuthTokenInt)item);
        }

        public virtual ICollection<RelationalPasswordResetSecretInt> PasswordResetSecretCollection { get; set; }
        public override IEnumerable<PasswordResetSecret> PasswordResetSecrets
        {
            get { return PasswordResetSecretCollection; }
        }
        protected internal override void AddPasswordResetSecret(PasswordResetSecret item)
        {
            PasswordResetSecretCollection.Add(new RelationalPasswordResetSecretInt { UserAccountID = this.Key, PasswordResetSecretID = item.PasswordResetSecretID, Question = item.Question, Answer = item.Answer });
        }
        protected internal override void RemovePasswordResetSecret(PasswordResetSecret item)
        {
            PasswordResetSecretCollection.Remove((RelationalPasswordResetSecretInt)item);
        }
    }
}
