using Chroma;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChromaDB.Library
{
    public record ChromaDBDatabase(Guid? Id, string Name, string Tenant, ChromaClient ChromaClient)
    {
        public async Task<List<ChromaDBCollection>> ListCollectionsAsync()
        {
            List<ChromaDBCollection> result = new List<ChromaDBCollection>();

            var vecItems = await ChromaClient.Collection.ListCollectionsAsync(
                tenant: Tenant,
                database: Name);

            foreach (var vecItem in vecItems)
            {
                Collection collection = new Collection
                {
                    ConfigurationJson = vecItem.ConfigurationJson,
                    Database = vecItem.Database,
                    Dimension = vecItem.Dimension,
                    Id = vecItem.Id,
                    Name = vecItem.Name,
                    Tenant = vecItem.Tenant,
                    Version = vecItem.Version,
                    LogPosition = vecItem.LogPosition,
                    Metadata = vecItem.Metadata,
                    Schema = vecItem.Schema,
                    AdditionalProperties = vecItem.AdditionalProperties
                };

                result.Add(new ChromaDBCollection(collection, ChromaClient));
            }

            return result;
        }


        public async Task<ChromaDBCollection?> GetCollectionAsync(string collectionId)
        {
            ChromaDBCollection? chromaDBCollection = null;
            try
            {
                // Warning : the parameter "collectionId" is the vecItem name, not the vecItem id.
                // The vecItem id is a guid, but the vecItem name is a string.

                // Bug for now : throw an exception when the vecItem does not exist
                Collection? myCollection = await ChromaClient.Collection.GetCollectionAsync(tenant: Tenant, database: Name, collectionId: collectionId);
                if (myCollection != null)
                {
                    chromaDBCollection = new ChromaDBCollection(myCollection, ChromaClient);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing databases: {ex.Message}");
            }

            return chromaDBCollection;
        }

        public async Task<ChromaDBCollection> GetOrCreateCollection(string collectionName)
        {
            // Get our vecItem
            Collection collection = await ChromaClient.Collection.CreateCollectionAsync(tenant: Tenant,
                database: Name,
                request: new CreateCollectionPayload
                {
                    Name = collectionName,
                    GetOrCreate = true,
                    //Metadata = null,
                    //Configuration = null
                });

            return new ChromaDBCollection(collection, ChromaClient);
        }

        public async Task DeleteCollectionAsync(string collectionName)
        {
            try
            {
                Collection? collection = await ChromaClient.Collection.GetCollectionAsync(tenant: Tenant, database: Name, collectionId: collectionName);
                if (collection != null)
                {
                    // Delete the collection
                    var deleteCollectionResponse = await ChromaClient.Collection.DeleteCollectionAsync(tenant: Tenant,
                    database: Name,
                    collectionId: collection.Name.ToString());

                    Console.WriteLine($"Delete collection response: {deleteCollectionResponse}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting collection: {ex.Message}");
            }
        }

        public async Task UpdateCollectionAsync(string oldCollectionName, string newCollectionName)
        {
            Collection? oldCollection = await ChromaClient.Collection.GetCollectionAsync(tenant: Tenant, database: Name, collectionId: oldCollectionName);
            if (oldCollection != null)
            {
                await ChromaClient.Collection.UpdateCollectionAsync(tenant: Tenant,
                database: Name,
                collectionId: oldCollection.Id.ToString(),
                request: new UpdateCollectionPayload
                {
                    NewName = newCollectionName,
                    // Could be used to update the configuration and metadata, but we don't want to change them here
                    NewConfiguration = null,
                    NewMetadata = null
                });
            }
        }

    }
}
