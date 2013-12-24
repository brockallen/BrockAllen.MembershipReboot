namespace BrockAllen.MembershipReboot.Nh
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Iesi.Collections.Generic;

    public class NhUserAccount : UserAccount
    {
        /// <summary>
        ///     To help ensure hashcode uniqueness, a carefully selected random number multiplier 
        ///     is used within the calculation.  Goodrich and Tamassia's Data Structures and
        ///     Algorithms in Java asserts that 31, 33, 37, 39 and 41 will produce the fewest number
        ///     of collissions.  See http://computinglife.wordpress.com/2008/11/20/why-do-hash-functions-use-prime-numbers/
        ///     for more information.
        /// </summary>
        private const int HashMultiplier = 31;

        private int? cachedHashcode;

        public NhUserAccount()
        {
            this.ClaimsCollection = new HashedSet<NhUserClaim>();
            this.LinkedAccountsCollection = new HashedSet<NhLinkedAccount>();
            this.LinkedAccountClaimsCollection = new HashedSet<NhLinkedAccountClaim>();
            this.CertificatesCollection = new HashedSet<NhUserCertificate>();
            this.TwoFactorAuthTokensCollection = new HashedSet<NhTwoFactorAuthToken>();
            this.PasswordResetSecretsCollection = new HashedSet<NhPasswordResetSecret>();
        }

        public override IEnumerable<UserClaim> Claims
        {
            get
            {
                return this.ClaimsCollection;
            }
        }

        protected override void AddClaim(UserClaim item)
        {
            var claim = new NhUserClaim();
            claim.GetType().GetProperty("Type").SetValue(claim, item.Type);
            claim.GetType().GetProperty("Value").SetValue(claim, item.Value);
            claim.GetType().GetProperty("Account").SetValue(claim, this);
            this.ClaimsCollection.Add(claim);
        }

        protected override void RemoveClaim(UserClaim item)
        {
            var removed = this.ClaimsCollection.SingleOrDefault(x => x.Type == item.Type && x.Value == item.Value);
            this.ClaimsCollection.Remove(removed);
        }

        public override IEnumerable<LinkedAccount> LinkedAccounts
        {
            get
            {
                return this.LinkedAccountsCollection;
            }
        }

        protected override void AddLinkedAccount(LinkedAccount item)
        {
            var linkedAccount = new NhLinkedAccount();
            linkedAccount.GetType().GetProperty("LastLogin").SetValue(linkedAccount, item.LastLogin);
            linkedAccount.GetType().GetProperty("ProviderAccountID").SetValue(linkedAccount, item.ProviderAccountID);
            linkedAccount.GetType().GetProperty("ProviderName").SetValue(linkedAccount, item.ProviderName);
            linkedAccount.GetType().GetProperty("Account").SetValue(linkedAccount, this);
            this.LinkedAccountsCollection.Add(linkedAccount);
        }

        protected override void RemoveLinkedAccount(LinkedAccount item)
        {
            var removed =
                this.LinkedAccountsCollection.SingleOrDefault(
                    x =>
                    x.ProviderAccountID == item.ProviderAccountID && x.ProviderName == item.ProviderName
                    && x.LastLogin == item.LastLogin);
            this.LinkedAccountsCollection.Remove(removed);
        }

        public override IEnumerable<LinkedAccountClaim> LinkedAccountClaims
        {
            get
            {
                return this.LinkedAccountClaimsCollection;
            }
        }

        protected override void AddLinkedAccountClaim(LinkedAccountClaim item)
        {
            var linkedAccountClaim = new NhLinkedAccountClaim();
            linkedAccountClaim.GetType()
                .GetProperty("ProviderAccountID")
                .SetValue(linkedAccountClaim, item.ProviderAccountID);
            linkedAccountClaim.GetType().GetProperty("ProviderName").SetValue(linkedAccountClaim, item.ProviderName);
            linkedAccountClaim.GetType().GetProperty("Type").SetValue(linkedAccountClaim, item.Type);
            linkedAccountClaim.GetType().GetProperty("Value").SetValue(linkedAccountClaim, item.Value);
            linkedAccountClaim.GetType().GetProperty("Account").SetValue(linkedAccountClaim, this);
            this.LinkedAccountClaimsCollection.Add(linkedAccountClaim);
        }

        protected override void RemoveLinkedAccountClaim(LinkedAccountClaim item)
        {
            var removed =
                this.LinkedAccountClaimsCollection.SingleOrDefault(
                    x =>
                    x.ProviderAccountID == item.ProviderAccountID && x.ProviderName == item.ProviderName
                    && x.Type == item.Type && x.Value == item.Value);
            this.LinkedAccountClaimsCollection.Remove(removed);
        }

        public override IEnumerable<UserCertificate> Certificates
        {
            get
            {
                return this.CertificatesCollection;
            }
        }

        protected override void AddCertificate(UserCertificate item)
        {
            var userCertificate = new NhUserCertificate();
            userCertificate.GetType().GetProperty("Thumbprint").SetValue(userCertificate, item.Thumbprint);
            userCertificate.GetType().GetProperty("Subject").SetValue(userCertificate, item.Subject);
            userCertificate.GetType().GetProperty("Account").SetValue(userCertificate, this);
            this.CertificatesCollection.Add(userCertificate);
        }

        protected override void RemoveCertificate(UserCertificate item)
        {
            var removed =
                this.CertificatesCollection.SingleOrDefault(
                    x => x.Thumbprint == item.Thumbprint && x.Subject == item.Subject);
            this.CertificatesCollection.Remove(removed);
        }

        public override IEnumerable<TwoFactorAuthToken> TwoFactorAuthTokens
        {
            get
            {
                return this.TwoFactorAuthTokensCollection;
            }
        }

        protected override void AddTwoFactorAuthToken(TwoFactorAuthToken item)
        {
            var twoFactorAuthToken = new NhTwoFactorAuthToken();
            twoFactorAuthToken.GetType().GetProperty("Issued").SetValue(twoFactorAuthToken, item.Issued);
            twoFactorAuthToken.GetType().GetProperty("Token").SetValue(twoFactorAuthToken, item.Token);
            twoFactorAuthToken.GetType().GetProperty("Account").SetValue(twoFactorAuthToken, this);
            this.TwoFactorAuthTokensCollection.Add(twoFactorAuthToken);
        }

        protected override void RemoveTwoFactorAuthToken(TwoFactorAuthToken item)
        {
            var removed =
                this.TwoFactorAuthTokensCollection.SingleOrDefault(
                    x => x.Token == item.Token && x.Issued == item.Issued);
            this.TwoFactorAuthTokensCollection.Remove(removed);
        }

        public override IEnumerable<PasswordResetSecret> PasswordResetSecrets
        {
            get
            {
                return this.PasswordResetSecretsCollection;
            }
        }

        protected override void AddPasswordResetSecret(PasswordResetSecret item)
        {
            var passwordResetSecret = new NhPasswordResetSecret();
            passwordResetSecret.GetType()
                .GetProperty("PasswordResetSecretID")
                .SetValue(passwordResetSecret, item.PasswordResetSecretID);
            passwordResetSecret.GetType().GetProperty("Question").SetValue(passwordResetSecret, item.Question);
            passwordResetSecret.GetType().GetProperty("Answer").SetValue(passwordResetSecret, item.Answer);
            passwordResetSecret.GetType().GetProperty("Account").SetValue(passwordResetSecret, this);
            this.PasswordResetSecretsCollection.Add(passwordResetSecret);
        }

        protected override void RemovePasswordResetSecret(PasswordResetSecret item)
        {
            var removed =
                this.PasswordResetSecretsCollection.SingleOrDefault(
                    x =>
                    x.PasswordResetSecretID == item.PasswordResetSecretID && x.Question == item.Question
                    && x.Answer == item.Answer);
            this.PasswordResetSecretsCollection.Remove(removed);
        }

        public virtual long Version { get; protected set; }

        public virtual ICollection<NhUserClaim> ClaimsCollection { get; protected set; }
        public virtual ICollection<NhLinkedAccount> LinkedAccountsCollection { get; protected set; }
        public virtual ICollection<NhLinkedAccountClaim> LinkedAccountClaimsCollection { get; protected set; }
        public virtual ICollection<NhUserCertificate> CertificatesCollection { get; protected set; }
        public virtual ICollection<NhTwoFactorAuthToken> TwoFactorAuthTokensCollection { get; protected set; }
        public virtual ICollection<NhPasswordResetSecret> PasswordResetSecretsCollection { get; protected set; }

        public static bool operator ==(NhUserAccount lhs, NhUserAccount rhs)
        {
            return Equals(lhs, rhs);
        }

        public static bool operator !=(NhUserAccount lhs, NhUserAccount rhs)
        {
            return !Equals(lhs, rhs);
        }

        public override bool Equals(object obj)
        {
            var other = obj as NhUserAccount;
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (!this.IsTransient() && !other.IsTransient())
            {
                if (this.ID == other.ID)
                {
                    var otherType = other.GetUnproxiedType();
                    var thisType = this.GetUnproxiedType();
                    return thisType.IsAssignableFrom(otherType) || otherType.IsAssignableFrom(thisType);
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            // once we have a hashcode we'll never change it
            if (this.cachedHashcode.HasValue)
            {
                return this.cachedHashcode.Value;
            }

            // when this instance is new we use the base hash code
            // and remember it, so an instance can NEVER change its
            // hash code.
            if (this.IsTransient())
            {
                this.cachedHashcode = base.GetHashCode();
            }
            else
            {
                unchecked
                {
                    // It's possible for two objects to return the same hash code based on 
                    // identically valued properties, even if they're of two different types, 
                    // so we include the object's type in the hash calculation
                    var hashCode = this.GetType().GetHashCode();
                    this.cachedHashcode = (hashCode * HashMultiplier) ^ this.ID.GetHashCode();
                }
            }

            return this.cachedHashcode.Value;
        }

        protected bool IsTransient()
        {
            return this.Version == default(long);
        }

        /// <summary>
        ///     When NHibernate proxies objects, it masks the type of the actual entity object.
        ///     This wrapper burrows into the proxied object to get its actual type.
        /// 
        ///     Although this assumes NHibernate is being used, it doesn't require any NHibernate
        ///     related dependencies and has no bad side effects if NHibernate isn't being used.
        /// 
        ///     Related discussion is at http://groups.google.com/group/sharp-architecture/browse_thread/thread/ddd05f9baede023a ...thanks Jay Oliver!
        /// </summary>
        protected virtual Type GetUnproxiedType()
        {
            return this.GetType();
        }
    }
}