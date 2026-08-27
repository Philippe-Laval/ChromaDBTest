using Chroma;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChromaDB.Library;


public class ChromaDBClient
{
    public ChromaClient ChromaClient { get; private set; }

    public ChromaDBClient(string host = "127.0.0.1", int port = 8000, string apiKey = "NotNeededForLocalhost")
    {
        ChromaClient = new ChromaClient(
             apiKey: apiKey,
             baseUri: new Uri($"http://{host}:{port}"));
    }

    #region Database Management

    public async Task CreateDatabaseAsync(string databaseName,
        string tenant = "default_tenant")
    {
        try
        {
            var createDatabaseResponse = await ChromaClient.Database.CreateDatabaseAsync(tenant, databaseName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating a database: {ex.Message}");
        }
    }

    public async Task DeleteDatabaseAsync(string databaseName, string tenant = "default_tenant")
    {
        try
        {
            var deleteDatabaseResponse = await ChromaClient.Database.DeleteDatabaseAsync(tenant, databaseName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting a database: {ex.Message}");
        }
    }

    public async Task<List<ChromaDBDatabase>> ListDatabasesAsync(string tenant = "default_tenant")
    {
        List<ChromaDBDatabase> result = new List<ChromaDBDatabase>();

        try
        {
            var databases = await ChromaClient.Database.ListDatabasesAsync(tenant);
            foreach (var database in databases)
            {
                result.Add(new ChromaDBDatabase(database.Id, database.Name, database.Tenant));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing databases: {ex.Message}");
        }

        return result;
    }

    public async Task<int> CountCollectionsAsync(string databaseName,
        string tenant = "default_tenant")
    {
        var count = await ChromaClient.Collection.CountCollectionsAsync(tenant: tenant, database: databaseName);
        Console.WriteLine($"Collection count: {count}");
        return count;
    }

    public async Task TestGetCollectionAsync()
    {
        try
        {
            // Bug for now : throw an exception when the collection does not exist
            Collection? myCollection = await ChromaClient.Collection.GetCollectionAsync(tenant: "default_tenant", database: "default_database", collectionId: "c359bdb6-c29d-4fe3-87f1-b13ec62061e5");
            if (myCollection != null)
            {
                Console.WriteLine($"Collection: {myCollection.Name} {myCollection.Dimension} {myCollection.Database} {myCollection.Tenant}");
            }
            else
            {
                Console.WriteLine("Collection not found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing databases: {ex.Message}");
        }
    }

    public async Task TestListCollectionsAsync(string databaseName,
        string tenant = "default_tenant")
    {
        var collections = await ChromaClient.Collection.ListCollectionsAsync(
            tenant: tenant,
            database: databaseName);

        foreach (var collection in collections)
        {
            Console.WriteLine($"Collection: {collection.Id} {collection.Name} {collection.Dimension} {collection.Database} {collection.Tenant}");
        }
    }

    #endregion



    public async Task<Collection> GetOrCreateCollection(string collectionName, 
        string tenant = "default_tenant",
        string database = "default_database")
    {
        // Get our collection
        Collection collection = await ChromaClient.Collection.CreateCollectionAsync(tenant: tenant,
            database: database,
            request: new CreateCollectionPayload
            {
                Name = collectionName,
                GetOrCreate = true,
                //Metadata = null,
                //Configuration = null
            });

        return collection;
    }

    public async Task<Collection> GetCollectionAsync(string collectionName, 
        string tenant = "default_tenant",
        string database = "default_database")
    {
        // Warning : the parameter "collectionId" is the collection name, not the collection id.
        // The collection id is a guid, but the collection name is a string.
        var myCollection = await ChromaClient.Collection.GetCollectionAsync(tenant: tenant,
            database: database,
            collectionId: collectionName);

        return myCollection;
    }

    public async Task CollectionAddAsync(string collectionName,
        string tenant = "default_tenant",
        string database = "default_database")
    {
        // Fake embeddingsPayloadVariant1 for testing (384 dimensions)
        IList<IList<float>> embeddingsPayloadVariant1 = new List<IList<float>>
            {
                GetEmbeddingForDoc3(),
                GetEmbeddingForDoc4()
            };

        var embeddingsPayload = new EmbeddingsPayload
        {
            EmbeddingsPayloadVariant1 = embeddingsPayloadVariant1,
            EmbeddingsPayloadVariant2 = null
        };

        AddCollectionRecordsPayload addCollectionRecordsPayload = new AddCollectionRecordsPayload
        {
            // required fields
            Ids = new List<string> { "id3", "id4" },
            Embeddings = embeddingsPayload,
            // optional fields
            Documents = new List<string> { "This is a document about lemons", "This is a document about mangos" }
        };

        // Get the collection where we want to add records
        Collection collection = await GetOrCreateCollection(collectionName, tenant, database);

        // Add records to the collection
        await ChromaClient.Record.CollectionAddAsync(tenant: tenant,
            database: database,
            collectionId: collection.Id.ToString(),
            request: addCollectionRecordsPayload);
    }



    public static IList<float> GetEmbeddingForDoc1()
    {
        return GetEmbedding(0.1f);
    }
    public static IList<float> GetEmbeddingForDoc2()
    {
        return GetEmbedding(0.2f);
    }

    public static IList<float> GetEmbeddingForDoc3()
    {
        return GetEmbedding(0.3f);
    }

    public static IList<float> GetEmbeddingForDoc4()
    {
        return GetEmbedding(0.4f);
    }

    public static IList<float> GetEmbedding(float value)
    {
        // Fake embeddingsPayloadVariant1 for testing (384 dimensions)
        List<float> embedding = new List<float>();

        for (int i = 0; i < 384; i++)
        {
            embedding.Add(value);
        }

        return embedding;
    }
}

