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

namespace BrockAllen.MembershipReboot.Relational
{
    public class RelationalUserAccount : UserAccount
    {
        public virtual ICollection<RelationalUserClaim> ClaimCollection { get; set; }
        public override IEnumerable<UserClaim> Claims
        {
            get { return ClaimCollection; }
        }
        public override void AddClaim(UserClaim item)
        {
            ClaimCollection.Add(new RelationalUserClaim { UserAccountID = this.ID, Type = item.Type, Value = item.Value });
        }
        public override void RemoveClaim(UserClaim item)
        {
            ClaimCollection.Remove((RelationalUserClaim)item);
        }

        public virtual ICollection<RelationalLinkedAccount> LinkedAccountCollection { get; set; }
        public override IEnumerable<LinkedAccount> LinkedAccounts
        {
            get { return LinkedAccountCollection; }
        }
        public override void AddLinkedAccount(LinkedAccount item)
        {
            LinkedAccountCollection.Add(new RelationalLinkedAccount { UserAccountID = this.ID, ProviderName = item.ProviderName, ProviderAccountID = item.ProviderAccountID, LastLogin = item.LastLogin });
        }
        public override void RemoveLinkedAccount(LinkedAccount item)
        {
            LinkedAccountCollection.Remove((RelationalLinkedAccount)item);
        }

        public virtual ICollection<RelationalLinkedAccountClaim> LinkedAccountClaimCollection { get; set; }
        public override IEnumerable<LinkedAccountClaim> LinkedAccountClaims
        {
            get { return LinkedAccountClaimCollection; }
        }
        public override void AddLinkedAccountClaim(LinkedAccountClaim item)
        {
            LinkedAccountClaimCollection.Add(new RelationalLinkedAccountClaim { UserAccountID = this.ID, ProviderName = item.ProviderName, Type = item.Type, Value = item.Value });
        }
        public override void RemoveLinkedAccountClaim(LinkedAccountClaim item)
        {
            LinkedAccountClaimCollection.Remove((RelationalLinkedAccountClaim)item);
        }
        
        public virtual ICollection<RelationalUserCertificate> UserCertificateCollection { get; set; }
        public override IEnumerable<UserCertificate> Certificates
        {
            get { return UserCertificateCollection; }
        }
        public override void AddCertificate(UserCertificate item)
        {
            UserCertificateCollection.Add(new RelationalUserCertificate { UserAccountID = this.ID, Thumbprint = item.Thumbprint, Subject = item.Subject });
        }
        public override void RemoveCertificate(UserCertificate item)
        {
            UserCertificateCollection.Remove((RelationalUserCertificate)item);
        }

        public virtual ICollection<RelationalTwoFactorAuthToken> TwoFactorAuthTokenCollection { get; set; }
        public override IEnumerable<TwoFactorAuthToken> TwoFactorAuthTokens
        {
            get { return TwoFactorAuthTokenCollection; }
        }
        public override void AddTwoFactorAuthToken(TwoFactorAuthToken item)
        {
            TwoFactorAuthTokenCollection.Add(new RelationalTwoFactorAuthToken { UserAccountID = this.ID, Token = item.Token, Issued = item.Issued });
        }
        public override void RemoveTwoFactorAuthToken(TwoFactorAuthToken item)
        {
            TwoFactorAuthTokenCollection.Remove((RelationalTwoFactorAuthToken)item);
        }

        public virtual ICollection<RelationalPasswordResetSecret> PasswordResetSecretCollection { get; set; }
        public override IEnumerable<PasswordResetSecret> PasswordResetSecrets
        {
            get { return PasswordResetSecretCollection; }
        }
        public override void AddPasswordResetSecret(PasswordResetSecret item)
        {
            PasswordResetSecretCollection.Add(new RelationalPasswordResetSecret { UserAccountID = this.ID, PasswordResetSecretID = item.PasswordResetSecretID, Question = item.Question, Answer = item.Answer });
        }
        public override void RemovePasswordResetSecret(PasswordResetSecret item)
        {
            PasswordResetSecretCollection.Remove((RelationalPasswordResetSecret)item);
        }
    }
}
