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

namespace BrockAllen.MembershipReboot
{
    public class UserAccount
    {
        public virtual Guid ID { get; protected internal set; }

        [StringLength(50)]
        [Required]
        public virtual string Tenant { get; protected internal set; }
        [StringLength(100)]
        [Required]
        public virtual string Username { get; protected internal set; }
        
        public virtual DateTime Created { get; protected internal set; }
        public virtual DateTime LastUpdated { get; protected internal set; }
        public virtual bool IsAccountClosed { get; protected internal set; }
        public virtual DateTime? AccountClosed { get; protected internal set; }

        public virtual bool IsLoginAllowed { get; protected internal set; }
        public virtual DateTime? LastLogin { get; protected internal set; }
        public virtual DateTime? LastFailedLogin { get; protected internal set; }
        public virtual int FailedLoginCount { get; protected internal set; }
        
        public virtual DateTime? PasswordChanged { get; protected internal set; }
        public virtual bool RequiresPasswordReset { get; protected internal set; }

        [EmailAddress]
        [StringLength(100)]
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

        public virtual ICollection<UserClaim> Claims { get; protected internal set; }
        public virtual ICollection<LinkedAccount> LinkedAccounts { get; protected internal set; }
        public virtual ICollection<UserCertificate> Certificates { get; protected internal set; }
        public virtual ICollection<TwoFactorAuthToken> TwoFactorAuthTokens { get; protected internal set; }
        public virtual ICollection<PasswordResetSecret> PasswordResetSecrets { get; protected internal set; }
    }
}
