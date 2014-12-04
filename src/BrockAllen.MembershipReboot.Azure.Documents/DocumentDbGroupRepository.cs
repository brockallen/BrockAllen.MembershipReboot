using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Azure.Documents.Linq;

namespace BrockAllen.MembershipReboot.Azure.Documents
{
    public class DocumentDbGroupRepository<TGroup> : 
        QueryableGroupRepository<TGroup>
        where TGroup : HierarchicalGroup, new()
    {

        public DocumentDbGroupRepository()
        {
        }

        protected override IQueryable<TGroup> Queryable
        {
            get { return DocumentDB.Client.CreateDocumentQuery<TGroup>(DocumentDB.Collection.DocumentsLink); }
        }

        public override TGroup Create()
        {
            return new TGroup();
        }

        public override void Add(TGroup item)
        {
            DocumentDB.Client.CreateDocumentAsync(DocumentDB.Collection.DocumentsLink, item).Wait();
        }

        public override void Remove(TGroup item)
        {
            dynamic doc = this.Queryable.FirstOrDefault(d => d.ID == item.ID);

            if (doc != null)
            {
                DocumentDB.Client.DeleteDocumentAsync(doc.SelfLink).Wait();
            }
        }

        public override void Update(TGroup item)
        {
            dynamic doc = this.Queryable.FirstOrDefault(d => d.ID == item.ID);

            if (doc != null)
            {
                DocumentDB.Client.CreateDocumentAsync(doc.SelfLink, item).Wait();
            }
        }

        public override IEnumerable<TGroup> GetByChildID(Guid childGroupID)
        {
            var q =
                from g in Queryable
                from c in g.Children
                where c.ChildGroupID == childGroupID
                select g;
            return q;
        }
    }
}
