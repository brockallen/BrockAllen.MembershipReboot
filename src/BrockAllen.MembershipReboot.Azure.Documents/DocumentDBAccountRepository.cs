using System;

using BrockAllen.MembershipReboot.Hierarchical;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Azure.Documents.Linq;
using BrockAllen.MembershipReboot.Hierarchical;

namespace BrockAllen.MembershipReboot.Azure.Documents
{
    public class DocumentDBAccountRepository<TAccount> : 
        QueryableUserAccountRepository<TAccount>
        where TAccount : HierarchicalUserAccount, new()
    {
        private readonly DocumentDB _db;

        public DocumentDBAccountRepository(DocumentDB db)
        {
            this.UseEqualsOrdinalIgnoreCaseForQueries = true;
            _db = db;
        }

        protected override IQueryable<TAccount> Queryable
        {
            get { return DocumentDB.Client.CreateDocumentQuery<TAccount>(DocumentDB.Collection.DocumentsLink); }
        }

        public override TAccount Create()
        {
            return new TAccount();
        }

        public override void Add(TAccount item)
        {
            DocumentDB.Client.CreateDocumentAsync(DocumentDB.Collection.DocumentsLink, item).Wait();
        }

        public override void Update(TAccount item)
        {
            dynamic doc = this.Queryable.FirstOrDefault(d => d.ID == item.ID);

            if (doc != null)
            {
                DocumentDB.Client.CreateDocumentAsync(doc.SelfLink, item).Wait();
            }
        }

        public override void Remove(TAccount item)
        {
            dynamic doc = this.Queryable.FirstOrDefault(d => d.ID == item.ID);

            if (doc != null)
            {
                DocumentDB.Client.DeleteDocumentAsync(doc.SelfLink).Wait();
            }
        }

        public override TAccount GetByLinkedAccount(string tenant, string provider, string id)
        {
            var query =
                from a in Queryable
                where a.Tenant == tenant
                from la in a.LinkedAccountCollection
                where la.ProviderName == provider && la.ProviderAccountID == id
                select a;
            return query.SingleOrDefault();
        }

        public override TAccount GetByCertificate(string tenant, string thumbprint)
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
