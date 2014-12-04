using System.Linq;
using BrockAllen.MembershipReboot.Hierarchical;

namespace BrockAllen.MembershipReboot.Azure.Documents
{
    public class DocumentDBAccountRepository : 
        QueryableUserAccountRepository<HierarchicalUserAccount>
    {
        private readonly DocumentDB _db;

        public DocumentDBAccountRepository(DocumentDB db)
        {
            this.UseEqualsOrdinalIgnoreCaseForQueries = true;
            _db = db;
        }

        protected override IQueryable<HierarchicalUserAccount> Queryable
        {
            get { return _db.Users(); }
        }

        public override HierarchicalUserAccount Create()
        {
            return new HierarchicalUserAccount();
        }

        public override void Add(HierarchicalUserAccount item)
        {
            _db.AddUserAccount(item);
        }

        public override void Update(HierarchicalUserAccount item)
        {
            _db.UpdateUserAccountp(item);
        }

        public override void Remove(HierarchicalUserAccount item)
        {
            _db.DeleteUserAccounts(item);
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
    }
}
