using System;
using System.Linq;
using MongoDB.Driver.Builders;
using BrockAllen.MembershipReboot.Hierarchical;

namespace BrockAllen.MembershipReboot.MongoDb
{
    public class MongoUserAccountRepository : 
        QueryableUserAccountRepository<HierarchicalUserAccount>
    {
        private readonly MongoDatabase _db;

        public MongoUserAccountRepository(MongoDatabase db)
        {
            _db = db;
        }

        protected override IQueryable<HierarchicalUserAccount> Queryable
        {
            get { return _db.Users().FindAll().AsQueryable(); }
        }

        public override HierarchicalUserAccount Create()
        {
            return new HierarchicalUserAccount();
        }

        public override void Add(HierarchicalUserAccount item)
        {
            _db.Users().Insert(item);
        }

        public override void Update(HierarchicalUserAccount item)
        {
            _db.Users().Save(item);
        }

        public override void Remove(HierarchicalUserAccount item)
        {
            _db.Users().Remove(Query<HierarchicalUserAccount>.EQ(e => e.ID, item.ID));
        }

        public override HierarchicalUserAccount GetByLinkedAccount(string tenant, string provider, string id)
        {
            var query =
                from a in Queryable
                where a.Tenant == tenant
                from la in a.LinkedAccountCollection
                where la.ProviderName == provider && la.ProviderAccountID == id
                select a;
            return query.SingleOrDefault();
        }

        public override HierarchicalUserAccount GetByCertificate(string tenant, string thumbprint)
        {
            var query =
                from a in Queryable
                where a.Tenant == tenant
                from c in a.UserCertificateCollection
                where c.Thumbprint == thumbprint
                select a;
            return query.SingleOrDefault();
        }

        //IUserAccountRepository<HierarchicalUserAccount> This { get { return (IUserAccountRepository<HierarchicalUserAccount>)this; } }


        //UserAccount IUserAccountRepository<UserAccount>.Create()
        //{
        //    return This.Create();
        //}

        //void IUserAccountRepository<UserAccount>.Add(UserAccount item)
        //{
        //    This.Add((HierarchicalUserAccount)item);
        //}

        //void IUserAccountRepository<UserAccount>.Remove(UserAccount item)
        //{
        //    This.Remove((HierarchicalUserAccount)item);
        //}

        //void IUserAccountRepository<UserAccount>.Update(UserAccount item)
        //{
        //    This.Update((HierarchicalUserAccount)item);
        //}

        //UserAccount IUserAccountRepository<UserAccount>.GetByID(Guid id)
        //{
        //    return This.GetByID(id);
        //}

        //UserAccount IUserAccountRepository<UserAccount>.GetByUsername(string username)
        //{
        //    return This.GetByUsername(username);
        //}

        //UserAccount IUserAccountRepository<UserAccount>.GetByUsername(string tenant, string username)
        //{
        //    return This.GetByUsername(tenant, username);
        //}

        //UserAccount IUserAccountRepository<UserAccount>.GetByEmail(string tenant, string email)
        //{
        //    return This.GetByEmail(tenant, email);
        //}

        //UserAccount IUserAccountRepository<UserAccount>.GetByMobilePhone(string tenant, string phone)
        //{
        //    return This.GetByMobilePhone(tenant, phone);
        //}

        //UserAccount IUserAccountRepository<UserAccount>.GetByVerificationKey(string key)
        //{
        //    return This.GetByVerificationKey(key);
        //}

        //UserAccount IUserAccountRepository<UserAccount>.GetByLinkedAccount(string tenant, string provider, string id)
        //{
        //    return This.GetByLinkedAccount(tenant, provider, id);
        //}

        //UserAccount IUserAccountRepository<UserAccount>.GetByCertificate(string tenant, string thumbprint)
        //{
        //    return This.GetByCertificate(tenant, thumbprint);
        //}
    }
}
