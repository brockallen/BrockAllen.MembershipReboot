/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


using BrockAllen.MembershipReboot.Relational;
namespace BrockAllen.MembershipReboot.Ef
{
    public class DefaultUserAccountRepository
           : DbContextUserAccountRepository<DefaultMembershipRebootDatabase, RelationalUserAccount>, IUserAccountRepository
    {
        public DefaultUserAccountRepository()
        {
        }

        public DefaultUserAccountRepository(string name)
            : base(new DefaultMembershipRebootDatabase(name))
        {
        }

        IUserAccountRepository<RelationalUserAccount> This { get { return (IUserAccountRepository<RelationalUserAccount>)this; } }

        System.Linq.IQueryable<UserAccount> IRepository<UserAccount>.GetAll()
        {
            return This.GetAll();
        }

        UserAccount IRepository<UserAccount>.Get(System.Guid key)
        {
            return This.Get(key);
        }

        UserAccount IRepository<UserAccount>.Create()
        {
            return This.Create();
        }

        void IRepository<UserAccount>.Add(UserAccount item)
        {
            This.Add((RelationalUserAccount)item);
        }

        void IRepository<UserAccount>.Remove(UserAccount item)
        {
            This.Remove((RelationalUserAccount)item);
        }

        void IRepository<UserAccount>.Update(UserAccount item)
        {
            This.Update((RelationalUserAccount)item);
        }

        void System.IDisposable.Dispose()
        {
            base.Dispose();
        }
    }
}
