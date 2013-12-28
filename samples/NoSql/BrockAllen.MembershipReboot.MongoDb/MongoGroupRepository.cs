using System;
using System.Linq;
using MongoDB.Driver.Builders;
using System.Collections.Generic;

namespace BrockAllen.MembershipReboot.MongoDb
{
    public class MongoGroupRepository : 
        QueryableGroupRepository<HierarchicalGroup>
    {
        private readonly MongoDatabase _db;

        public MongoGroupRepository(MongoDatabase db)
        {
            _db = db;
        }

        protected override IQueryable<HierarchicalGroup> Queryable
        {
            get { return _db.Groups().FindAll().AsQueryable(); }
        }

        public override HierarchicalGroup Create()
        {
            return new HierarchicalGroup();
        }

        public override void Add(HierarchicalGroup item)
        {
            _db.Groups().Insert(item);
        }

        public override void Remove(HierarchicalGroup item)
        {
            _db.Groups().Remove(Query<HierarchicalGroup>.EQ(e => e.ID, item.ID));
        }

        public override void Update(HierarchicalGroup item)
        {
            _db.Groups().Save(item);
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
