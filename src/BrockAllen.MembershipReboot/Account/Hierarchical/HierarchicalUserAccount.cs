/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace BrockAllen.MembershipReboot.Hierarchical
{
    public class HierarchicalUserAccount : UserAccount
    {
        public HashSet<UserClaim> ClaimCollection = new HashSet<UserClaim>();
        public override IEnumerable<UserClaim> Claims
        {
            get { return ClaimCollection; }
        }
        public override void AddClaim(UserClaim item)
        {
            ClaimCollection.Add(item);
        }
        public override void RemoveClaim(UserClaim item)
        {
            ClaimCollection.Remove(item);
        }

        public HashSet<LinkedAccount> LinkedAccountCollection = new HashSet<LinkedAccount>();
        public override IEnumerable<LinkedAccount> LinkedAccounts
        {
            get { return LinkedAccountCollection; }
        }
        public override void AddLinkedAccount(LinkedAccount item)
        {
            LinkedAccountCollection.Add(item);
        }
        public override void RemoveLinkedAccount(LinkedAccount item)
        {
            LinkedAccountCollection.Remove(item);
        }
        
        public HashSet<LinkedAccountClaim> LinkedAccountClaimCollection = new HashSet<LinkedAccountClaim>();
        public override IEnumerable<LinkedAccountClaim> LinkedAccountClaims
        {
            get { return LinkedAccountClaimCollection; }
        }
        public override void AddLinkedAccountClaim(LinkedAccountClaim item)
        {
            LinkedAccountClaimCollection.Add(item);
        }
        public override void RemoveLinkedAccountClaim(LinkedAccountClaim item)
        {
            LinkedAccountClaimCollection.Remove(item);
        }

        public HashSet<UserCertificate> UserCertificateCollection = new HashSet<UserCertificate>();
        public override IEnumerable<UserCertificate> Certificates
        {
            get { return UserCertificateCollection; }
        }
        public override void AddCertificate(UserCertificate item)
        {
            UserCertificateCollection.Add(item);
        }
        public override void RemoveCertificate(UserCertificate item)
        {
            UserCertificateCollection.Remove(item);
        }

        public HashSet<TwoFactorAuthToken> TwoFactorAuthTokenCollection = new HashSet<TwoFactorAuthToken>();
        public override IEnumerable<TwoFactorAuthToken> TwoFactorAuthTokens
        {
            get { return TwoFactorAuthTokenCollection; }
        }
        public override void AddTwoFactorAuthToken(TwoFactorAuthToken item)
        {
            TwoFactorAuthTokenCollection.Add(item);
        }
        public override void RemoveTwoFactorAuthToken(TwoFactorAuthToken item)
        {
            TwoFactorAuthTokenCollection.Remove(item);
        }

        public HashSet<PasswordResetSecret> PasswordResetSecretCollection = new HashSet<PasswordResetSecret>();
        public override IEnumerable<PasswordResetSecret> PasswordResetSecrets
        {
            get { return PasswordResetSecretCollection; }
        }
        public override void AddPasswordResetSecret(PasswordResetSecret item)
        {
            PasswordResetSecretCollection.Add(item);
        }
        public override void RemovePasswordResetSecret(PasswordResetSecret item)
        {
            PasswordResetSecretCollection.Remove(item);
        }
    }
}
