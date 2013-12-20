/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


using BrockAllen.MembershipReboot.Relational;
namespace BrockAllen.MembershipReboot.Ef
{
    public class DefaultUserAccountRepository
           : DbContextUserAccountRepository<DefaultMembershipRebootDatabase, RelationalUserAccount>, 
             IUserAccountRepository
    {
        public DefaultUserAccountRepository()
        {
        }

        public DefaultUserAccountRepository(string name)
            : base(new DefaultMembershipRebootDatabase(name))
        {
        }

        IUserAccountRepository<RelationalUserAccount> This { get { return (IUserAccountRepository<RelationalUserAccount>)this; } }

        public new UserAccount Create()
        {
            return This.Create();
        }

        public void Add(UserAccount item)
        {
            This.Add((RelationalUserAccount)item);
        }

        public void Remove(UserAccount item)
        {
            This.Remove((RelationalUserAccount)item);
        }

        public void Update(UserAccount item)
        {
            This.Update((RelationalUserAccount)item);
        }

        public new UserAccount GetByID(System.Guid id)
        {
            return This.GetByID(id);
        }

        public new UserAccount GetByUsername(string username)
        {
            return This.GetByUsername(username);
        }

        UserAccount IUserAccountRepository<UserAccount>.GetByUsername(string tenant, string username)
        {
            return This.GetByUsername(tenant, username);
        }

        public new UserAccount GetByEmail(string tenant, string email)
        {
            return This.GetByEmail(tenant, email);
        }

        public new UserAccount GetByMobilePhone(string tenant, string phone)
        {
            return This.GetByMobilePhone(tenant, phone);
        }

        public new UserAccount GetByVerificationKey(string key)
        {
            return This.GetByVerificationKey(key);
        }

        public new UserAccount GetByLinkedAccount(string tenant, string provider, string id)
        {
            return This.GetByLinkedAccount(tenant, provider, id);
        }

        public new UserAccount GetByCertificate(string tenant, string thumbprint)
        {
            return This.GetByCertificate(tenant, thumbprint);
        }
    }
}
