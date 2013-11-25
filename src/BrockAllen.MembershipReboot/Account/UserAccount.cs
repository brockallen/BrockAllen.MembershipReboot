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
    public interface IUserAccount
    {
        Guid ID { get; set; }

        [StringLength(50)]
        [Required]
        string Tenant { get; set; }
        [StringLength(100)]
        [Required]
        string Username { get; set; }
        [EmailAddress]
        [StringLength(100)]
        [Required]
        string Email { get; set; }

        DateTime Created { get; set; }
        DateTime LastUpdated { get; set; }
        DateTime PasswordChanged { get;  set; }
        bool RequiresPasswordReset { get; set; }

        DateTime? LastFailedPasswordReset { get;  set; }
        int FailedPasswordResetCount { get;  set; }

        [StringLength(100)]
        string MobileCode { get;  set; }
        DateTime? MobileCodeSent { get;  set; }
        [StringLength(20)]
        string MobilePhoneNumber { get;  set; }
        DateTime? MobilePhoneNumberChanged { get;  set; }

        TwoFactorAuthMode AccountTwoFactorAuthMode { get;  set; }
        TwoFactorAuthMode CurrentTwoFactorAuthStatus { get;  set; }

        bool IsAccountVerified { get;  set; }
        bool IsLoginAllowed { get; set; }
        bool IsAccountClosed { get;  set; }
        DateTime? AccountClosed { get;  set; }

        DateTime? LastLogin { get;  set; }
        DateTime? LastFailedLogin { get;  set; }
        int FailedLoginCount { get;  set; }

        [StringLength(100)]
        string VerificationKey { get;  set; }
        VerificationKeyPurpose? VerificationPurpose { get;  set; }
        DateTime? VerificationKeySent { get;  set; }
        [StringLength(100)]
        string VerificationStorage { get;  set; }

        [Required]
        [StringLength(200)]
        string HashedPassword { get;  set; }

        ICollection<IUserClaim> Claims { get;  set; }
        ICollection<ILinkedAccount> LinkedAccounts { get;  set; }
        ICollection<IUserCertificate> Certificates { get;  set; }
        ICollection<ITwoFactorAuthToken> TwoFactorAuthTokens { get;  set; }
        ICollection<IPasswordResetSecret> PasswordResetSecrets { get;  set; }

        IUserClaim CreateUserClaim();
        ILinkedAccount CreateLinkedAccount();
        IUserCertificate CreateUserCertificate();
        ITwoFactorAuthToken CreateTwoFactorAuthToken();
        IPasswordResetSecret CreatePasswordResetSecret();
    }
}
