using System;
using System.Linq;
using MongoDB.Driver.Builders;

namespace BrockAllen.MembershipReboot.MongoDb
{
    public class MongoUserAccountRepository : IUserAccountRepository
    {
        private readonly MongoDatabase _db;

        public MongoUserAccountRepository(MongoDatabase db)
        {
            _db = db;
        }

        public UserAccount Create()
        {
            return new UserAccount();
        }
        
        public IQueryable<UserAccount> GetAll()
        {
            return _db.Users().FindAll().AsQueryable();
        }

        public UserAccount Get(params object[] keys)
        {
            return _db.Users().FindOne(Query<UserAccount>.EQ(e => e.ID, (Guid)keys[0]));
        }

        public void Add(UserAccount item)
        {
            _db.Users().Insert(item);
        }

        public void Update(UserAccount item)
        {
            _db.Users().Save(item);
        }

        public void Remove(UserAccount item)
        {
            _db.Users().Remove(Query<UserAccount>.EQ(e => e.ID, item.ID));
        }

        public void Dispose()
        {
        }

        public UserAccount FindByLinkedAccount(string tenant, string provider, string id) 
        {
            if (String.IsNullOrWhiteSpace(tenant)) return null;
            if (String.IsNullOrWhiteSpace(provider)) return null;
            if (String.IsNullOrWhiteSpace(id)) return null;
            
            var query =
                from u in GetAll()
                where u.Tenant == tenant
                from l in u.LinkedAccounts
                where l.ProviderName == provider && l.ProviderAccountID == id
                select u;

            return query.SingleOrDefault();
        }
    }
}
