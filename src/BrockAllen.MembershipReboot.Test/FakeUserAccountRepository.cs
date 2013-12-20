using BrockAllen.MembershipReboot.Hierarchical;
using System.Collections.Generic;
using System.Linq;

namespace BrockAllen.MembershipReboot.Test
{
    public class MyUserAccount : HierarchicalUserAccount 
    {
        protected override void AddClaim(UserClaim item)
        {
            base.AddClaim(item);
        }
    }

    public class FakeUserAccountRepository : QueryableUserAccountRepository<UserAccount>, IUserAccountRepository
    {
        public List<UserAccount> UserAccounts = new List<UserAccount>();

        protected override IQueryable<UserAccount> Queryable
        {
            get { return UserAccounts.AsQueryable(); }
        }

        public override UserAccount Create()
        {
            return new MyUserAccount();
        }

        public override void Add(UserAccount item)
        {
            UserAccounts.Add(item);
        }

        public override void Remove(UserAccount item)
        {
            UserAccounts.Remove(item);
        }

        public override void Update(UserAccount item)
        {
        }

        public override UserAccount GetByLinkedAccount(string tenant, string provider, string id)
        {
            var query = 
                from a in UserAccounts
                where a.Tenant == tenant
                from la in a.LinkedAccounts
                where la.ProviderName == provider && la.ProviderAccountID == id
                select a;
            return query.SingleOrDefault();
        }

        public override UserAccount GetByCertificate(string tenant, string thumbprint)
        {
            var query =
                from a in UserAccounts
                where a.Tenant == tenant
                from c in a.Certificates
                where c.Thumbprint == thumbprint
                select a;
            return query.SingleOrDefault();
        }
    }
}