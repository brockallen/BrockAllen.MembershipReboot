/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Relational;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Entity.Core.Objects.DataClasses;

namespace System.Data.Entity
{
    public static class DbModelBuilderExtensions
    {
        public static void RegisterUserAccountChildTablesForDelete<TKey, TAccount, TUserClaim, TLinkedAccount, TLinkedAccountClaim, TPasswordResetSecret, TTwoFactorAuthToken, TUserCertificate>(this DbContext ctx)
            where TAccount : RelationalUserAccount<TKey, TUserClaim, TLinkedAccount, TLinkedAccountClaim, TPasswordResetSecret, TTwoFactorAuthToken, TUserCertificate>
            where TUserClaim : RelationalUserClaim<TKey>, new()
            where TLinkedAccount : RelationalLinkedAccount<TKey>, new()
            where TLinkedAccountClaim : RelationalLinkedAccountClaim<TKey>, new()
            where TPasswordResetSecret : RelationalPasswordResetSecret<TKey>, new()
            where TTwoFactorAuthToken : RelationalTwoFactorAuthToken<TKey>, new()
            where TUserCertificate : RelationalUserCertificate<TKey>, new()
        {
            ctx.Set<TAccount>().Local.CollectionChanged +=
                delegate(object sender, NotifyCollectionChangedEventArgs e)
                {
                    if (e.Action == NotifyCollectionChangedAction.Add)
                    {
                        foreach (TAccount account in e.NewItems)
                        {
                            account.ClaimCollection.RegisterDeleteOnRemove(ctx);
                            account.LinkedAccountClaimCollection.RegisterDeleteOnRemove(ctx);
                            account.LinkedAccountCollection.RegisterDeleteOnRemove(ctx);
                            account.PasswordResetSecretCollection.RegisterDeleteOnRemove(ctx);
                            account.TwoFactorAuthTokenCollection.RegisterDeleteOnRemove(ctx);
                            account.UserCertificateCollection.RegisterDeleteOnRemove(ctx);
                        }
                    }
                };
        }

        public static void RegisterUserAccountChildTablesForDelete<TAccount>(this DbContext ctx)
            where TAccount : RelationalUserAccount
        {
            ctx.RegisterUserAccountChildTablesForDelete<int, TAccount, RelationalUserClaim, RelationalLinkedAccount, RelationalLinkedAccountClaim, RelationalPasswordResetSecret, RelationalTwoFactorAuthToken, RelationalUserCertificate>();
        }

        public static void RegisterGroupChildTablesForDelete<TGroup>(this DbContext ctx)
            where TGroup : RelationalGroup
        {
            ctx.Set<TGroup>().Local.CollectionChanged +=
                delegate(object sender, NotifyCollectionChangedEventArgs e)
                {
                    if (e.Action == NotifyCollectionChangedAction.Add)
                    {
                        foreach (TGroup group in e.NewItems)
                        {
                            group.ChildrenCollection.RegisterDeleteOnRemove(ctx);
                        }
                    }
                };
        }

        internal static void RegisterDeleteOnRemove<TChild>(this ICollection<TChild> collection, DbContext ctx)
            where TChild : class
        {
            var entities = collection as EntityCollection<TChild>;
            if (entities != null)
            {
                entities.AssociationChanged += delegate(object sender, CollectionChangeEventArgs e)
                {
                    if (e.Action == CollectionChangeAction.Remove)
                    {
                        var entity = e.Element as TChild;
                        if (entity != null)
                        {
                            ctx.Entry<TChild>(entity).State = EntityState.Deleted;
                        }
                    }
                };
            }
        }

        public static void ConfigureMembershipRebootUserAccounts<TKey, TAccount, TUserClaim, TLinkedAccount, TLinkedAccountClaim, TPasswordResetSecret, TTwoFactorAuthToken, TUserCertificate>(this DbModelBuilder modelBuilder)
            where TAccount : RelationalUserAccount<TKey, TUserClaim, TLinkedAccount, TLinkedAccountClaim, TPasswordResetSecret, TTwoFactorAuthToken, TUserCertificate>
            where TUserClaim : RelationalUserClaim<TKey>, new()
            where TLinkedAccount : RelationalLinkedAccount<TKey>, new()
            where TLinkedAccountClaim : RelationalLinkedAccountClaim<TKey>, new()
            where TPasswordResetSecret : RelationalPasswordResetSecret<TKey>, new()
            where TTwoFactorAuthToken : RelationalTwoFactorAuthToken<TKey>, new()
            where TUserCertificate : RelationalUserCertificate<TKey>, new()
        {
            modelBuilder.Entity<TAccount>()
                .HasKey(x => x.Key).ToTable("UserAccounts");

            modelBuilder.Entity<TAccount>().HasMany(x => x.PasswordResetSecretCollection)
                .WithRequired().HasForeignKey(x => x.ParentKey).WillCascadeOnDelete();
            modelBuilder.Entity<TPasswordResetSecret>()
                .HasKey(x => x.Key).ToTable("PasswordResetSecrets");

            modelBuilder.Entity<TAccount>().HasMany(x => x.TwoFactorAuthTokenCollection)
                .WithRequired().HasForeignKey(x => x.ParentKey).WillCascadeOnDelete();
            modelBuilder.Entity<TTwoFactorAuthToken>()
                .HasKey(x => x.Key).ToTable("TwoFactorAuthTokens");

            modelBuilder.Entity<TAccount>().HasMany(x => x.UserCertificateCollection)
                .WithRequired().HasForeignKey(x => x.ParentKey).WillCascadeOnDelete();
            modelBuilder.Entity<TUserCertificate>()
                .HasKey(x => x.Key).ToTable("UserCertificates");

            modelBuilder.Entity<TAccount>().HasMany(x => x.ClaimCollection)
                .WithRequired().HasForeignKey(x => x.ParentKey).WillCascadeOnDelete();
            modelBuilder.Entity<TUserClaim>()
                .HasKey(x => x.Key).ToTable("UserClaims");

            modelBuilder.Entity<TAccount>().HasMany(x => x.LinkedAccountCollection)
                .WithRequired().HasForeignKey(x => x.ParentKey).WillCascadeOnDelete();
            modelBuilder.Entity<TLinkedAccount>()
                .HasKey(x => x.Key).ToTable("LinkedAccounts");

            modelBuilder.Entity<TAccount>().HasMany(x => x.LinkedAccountClaimCollection)
                .WithRequired().HasForeignKey(x => x.ParentKey).WillCascadeOnDelete();
            modelBuilder.Entity<TLinkedAccountClaim>()
                .HasKey(x => x.Key).ToTable("LinkedAccountClaims");
        }
        
        public static void ConfigureMembershipRebootUserAccounts<TAccount>(this DbModelBuilder modelBuilder)
            where TAccount : RelationalUserAccount
        {
            modelBuilder.ConfigureMembershipRebootUserAccounts<int, TAccount, RelationalUserClaim, RelationalLinkedAccount, RelationalLinkedAccountClaim, RelationalPasswordResetSecret, RelationalTwoFactorAuthToken, RelationalUserCertificate>();
        }

        public static void ConfigureMembershipRebootGroups<TGroup>(this DbModelBuilder modelBuilder)
            where TGroup : RelationalGroup
        {
            modelBuilder.Entity<TGroup>()
                .HasKey(x => x.Key).ToTable("Groups");

            modelBuilder.Entity<TGroup>().HasMany(x => x.ChildrenCollection)
                .WithRequired().HasForeignKey(x=>x.ParentKey).WillCascadeOnDelete();
            modelBuilder.Entity<RelationalGroupChild>()
                .HasKey(x => x.Key).ToTable("GroupChilds");
        }
    }
}
