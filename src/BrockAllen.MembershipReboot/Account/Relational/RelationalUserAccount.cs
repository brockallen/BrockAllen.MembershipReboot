/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.Collections.Generic;

namespace BrockAllen.MembershipReboot.Relational
{
    public class RelationalUserAccount<TKey, TUserClaim, TLinkedAccount, TLinkedAccountClaim, TPasswordResetSecret, TTwoFactorAuthToken, TUserCertificate> : UserAccount
        where TUserClaim : RelationalUserClaim<TKey>, new()
        where TLinkedAccount : RelationalLinkedAccount<TKey>, new()
        where TLinkedAccountClaim : RelationalLinkedAccountClaim<TKey>, new()
        where TPasswordResetSecret : RelationalPasswordResetSecret<TKey>, new()
        where TTwoFactorAuthToken : RelationalTwoFactorAuthToken<TKey>, new()
        where TUserCertificate : RelationalUserCertificate<TKey>, new()
    {
        private ICollection<TUserClaim> claimCollection; 
        
        public virtual TKey Key { get; set; }

        public virtual ICollection<TUserClaim> ClaimCollection
        {
            get { return claimCollection ?? (claimCollection = new HashSet<TUserClaim>()); }
            set { claimCollection = value; }
        }
        
        public override IEnumerable<UserClaim> Claims
        {
            get { return ClaimCollection; }
        }
        protected internal override void AddClaim(UserClaim item)
        {
            ClaimCollection.Add(new TUserClaim { ParentKey = this.Key, Type = item.Type, Value = item.Value });
        }
        protected internal override void RemoveClaim(UserClaim item)
        {
            ClaimCollection.Remove((TUserClaim)item);
        }

        public virtual ICollection<TLinkedAccount> LinkedAccountCollection { get; set; }
        public override IEnumerable<LinkedAccount> LinkedAccounts
        {
            get { return LinkedAccountCollection; }
        }
        protected internal override void AddLinkedAccount(LinkedAccount item)
        {
            LinkedAccountCollection.Add(new TLinkedAccount { ParentKey = this.Key, ProviderName = item.ProviderName, ProviderAccountID = item.ProviderAccountID, LastLogin = item.LastLogin });
        }
        protected internal override void RemoveLinkedAccount(LinkedAccount item)
        {
            LinkedAccountCollection.Remove((TLinkedAccount)item);
        }

        public virtual ICollection<TLinkedAccountClaim> LinkedAccountClaimCollection { get; set; }
        public override IEnumerable<LinkedAccountClaim> LinkedAccountClaims
        {
            get { return LinkedAccountClaimCollection; }
        }
        protected internal override void AddLinkedAccountClaim(LinkedAccountClaim item)
        {
            LinkedAccountClaimCollection.Add(new TLinkedAccountClaim { ParentKey = this.Key, ProviderName = item.ProviderName, ProviderAccountID = item.ProviderAccountID, Type = item.Type, Value = item.Value });
        }
        protected internal override void RemoveLinkedAccountClaim(LinkedAccountClaim item)
        {
            LinkedAccountClaimCollection.Remove((TLinkedAccountClaim)item);
        }
        
        public virtual ICollection<TUserCertificate> UserCertificateCollection { get; set; }
        public override IEnumerable<UserCertificate> Certificates
        {
            get { return UserCertificateCollection; }
        }
        protected internal override void AddCertificate(UserCertificate item)
        {
            UserCertificateCollection.Add(new TUserCertificate { ParentKey = this.Key, Thumbprint = item.Thumbprint, Subject = item.Subject });
        }
        protected internal override void RemoveCertificate(UserCertificate item)
        {
            UserCertificateCollection.Remove((TUserCertificate)item);
        }

        public virtual ICollection<TTwoFactorAuthToken> TwoFactorAuthTokenCollection { get; set; }
        public override IEnumerable<TwoFactorAuthToken> TwoFactorAuthTokens
        {
            get { return TwoFactorAuthTokenCollection; }
        }

        protected internal override void AddTwoFactorAuthToken(TwoFactorAuthToken item)
        {
            TwoFactorAuthTokenCollection.Add(new TTwoFactorAuthToken { ParentKey = this.Key, Token = item.Token, Issued = item.Issued });
        }
        protected internal override void RemoveTwoFactorAuthToken(TwoFactorAuthToken item)
        {
            TwoFactorAuthTokenCollection.Remove((TTwoFactorAuthToken)item);
        }

        public virtual ICollection<TPasswordResetSecret> PasswordResetSecretCollection { get; set; }
        public override IEnumerable<PasswordResetSecret> PasswordResetSecrets
        {
            get { return PasswordResetSecretCollection; }
        }
        protected internal override void AddPasswordResetSecret(PasswordResetSecret item)
        {
            PasswordResetSecretCollection.Add(new TPasswordResetSecret { ParentKey = this.Key, PasswordResetSecretID = item.PasswordResetSecretID, Question = item.Question, Answer = item.Answer });
        }
        protected internal override void RemovePasswordResetSecret(PasswordResetSecret item)
        {
            PasswordResetSecretCollection.Remove((TPasswordResetSecret)item);
        }
    }
    
    public class RelationalUserAccount : RelationalUserAccount<int, RelationalUserClaim, RelationalLinkedAccount, RelationalLinkedAccountClaim, RelationalPasswordResetSecret, RelationalTwoFactorAuthToken, RelationalUserCertificate> { }
}
