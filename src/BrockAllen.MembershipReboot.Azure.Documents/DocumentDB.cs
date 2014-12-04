using BrockAllen.MembershipReboot.Hierarchical;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BrockAllen.MembershipReboot.Azure.Documents
{
    public class DocumentDB
    {
        public IQueryable<HierarchicalGroup> Groups()
        {
            return Client.CreateDocumentQuery<HierarchicalGroup>(Collection.DocumentsLink);
        }

        public void AddGroup(HierarchicalGroup group)
        {
            Client.CreateDocumentAsync(Collection.DocumentsLink, group).Wait();
        }

        public void UpdateGroup(HierarchicalGroup group)
        {
            dynamic doc = Groups().FirstOrDefault(d => d.ID == group.ID);
            
            if (doc != null)
            {
                Client.CreateDocumentAsync(doc.SelfLink, group).Wait();
            }
        }

        public void DeleteGroup(HierarchicalGroup group)
        {
            dynamic doc = Groups().FirstOrDefault(d => d.ID == group.ID);

            if (doc != null)
            {
                Client.DeleteDocumentAsync(doc.SelfLink).Wait();
            }
        }

        public IQueryable<HierarchicalUserAccount> Users()
        {
            return Client.CreateDocumentQuery<HierarchicalUserAccount>(Collection.DocumentsLink);
        }

        public void AddUserAccount(HierarchicalUserAccount userAccounts)
        {
            Client.CreateDocumentAsync(Collection.DocumentsLink, userAccounts).Wait();
        }

        public void UpdateUserAccountp(HierarchicalUserAccount userAccounts)
        {
            dynamic doc = Users().FirstOrDefault(d => d.ID == userAccounts.ID);

            if (doc != null)
            {
                Client.CreateDocumentAsync(doc.SelfLink, userAccounts).Wait();
            }
        }

        public void DeleteUserAccounts(HierarchicalUserAccount userAccounts)
        {
            dynamic doc = Users().FirstOrDefault(d => d.ID == userAccounts.ID);

            if (doc != null)
            {
                Client.DeleteDocumentAsync(doc.SelfLink).Wait();
            }
        }

        private static string databaseId;
        private static String DatabaseId
        {
            get
            {
                if (string.IsNullOrEmpty(databaseId))
                {
                    databaseId = ConfigurationManager.AppSettings["database"];
                }

                return databaseId;
            }
        }

        private static string collectionId;
        private static String CollectionId
        {
            get
            {
                if (string.IsNullOrEmpty(collectionId))
                {
                    collectionId = ConfigurationManager.AppSettings["collection"];
                }

                return collectionId;
            }
        }

        private static Database database;
        private static Database Database
        {
            get
            {
                if (database == null)
                {
                    database = ReadOrCreateDatabase();
                }

                return database;
            }
        }

        private static DocumentCollection collection;
        private static DocumentCollection Collection
        {
            get
            {
                if (collection == null)
                {
                    collection = ReadOrCreateCollection(Database.SelfLink);
                }

                return collection;
            }
        }

        private static DocumentClient client;

        private static DocumentClient Client
        {
            get
            {
                if (client == null)
                {
                    string endpoint = ConfigurationManager.AppSettings["endpoint"];
                    string authKey = ConfigurationManager.AppSettings["authKey"];
                    Uri endpointUri = new Uri(endpoint);
                    client = new DocumentClient(endpointUri, authKey);
                }

                return client;
            }
        }

        private static DocumentCollection ReadOrCreateCollection(string databaseLink)
        {
            var col = Client.CreateDocumentCollectionQuery(databaseLink)
                              .Where(c => c.Id == CollectionId)
                              .AsEnumerable()
                              .FirstOrDefault();

            if (col == null)
            {
                col = Client.CreateDocumentCollectionAsync(databaseLink, new DocumentCollection { Id = CollectionId }).Result;
            }

            return col;
        }

        private static Database ReadOrCreateDatabase()
        {
            var db = Client.CreateDatabaseQuery()
                            .Where(d => d.Id == DatabaseId)
                            .AsEnumerable()
                            .FirstOrDefault();

            if (db == null)
            {
                db = Client.CreateDatabaseAsync(new Database { Id = DatabaseId }).Result;
            }

            return db;
        }
    }
}
