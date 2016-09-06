﻿/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot
{
    public abstract class UserAccount
    {
        public virtual Guid ID { get; protected internal set; }

        [StringLength(50)]
        [Required]
        public virtual string Tenant { get; protected internal set; }
        
        // If email address is being used as the username then this property
        // should adhere to maximim length constraint for valid email addresses.
        // See Dominic Sayers answer at SO: http://stackoverflow.com/a/574698/99240
        [StringLength(254)]
        [Required]
        public virtual string Username { get; protected internal set; }

        public virtual DateTime Created { get; protected internal set; }
        public virtual DateTime LastUpdated { get; protected internal set; }
        public virtual bool IsAccountApproved { get { return AccountApproved.HasValue; } }
        public virtual DateTime? AccountApproved { get; protected internal set; }
        public virtual bool IsAccountRejected { get { return AccountRejected.HasValue; } }
        public virtual DateTime? AccountRejected { get; protected internal set; }
        public virtual bool IsAccountClosed { get; protected internal set; }
        public virtual DateTime? AccountClosed { get; protected internal set; }

        /// <summary>
        /// Returns true when the account is locked
        /// </summary>
        /// <remarks>
        /// An locked account will not be able to sign in
        /// </remarks>
        public virtual bool IsLoginAllowed { get; protected internal set; }
        public virtual DateTime? LastLogin { get; protected internal set; }
        public virtual DateTime? LastFailedLogin { get; protected internal set; }
        public virtual int FailedLoginCount { get; protected internal set; }

        public virtual DateTime? PasswordChanged { get; protected internal set; }
        public virtual bool RequiresPasswordReset { get; protected internal set; }

        // Maximum length of a valid email address is 254 characters.
        // See Dominic Sayers answer at SO: http://stackoverflow.com/a/574698/99240
        [EmailAddress]
        [StringLength(254)]
        public virtual string Email { get; protected internal set; }
        public virtual bool IsAccountVerified { get; protected internal set; }

        public virtual DateTime? LastFailedPasswordReset { get; protected internal set; }
        public virtual int FailedPasswordResetCount { get; protected internal set; }

        [StringLength(100)]
        public virtual string MobileCode { get; protected internal set; }
        public virtual DateTime? MobileCodeSent { get; protected internal set; }
        [StringLength(20)]
        public virtual string MobilePhoneNumber { get; protected internal set; }
        public virtual DateTime? MobilePhoneNumberChanged { get; protected internal set; }

        public virtual TwoFactorAuthMode AccountTwoFactorAuthMode { get; protected internal set; }
        public virtual TwoFactorAuthMode CurrentTwoFactorAuthStatus { get; protected internal set; }

        [StringLength(100)]
        public virtual string VerificationKey { get; protected internal set; }
        public virtual VerificationKeyPurpose? VerificationPurpose { get; protected internal set; }
        public virtual DateTime? VerificationKeySent { get; protected internal set; }
        [StringLength(100)]
        public virtual string VerificationStorage { get; protected internal set; }

        [StringLength(200)]
        public virtual string HashedPassword { get; protected internal set; }

        public abstract IEnumerable<UserClaim> Claims { get; }
        protected internal abstract void AddClaim(UserClaim item);
        protected internal abstract void RemoveClaim(UserClaim item);

        public abstract IEnumerable<LinkedAccount> LinkedAccounts { get; }
        protected internal abstract void AddLinkedAccount(LinkedAccount item);
        protected internal abstract void RemoveLinkedAccount(LinkedAccount item);
        
        public abstract IEnumerable<LinkedAccountClaim> LinkedAccountClaims { get; }
        protected internal abstract void AddLinkedAccountClaim(LinkedAccountClaim item);
        protected internal abstract void RemoveLinkedAccountClaim(LinkedAccountClaim item);

        public abstract IEnumerable<UserCertificate> Certificates { get; }
        protected internal abstract void AddCertificate(UserCertificate item);
        protected internal abstract void RemoveCertificate(UserCertificate item);

        public abstract IEnumerable<TwoFactorAuthToken> TwoFactorAuthTokens { get; }
        protected internal abstract void AddTwoFactorAuthToken(TwoFactorAuthToken item);
        protected internal abstract void RemoveTwoFactorAuthToken(TwoFactorAuthToken item);

        public abstract IEnumerable<PasswordResetSecret> PasswordResetSecrets { get; }
        protected internal abstract void AddPasswordResetSecret(PasswordResetSecret item);
        protected internal abstract void RemovePasswordResetSecret(PasswordResetSecret item);
    }
}
