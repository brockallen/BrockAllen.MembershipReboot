using System.Configuration;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using BrockAllen.MembershipReboot.Hierarchical;

namespace BrockAllen.MembershipReboot.MongoDb
{
    public class MongoDatabase
    {
        static MongoDatabase()
        {
            BsonClassMap.RegisterClassMap<Group>(cm =>
            {
                cm.AutoMap();
                cm.MapIdProperty(c => c.ID);
            });

            BsonClassMap.RegisterClassMap<UserAccount>(cm =>
            {
                cm.AutoMap();
                cm.MapIdProperty(c => c.ID);
            });

            BsonClassMap.RegisterClassMap<HierarchicalUserAccount>(cm => cm.AutoMap());
        }

        private readonly string _connectionStringName;

        public MongoDatabase(string connectionStringName)
        {
            _connectionStringName = connectionStringName;
        }

        public MongoCollection<Group> Groups()
        {
            return GetCollection<Group>("groups");
        }

        public MongoCollection<HierarchicalUserAccount> Users()
        {
            return GetCollection<HierarchicalUserAccount>("users");
        }

        public MongoCollection<T> GetCollection<T>(string name)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[_connectionStringName].ConnectionString;
            var databaseName = MongoUrl.Create(connectionString).DatabaseName;
            var client = new MongoClient(connectionString);
            var server = client.GetServer();
            var database = server.GetDatabase(databaseName);
            return database.GetCollection<T>(name);
        }
    }
}
