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
        public UserAccount()
        {
            this.Claims = new HashSet<UserClaim>();
            this.LinkedAccounts = new HashSet<LinkedAccount>();
            this.Certificates = new HashSet<UserCertificate>();
            this.TwoFactorAuthTokens = new HashSet<TwoFactorAuthToken>();
            this.PasswordResetSecrets = new HashSet<PasswordResetSecret>();
        }

        [Key]
        public virtual Guid ID { get; internal set; }

        [StringLength(50)]
        [Required]
        public virtual string Tenant { get; internal set; }
        [StringLength(100)]
        [Required]
        public virtual string Username { get; internal set; }
        
        public virtual DateTime Created { get; internal set; }
        public virtual DateTime LastUpdated { get; internal set; }
        public virtual bool IsAccountClosed { get; internal set; }
        public virtual DateTime? AccountClosed { get; internal set; }

        public virtual bool IsLoginAllowed { get; internal set; }
        public virtual DateTime? LastLogin { get; internal set; }
        public virtual DateTime? LastFailedLogin { get; internal set; }
        public virtual int FailedLoginCount { get; internal set; }
        
        public virtual DateTime? PasswordChanged { get; internal set; }
        public virtual bool RequiresPasswordReset { get; internal set; }

        [EmailAddress]
        [StringLength(100)]
        public virtual string Email { get; internal set; }
        public virtual bool IsAccountVerified { get; internal set; }

        public virtual DateTime? LastFailedPasswordReset { get; internal set; }
        public virtual int FailedPasswordResetCount { get; internal set; }

        [StringLength(100)]
        public virtual string MobileCode { get; internal set; }
        public virtual DateTime? MobileCodeSent { get; internal set; }
        [StringLength(20)]
        public virtual string MobilePhoneNumber { get; internal set; }
        public virtual DateTime? MobilePhoneNumberChanged { get; internal set; }

        public virtual TwoFactorAuthMode AccountTwoFactorAuthMode { get; internal set; }
        public virtual TwoFactorAuthMode CurrentTwoFactorAuthStatus { get; internal set; }

        [StringLength(100)]
        public virtual string VerificationKey { get; internal set; }
        public virtual VerificationKeyPurpose? VerificationPurpose { get; internal set; }
        public virtual DateTime? VerificationKeySent { get; internal set; }
        [StringLength(100)]
        public virtual string VerificationStorage { get; internal set; }

        [StringLength(200)]
        public virtual string HashedPassword { get; internal set; }

        public virtual ICollection<UserClaim> Claims { get; internal set; }
        public virtual ICollection<LinkedAccount> LinkedAccounts { get; internal set; }
        public virtual ICollection<UserCertificate> Certificates { get; internal set; }
        public virtual ICollection<TwoFactorAuthToken> TwoFactorAuthTokens { get; internal set; }
        public virtual ICollection<PasswordResetSecret> PasswordResetSecrets { get; internal set; }
    }
}
