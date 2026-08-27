using Chroma;
using System;
using System.Collections.Generic;
using System.Resources;
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

    #region Server Management

    public async Task<string> GetVersionAsync()
    {
        string version = await ChromaClient.System.VersionAsync();
        return version ?? "Unknown";
    }

    public async Task<HeartbeatResponse> GetHeartbeatAsync()
    {
        HeartbeatResponse heartbeat = await ChromaClient.System.HeartbeatAsync();
        return heartbeat;
    }

    public async Task<string> GetHealthcheckAsync()
    {
        string healthcheck = await ChromaClient.System.HealthcheckAsync();
        return healthcheck ?? "Unknown";
    }

    public async Task<ChecklistResponse> GetPreFlightChecksAsync()
    {
        ChecklistResponse checklistResponse = await ChromaClient.System.PreFlightChecksAsync();
        return checklistResponse;
    }

    #endregion


    #region Database Management

    public async Task<ChromaDBDatabase?> CreateDatabaseAsync(string databaseName,
        string tenant = "default_tenant")
    {
        ChromaDBDatabase? chromaDBDatabase = null;

        try
        {
            var createDatabaseResponse = await ChromaClient.Database.CreateDatabaseAsync(tenant, databaseName);

            chromaDBDatabase = new ChromaDBDatabase(null, databaseName, tenant, ChromaClient);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating a database: {ex.Message}");
        }

        return chromaDBDatabase;
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
                result.Add(new ChromaDBDatabase(database.Id, database.Name, database.Tenant, ChromaClient));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing databases: {ex.Message}");
        }

        return result;
    }

    #endregion

    #region Collection Management

    public async Task<int> CountCollectionsAsync(string databaseName,
        string tenant = "default_tenant")
    {
        var count = await ChromaClient.Collection.CountCollectionsAsync(tenant: tenant, database: databaseName);
        return count;
    }

    public async Task<ChromaDBCollection?> GetCollectionAsync(string collectionId,
        string tenant = "default_tenant",
        string database = "default_database")
    {
        ChromaDBCollection? chromaDBCollection = null;
        try
        {
            // Warning : the parameter "collectionId" is the vecItem name, not the vecItem id.
            // The vecItem id is a guid, but the vecItem name is a string.

            // Bug for now : throw an exception when the vecItem does not exist
            Collection? myCollection = await ChromaClient.Collection.GetCollectionAsync(tenant: tenant, database: database, collectionId: collectionId);
            if (myCollection != null)
            {
                chromaDBCollection = new ChromaDBCollection(myCollection, ChromaClient);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }

        return chromaDBCollection;
    }

    public async Task<ChromaDBCollection> GetOrCreateCollection(string collectionName,
       string tenant = "default_tenant",
       string database = "default_database")
    {
        // Get our vecItem
        Collection collection = await ChromaClient.Collection.CreateCollectionAsync(tenant: tenant,
            database: database,
            request: new CreateCollectionPayload
            {
                Name = collectionName,
                GetOrCreate = true,
                //Metadata = null,
                //Configuration = null
            });

        return new ChromaDBCollection(collection, ChromaClient);
    }

    public async Task<List<ChromaDBCollection>> ListCollectionsAsync(string databaseName,
        string tenant = "default_tenant")
    {
        List<ChromaDBCollection> result = new List<ChromaDBCollection>();

        var vecItems = await ChromaClient.Collection.ListCollectionsAsync(
            tenant: tenant,
            database: databaseName);

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

    /// <summary>
    /// Delete a collection
    /// </summary>
    /// <returns></returns>
    public async Task DeleteCollectionAsync(string collectionName,
       string tenant = "default_tenant",
       string database = "default_database")
    {
        try
        {
            Collection? collection = await ChromaClient.Collection.GetCollectionAsync(tenant: tenant, database: database, collectionId: collectionName);
            if (collection != null)
            {
                // Delete the collection
                var deleteCollectionResponse = 
                    await ChromaClient.Collection.DeleteCollectionAsync(tenant: tenant,
                        database: database,
                        collectionId: collection.Name.ToString());

                Console.WriteLine($"Delete collection response: {deleteCollectionResponse}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting collection: {ex.Message}");
        }

    }

    #endregion







    //public async Task<Collection> GetCollectionAsync(string collectionName, 
    //    string tenant = "default_tenant",
    //    string database = "default_database")
    //{
    //    // Warning : the parameter "collectionId" is the vecItem name, not the vecItem id.
    //    // The vecItem id is a guid, but the vecItem name is a string.
    //    var collection = await ChromaClient.Collection.GetCollectionAsync(tenant: tenant,
    //        database: database,
    //        collectionId: collectionName);

    //    return collection;
    //}


}

