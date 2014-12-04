using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Azure.Documents.Linq;

namespace BrockAllen.MembershipReboot.Azure.Documents
{
    public class DocumentDbGroupRepository : 
        QueryableGroupRepository<HierarchicalGroup>
    {
        private readonly DocumentDB _db;

        public DocumentDbGroupRepository(DocumentDB db)
        {
            _db = db;
        }

        protected override IQueryable<HierarchicalGroup> Queryable
        {
            get { return _db.Groups(); }
        }

        public override HierarchicalGroup Create()
        {
            return new HierarchicalGroup();
        }

        public override void Add(HierarchicalGroup item)
        {
            _db.AddGroup(item);
        }

        public override void Remove(HierarchicalGroup item)
        {
            _db.DeleteGroup(item);
        }

        public override void Update(HierarchicalGroup item)
        {
            _db.UpdateGroup(item);
        }

        public override IEnumerable<HierarchicalGroup> GetByChildID(Guid childGroupID)
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
