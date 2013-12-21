/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.Collections.Generic;

namespace BrockAllen.MembershipReboot.Hierarchical
{
    public class HierarchicalUserAccount : UserAccount
    {
        public HashSet<UserClaim> ClaimCollection = new HashSet<UserClaim>();
        public override IEnumerable<UserClaim> Claims
        {
            get { return ClaimCollection; }
        }
        protected internal override void AddClaim(UserClaim item)
        {
            ClaimCollection.Add(item);
        }
        protected internal override void RemoveClaim(UserClaim item)
        {
            ClaimCollection.Remove(item);
        }

        public HashSet<LinkedAccount> LinkedAccountCollection = new HashSet<LinkedAccount>();
        public override IEnumerable<LinkedAccount> LinkedAccounts
        {
            get { return LinkedAccountCollection; }
        }
        protected internal override void AddLinkedAccount(LinkedAccount item)
        {
            LinkedAccountCollection.Add(item);
        }
        protected internal override void RemoveLinkedAccount(LinkedAccount item)
        {
            LinkedAccountCollection.Remove(item);
        }
        
        public HashSet<LinkedAccountClaim> LinkedAccountClaimCollection = new HashSet<LinkedAccountClaim>();
        public override IEnumerable<LinkedAccountClaim> LinkedAccountClaims
        {
            get { return LinkedAccountClaimCollection; }
        }
        protected internal override void AddLinkedAccountClaim(LinkedAccountClaim item)
        {
            LinkedAccountClaimCollection.Add(item);
        }
        protected internal override void RemoveLinkedAccountClaim(LinkedAccountClaim item)
        {
            LinkedAccountClaimCollection.Remove(item);
        }

        public HashSet<UserCertificate> UserCertificateCollection = new HashSet<UserCertificate>();
        public override IEnumerable<UserCertificate> Certificates
        {
            get { return UserCertificateCollection; }
        }
        protected internal override void AddCertificate(UserCertificate item)
        {
            UserCertificateCollection.Add(item);
        }
        protected internal override void RemoveCertificate(UserCertificate item)
        {
            UserCertificateCollection.Remove(item);
        }

        public HashSet<TwoFactorAuthToken> TwoFactorAuthTokenCollection = new HashSet<TwoFactorAuthToken>();
        public override IEnumerable<TwoFactorAuthToken> TwoFactorAuthTokens
        {
            get { return TwoFactorAuthTokenCollection; }
        }
        protected internal override void AddTwoFactorAuthToken(TwoFactorAuthToken item)
        {
            TwoFactorAuthTokenCollection.Add(item);
        }
        protected internal override void RemoveTwoFactorAuthToken(TwoFactorAuthToken item)
        {
            TwoFactorAuthTokenCollection.Remove(item);
        }

        public HashSet<PasswordResetSecret> PasswordResetSecretCollection = new HashSet<PasswordResetSecret>();
        public override IEnumerable<PasswordResetSecret> PasswordResetSecrets
        {
            get { return PasswordResetSecretCollection; }
        }
        protected internal override void AddPasswordResetSecret(PasswordResetSecret item)
        {
            PasswordResetSecretCollection.Add(item);
        }
        protected internal override void RemovePasswordResetSecret(PasswordResetSecret item)
        {
            PasswordResetSecretCollection.Remove(item);
        }
    }
}
