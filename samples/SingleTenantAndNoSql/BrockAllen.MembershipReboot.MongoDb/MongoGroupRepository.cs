using System;
using System.Linq;
using MongoDB.Driver.Builders;

namespace BrockAllen.MembershipReboot.MongoDb
{
    public class MongoGroupRepository : IGroupRepository
    {
        private readonly MongoDatabase _db;

        public MongoGroupRepository(MongoDatabase db)
        {
            _db = db;
        }

        public Group Create()
        {
            return new Group();
        }

        public IQueryable<Group> GetAll()
        {
            return _db.Groups().FindAll().AsQueryable();
        }

        public Group Get(Guid key)
        {
            return _db.Groups().FindOne(Query<Group>.EQ(e => e.ID, key));
        }

        public void Add(Group item)
        {
            _db.Groups().Insert(item);
        }

        public void Update(Group item)
        {
            _db.Groups().Save(item);
        }

        public void Remove(Group item)
        {
            _db.Groups().Remove(Query<Group>.EQ(e => e.ID, item.ID));
        }

        public void Dispose()
        {
        }
    }
}
