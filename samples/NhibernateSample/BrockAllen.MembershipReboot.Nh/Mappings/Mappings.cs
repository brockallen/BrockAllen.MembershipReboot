namespace BrockAllen.MembershipReboot.Nh.Mappings
{
    using BrockAllen.MembershipReboot.Nh;

    using global::NHibernate.Mapping.ByCode;
    using global::NHibernate.Mapping.ByCode.Conformist;
    using global::NHibernate.Type;

    public class LinkedAccountMapping : ClassMapping<NhLinkedAccount>
    {
        public LinkedAccountMapping()
        {
            this.Table("LinkedAccounts");
            this.ComposedId(
                pm =>
                {
                    pm.ManyToOne(
                            x => x.Account,
                            mm =>
                            {
                                mm.Column("UserAccountId");
                                mm.ForeignKey("FK_UserAccount_UserClaims");
                                mm.NotNullable(true);
                            });
                    pm.Property(x => x.ProviderName, m => m.Length(30));
                    pm.Property(x => x.ProviderAccountID, m => m.Length(100));
                });
            this.Property(x => x.LastLogin);
            this.Version(
                x => x.Version,
                vm =>
                    {
                        vm.Generated(VersionGeneration.Never);
                        vm.Type(new Int64Type());
                    });

            this.DynamicInsert(true);
            this.DynamicUpdate(true);
        }
    }

    public class LinkedAccountClaimMapping : ClassMapping<NhLinkedAccountClaim>
    {
        public LinkedAccountClaimMapping()
        {
            this.Table("LinkedAccountClaims");
            this.ComposedId(
                pm =>
                {
                    pm.ManyToOne(
                            x => x.Account,
                            mm =>
                            {
                                mm.Column("UserAccountId");
                                mm.ForeignKey("FK_UserAccount_UserClaims");
                                mm.NotNullable(true);
                            });
                    pm.Property(x => x.ProviderName, m => m.Length(30));
                    pm.Property(x => x.ProviderAccountID, m => m.Length(100));
                    pm.Property(x => x.Type, m => m.Length(150));
                    pm.Property(x => x.Value, m => m.Length(150));
                });
            this.Version(
                x => x.Version,
                vm =>
                {
                    vm.Generated(VersionGeneration.Never);
                    vm.Type(new Int64Type());
                });

            this.DynamicInsert(true);
            this.DynamicUpdate(true);
        }
    }

    public class PasswordResetSecretMapping : ClassMapping<NhPasswordResetSecret>
    {
        public PasswordResetSecretMapping()
        {
            this.Table("PasswordResetSecrets");
            this.ComposedId(
                pm =>
                    {
                        pm.ManyToOne(
                            x => x.Account,
                            mm =>
                            {
                                mm.Column("UserAccountId");
                                mm.ForeignKey("FK_UserAccount_UserClaims");
                                mm.NotNullable(true);
                            });
                        pm.Property(x => x.PasswordResetSecretID);
                    });
            this.Property(
                x => x.Question,
                pm =>
                {
                    pm.NotNullable(true);
                    pm.Length(150);
                });
            this.Property(
                x => x.Answer,
                pm =>
                {
                    pm.NotNullable(true);
                    pm.Length(150);
                });
            this.Version(
                x => x.Version,
                vm =>
                {
                    vm.Generated(VersionGeneration.Never);
                    vm.Type(new Int64Type());
                });

            this.DynamicInsert(true);
            this.DynamicUpdate(true);
        }
    }

    public class TwoFactorAuthTokenMapping : ClassMapping<NhTwoFactorAuthToken>
    {
        public TwoFactorAuthTokenMapping()
        {
            this.Table("TwoFactorAuthTokens");
            this.ComposedId(
                pm =>
                    {
                        pm.ManyToOne(
                            x => x.Account,
                            mm =>
                            {
                                mm.Column("UserAccountId");
                                mm.ForeignKey("FK_UserAccount_UserClaims");
                                mm.NotNullable(true);
                            });
                        pm.Property(x => x.Token, p => p.Length(100));
                    });
            this.Property(x => x.Issued);
            this.Version(
                x => x.Version,
                vm =>
                {
                    vm.Generated(VersionGeneration.Never);
                    vm.Type(new Int64Type());
                });

            this.DynamicInsert(true);
            this.DynamicUpdate(true);
        }
    }

    public class UserAccountMapping : ClassMapping<NhUserAccount>
    {
        public UserAccountMapping()
        {
            this.Table("UserAccounts");
            this.Id(x => x.ID, idm => idm.Generator(Generators.Assigned));
            this.Property(
                x => x.Tenant,
                pm =>
                {
                    pm.NotNullable(true);
                    pm.Length(50);
                });
            this.Property(
                x => x.Username,
                pm =>
                {
                    pm.NotNullable(true);
                    pm.Length(100);
                });
            this.Property(x => x.Created);
            this.Property(x => x.LastUpdated);
            this.Property(x => x.IsAccountClosed);
            this.Property(x => x.AccountClosed);
            this.Property(x => x.IsLoginAllowed);
            this.Property(x => x.LastLogin);
            this.Property(x => x.LastFailedLogin);
            this.Property(x => x.FailedLoginCount);
            this.Property(x => x.PasswordChanged);
            this.Property(x => x.RequiresPasswordReset);
            this.Property(x => x.Email, pm => pm.Length(100));
            this.Property(x => x.IsAccountVerified);
            this.Property(x => x.LastFailedPasswordReset);
            this.Property(x => x.FailedPasswordResetCount);
            this.Property(x => x.MobileCode, pm => pm.Length(100));
            this.Property(x => x.MobileCodeSent);
            this.Property(x => x.MobilePhoneNumber, pm => pm.Length(20));
            this.Property(x => x.MobilePhoneNumberChanged);
            this.Property(x => x.AccountTwoFactorAuthMode);
            this.Property(x => x.CurrentTwoFactorAuthStatus);
            this.Property(x => x.VerificationKey, pm => pm.Length(100));
            this.Property(x => x.VerificationPurpose);
            this.Property(x => x.VerificationKeySent);
            this.Property(x => x.VerificationStorage, pm => pm.Length(100));
            this.Property(x => x.HashedPassword, pm => pm.Length(100));
            this.Version(
                x => x.Version,
                vm =>
                {
                    vm.Generated(VersionGeneration.Never);
                    vm.Type(new Int64Type());
                });

            this.Set(
                x => x.ClaimsCollection,
                spm =>
                    {
                        spm.Inverse(true);
                        spm.Cascade(Cascade.All);
                        spm.Key(
                            km =>
                                {
                                    km.Column("UserAccountId");
                                    km.ForeignKey("FK_UserAccount_UserClaims");
                                });
                    },
                r => r.OneToMany());
            this.Set(
                x => x.LinkedAccountsCollection,
                spm =>
                {
                    spm.Inverse(true);
                    spm.Cascade(Cascade.All);
                    spm.Key(
                        km =>
                        {
                            km.Column("UserAccountId");
                            km.ForeignKey("FK_UserAccount_LinkedAccounts");
                        });
                },
                r => r.OneToMany());
            this.Set(
                x => x.LinkedAccountClaimsCollection,
                spm =>
                {
                    spm.Inverse(true);
                    spm.Cascade(Cascade.All);
                    spm.Key(
                        km =>
                        {
                            km.Column("UserAccountId");
                            km.ForeignKey("FK_UserAccount_LinkedAccountClaims");
                        });
                },
                r => r.OneToMany());
            this.Set(
                x => x.CertificatesCollection,
                spm =>
                {
                    spm.Inverse(true);
                    spm.Cascade(Cascade.All);
                    spm.Key(
                        km =>
                        {
                            km.Column("UserAccountId");
                            km.ForeignKey("FK_UserAccount_UserCertificates");
                        });
                },
                r => r.OneToMany());
            this.Set(
                x => x.TwoFactorAuthTokensCollection,
                spm =>
                {
                    spm.Inverse(true);
                    spm.Cascade(Cascade.All);
                    spm.Key(
                        km =>
                        {
                            km.Column("UserAccountId");
                            km.ForeignKey("FK_UserAccount_TwoFactorAuthTokens");
                        });
                },
                r => r.OneToMany());
            this.Set(
                x => x.PasswordResetSecretsCollection,
                spm =>
                {
                    spm.Inverse(true);
                    spm.Cascade(Cascade.All);
                    spm.Key(
                        km =>
                        {
                            km.Column("UserAccountId");
                            km.ForeignKey("FK_UserAccount_PasswordResetSecrets");
                        });
                },
                r => r.OneToMany());
            this.DynamicInsert(true);
            this.DynamicUpdate(true);
        }
    }

    public class UserCertificateMapping : ClassMapping<NhUserCertificate>
    {
        public UserCertificateMapping()
        {
            this.Table("UserCertificates");
            this.ComposedId(
                pm =>
                    {
                        pm.ManyToOne(
                            x => x.Account,
                            mm =>
                                {
                                    mm.Column("UserAccountId");
                                    mm.ForeignKey("FK_UserAccount_UserCertificates");
                                    mm.NotNullable(true);
                                });
                        pm.Property(x => x.Thumbprint, p => p.Length(150));
                    });
            this.Property(x => x.Subject, pm => pm.Length(250));
            this.Version(
                x => x.Version,
                vm =>
                {
                    vm.Generated(VersionGeneration.Never);
                    vm.Type(new Int64Type());
                });

            this.DynamicInsert(true);
            this.DynamicUpdate(true);
        }
    }

    public class UserClaimMapping : ClassMapping<NhUserClaim>
    {
        public UserClaimMapping()
        {
            this.Table("UserClaims");
            this.ComposedId(
                pm =>
                    {
                        pm.ManyToOne(
                            x => x.Account,
                            mm =>
                                {
                                    mm.Column("UserAccountId");
                                    mm.ForeignKey("FK_UserAccount_UserClaims");
                                    mm.NotNullable(true);
                                });
                        pm.Property(x => x.Type, p => p.Length(150));
                        pm.Property(x => x.Value, p => p.Length(150));
                    });
            this.Version(
                x => x.Version,
                vm =>
                    {
                        vm.Generated(VersionGeneration.Never);
                        vm.Type(new Int64Type());
                    });

            this.DynamicInsert(true);
            this.DynamicUpdate(true);
        }
    }
}