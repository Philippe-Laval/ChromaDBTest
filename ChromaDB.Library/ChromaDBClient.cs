using Chroma;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Resources;
using System.Text;
using System.Xml.Linq;
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

    public async Task<int> CountCollectionsAsync(string databaseName,
       string tenant = "default_tenant")
    {
        var count = await ChromaClient.Collection.CountCollectionsAsync(tenant: tenant, database: databaseName);
        return count;
    }

    #endregion

    #region Collection Management

    public async Task<ChromaDBCollection?> GetCollectionAsync(string collectionName,
        string database = "default_database",
        string tenant = "default_tenant")
    {
        ChromaDBCollection? chromaDBCollection = null;
        try
        {
            // Warning : the parameter "collectionName" is the vecItem name, not the vecItem id.
            // The vecItem id is a guid, but the vecItem name is a string.

            // Bug for now : throw an exception when the vecItem does not exist
            Collection? myCollection = await ChromaClient.Collection.GetCollectionAsync(tenant: tenant, database: database, collectionId: collectionName);
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
        string database = "default_database",
        string tenant = "default_tenant")
    {
        Collection collection = await ChromaClient.Collection.CreateCollectionAsync(tenant: tenant,
            database: database,
            request: new CreateCollectionPayload
            {
                Name = collectionName,
                GetOrCreate = true,
                Metadata = null,
                Configuration = null
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
        string database = "default_database",
        string tenant = "default_tenant")
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

    /// <summary>
    /// Changes the collection name
    /// </summary>
    /// <param name="oldCollectionName"></param>
    /// <param name="newCollectionName"></param>
    /// <param name="tenant"></param>
    /// <param name="database"></param>
    /// <returns></returns>
    public async Task UpdateCollectionAsync(string oldCollectionName, string newCollectionName,
        string database = "default_database",
        string tenant = "default_tenant")
    {
        Collection? oldCollection = await ChromaClient.Collection.GetCollectionAsync(tenant: tenant, database: database, collectionId: oldCollectionName);
        if (oldCollection != null)
        {
            await ChromaClient.Collection.UpdateCollectionAsync(tenant: tenant,
            database: database,
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

    #endregion

    #region Record Management

    /// <summary>
    /// 
    /// </summary>
    /// <param name="collectionName"></param>
    /// <param name="ids">if indicated restrict the query to the list of ids</param>
    /// <param name="include">if indicated specify which related data to include in the query</param>
    /// <param name="Where">if indicated restrict the query to the specified conditions in metadatas</param>
    /// <param name="WhereDocument">if indicated restrict the query to the specified conditions in documents</param>
    /// <param name="limit">if indicated limit the number of results returned</param>
    /// <param name="offset">if indicated specify the number of results to skip</param>
    /// <param name="tenant"></param>
    /// <param name="database"></param>
    /// <returns></returns>
    public async Task<List<ChromaDocument>> CollectionGetAsync(string collectionName,
        List<string>? ids,
        List<Include>? include,
        object? Where,
        object? WhereDocument,
        int? limit,
        int? offset,
        string database = "default_database",
        string tenant = "default_tenant")
    {
        List<ChromaDocument> result = new List<ChromaDocument>();

        // Get our collection
        Collection collection = await ChromaClient.Collection.CreateCollectionAsync(tenant: tenant,
             database: database,
             request: new CreateCollectionPayload
             {
                 Name = collectionName,
                 GetOrCreate = true,
                 Metadata = null,
                 Configuration = null
             });

        RawWhereFields? rawWhereFields = null;

        if (Where is not null || WhereDocument is not null)
        {
            // Handle the Where and WhereDocument conditions here
            rawWhereFields = new RawWhereFields
            {
                Where = Where,
                WhereDocument = WhereDocument
            };
        }

        GetRequestPayloadVariant2 getRequestPayloadVariant2 = new GetRequestPayloadVariant2
        {
            Ids = ids,
            Include = include,
            Limit = limit,
            Offset = offset
        };

        GetRequestPayload requestPayload = new GetRequestPayload
        {
            GetRequestPayloadVariant2 = getRequestPayloadVariant2,
            RawWhereFields = rawWhereFields
        };

        GetResponse response = await ChromaClient.Record.CollectionGetAsync(tenant: tenant,
            database: database,
            collectionId: collection.Id.ToString(),
            request: requestPayload);

        if (response != null)
        {
            QueryResult queryResult = new QueryResult
            {
                Ids = response.Ids,
                //Distances = response.Distances,
                Embeddings = response.Embeddings,
                Documents = response.Documents,
                Metadatas = ConvertMetadatas(response.Metadatas),
                Uris = response.Uris
            };

            result = queryResult.ToDocuments();

            foreach (var document in result)
            {
                Console.WriteLine($"Document Id: {document.Id}");
                Console.WriteLine($"Document Content: {document.Text}");
                Console.WriteLine($"Document Embedding: {string.Join(", ", document.Embeddings ?? new List<float>())}");
                Console.WriteLine($"Document Metadata: {string.Join(", ", document.Metadata ?? new Dictionary<string, object>())}");
                Console.WriteLine($"Document Uri: {document.Uri}");
            }
        }

        return result;
    }

    private IList<IDictionary<string, object>> ConvertMetadatas(IList<global::Chroma.OneOf<object, global::Chroma.HashMap>>? Metadatas)
    {
        var result = new List<IDictionary<string, object>>();

        if (Metadatas != null)
        {
            foreach (var metadata in Metadatas)
            {
                var dict = new Dictionary<string, object>();

                metadata.Switch(
                    obj => { /* Handle object case */ },
                    hashMap => {
                        // Handles the HashMap case and adds its properties to the result dictionary
                        foreach (var kvp in hashMap.AdditionalProperties)
                        {
                            dict[kvp.Key] = kvp.Value;
                        }
                    }
                );
                result.Add(dict);
            }
        }

        return result;
    }



    /// <summary>
    /// Given a list of embeddingsPayloadVariant1, finds the documents the nearest to the embeddingsPayloadVariant1 in the collection. 
    /// The result is a list of documents, one for each embedding in the query.
    /// </summary>
    /// <returns></returns>
    public async Task CollectionQueryAsync(string collectionName,
        IList<IList<float>> queryEmbeddings,
        IList<Include>? include,
        IList<string>? ids,
        int? nResults,
        object? Where,
        object? WhereDocument,
        int? limit,
        int? offset,
        string database = "default_database",
        string tenant = "default_tenant")
    {
        // Get our collection
        Collection collection = await ChromaClient.Collection.CreateCollectionAsync(tenant: tenant,
              database: database,
              request: new CreateCollectionPayload
              {
                  Name = collectionName,
                  GetOrCreate = true,
                  Metadata = null,
                  Configuration = null
              });

        QueryRequestPayloadVariant2 queryRequestPayloadVariant2 = new QueryRequestPayloadVariant2
        {
            QueryEmbeddings = queryEmbeddings,
            NResults = nResults,
            Include = include,
            Ids = ids
        };

        RawWhereFields? rawWhereFields = null;

        if (Where is not null || WhereDocument is not null)
        {
            // Handle the Where and WhereDocument conditions here
            rawWhereFields = new RawWhereFields
            {
                Where = Where,
                WhereDocument = WhereDocument
            };
        }

        QueryRequestPayload queryRequestPayload = new QueryRequestPayload
        {
            QueryRequestPayloadVariant2 = queryRequestPayloadVariant2,
            RawWhereFields = rawWhereFields
        };

        QueryResponse queryResponse = await ChromaClient.Record.CollectionQueryAsync(tenant: "default_tenant",
            database: "default_database",
            collectionId: collection.Id.ToString(),
            request: queryRequestPayload,
            limit: limit,
            offset: offset
            );

        if (queryResponse != null)
        {
            if (queryResponse.Ids != null && queryResponse.Ids.Count > 0)
            {
                IList<string>? _ids = queryResponse.Ids[0];
            }

            if (queryResponse.Documents != null && queryResponse.Documents.Count > 0)
            {
                IList<string>? documents = queryResponse.Documents[0];
            }

            if (queryResponse.Distances != null && queryResponse.Distances.Count > 0)
            {
                IList<float>? distances = queryResponse.Distances[0];
            }

            if (queryResponse.Metadatas != null && queryResponse.Metadatas.Count > 0)
            {
                IList<OneOf<object, HashMap>>? metadatas = queryResponse.Metadatas[0];
            }

            if (queryResponse.Uris != null && queryResponse.Uris.Count > 0)
            {
                IList<string>? uris = queryResponse.Uris[0];
            }

        }

    }


    public async Task CollectionAddAsync(string collectionName,
        IList<string> ids,
        IList<IList<float>> embeddings,
        IList<string>? documents,
        IList<string>? uris,
        IList<IDictionary<string, object>>? metadatas,
        string database = "default_database",
        string tenant = "default_tenant")
    {
        var embeddingsPayload = new EmbeddingsPayload
        {
            EmbeddingsPayloadVariant1 = embeddings,
            EmbeddingsPayloadVariant2 = null
        };

        List<OneOf<object, global::Chroma.HashMap>>? metas = null;

        if (metadatas is not null)
        {
            metas = new List<OneOf<object, global::Chroma.HashMap>>();

            foreach (var metadata in metadatas)
            {
                OneOf<object, HashMap> oneOf = new Chroma.OneOf<object, HashMap>(metadatas, null);
                metas.Add(oneOf);
            }
        }

        AddCollectionRecordsPayload addCollectionRecordsPayload = new AddCollectionRecordsPayload
        {
            // required fields
            Ids = ids,
            Embeddings = embeddingsPayload,
            // optional fields
            Documents = documents,
            Metadatas = metas,
            Uris = uris
        };

        // Get the collection where we want to add records
        Collection collection = await ChromaClient.Collection.CreateCollectionAsync(tenant: tenant,
              database: database,
              request: new CreateCollectionPayload
              {
                  Name = collectionName,
                  GetOrCreate = true,
                  Metadata = null,
                  Configuration = null
              });

        // Add records to the collection
        var response = await ChromaClient.Record.CollectionAddAsync(tenant: tenant,
            database: database,
            collectionId: collection.Id.ToString(),
            request: addCollectionRecordsPayload);
    }

    #endregion





    //public async Task<Collection> GetCollectionAsync(string oldCollectionName, 
    //    string tenant = "default_tenant",
    //    string database = "default_database")
    //{
    //    // Warning : the parameter "collectionName" is the vecItem name, not the vecItem id.
    //    // The vecItem id is a guid, but the vecItem name is a string.
    //    var collection = await ChromaClient.Collection.GetCollectionAsync(tenant: tenant,
    //        database: database,
    //        collectionName: oldCollectionName);

    //    return collection;
    //}


}

