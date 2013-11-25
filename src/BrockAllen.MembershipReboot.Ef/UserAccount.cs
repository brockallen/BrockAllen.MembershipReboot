using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace BrockAllen.MembershipReboot.Ef
{
    public class UserAccount : IUserAccount
    {
        [Key]
        public virtual Guid ID { get; set; }

        [StringLength(50)]
        [Required]
        public virtual string Tenant { get; set; }
        [StringLength(100)]
        [Required]
        public virtual string Username { get; set; }
        [EmailAddress]
        [StringLength(100)]
        [Required]
        public virtual string Email { get; set; }

        public virtual DateTime Created { get; set; }
        public virtual DateTime LastUpdated { get; set; }
        public virtual DateTime PasswordChanged { get; set; }
        public virtual bool RequiresPasswordReset { get; set; }

        public virtual DateTime? LastFailedPasswordReset { get; set; }
        public virtual int FailedPasswordResetCount { get; set; }

        [StringLength(100)]
        public virtual string MobileCode { get; set; }
        public virtual DateTime? MobileCodeSent { get; set; }
        [StringLength(20)]
        public virtual string MobilePhoneNumber { get; set; }
        public virtual DateTime? MobilePhoneNumberChanged { get; set; }

        public virtual TwoFactorAuthMode AccountTwoFactorAuthMode { get; set; }
        public virtual TwoFactorAuthMode CurrentTwoFactorAuthStatus { get; set; }

        public virtual bool IsAccountVerified { get; set; }
        public virtual bool IsLoginAllowed { get; set; }
        public virtual bool IsAccountClosed { get; set; }
        public virtual DateTime? AccountClosed { get; set; }

        public virtual DateTime? LastLogin { get; set; }
        public virtual DateTime? LastFailedLogin { get; set; }
        public virtual int FailedLoginCount { get; set; }

        [StringLength(100)]
        public virtual string VerificationKey { get; set; }
        public virtual VerificationKeyPurpose? VerificationPurpose { get; set; }
        public virtual DateTime? VerificationKeySent { get; set; }
        [StringLength(100)]
        public virtual string VerificationStorage { get; set; }

        [Required]
        [StringLength(200)]
        public virtual string HashedPassword { get; set; }

        public ICollection<IUserClaim> Claims { get; set; }
        public ICollection<ILinkedAccount> LinkedAccounts { get; set; }
        public ICollection<IUserCertificate> Certificates { get; set; }
        public ICollection<ITwoFactorAuthToken> TwoFactorAuthTokens { get; set; }
        public ICollection<IPasswordResetSecret> PasswordResetSecrets { get; set; }


        public IUserClaim CreateUserClaim()
        {
            return new UserClaim();
        }

        public ILinkedAccount CreateLinkedAccount()
        {
            return new LinkedAccount();
        }

        public IUserCertificate CreateUserCertificate()
        {
            return new UserCertificate();
        }

        public ITwoFactorAuthToken CreateTwoFactorAuthToken()
        {
            return new TwoFactorAuthToken();
        }

        public IPasswordResetSecret CreatePasswordResetSecret()
        {
            return new PasswordResetSecret();
        }
    }
}
