using Chroma;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Resources;
using System.Text;
using System.Text.Json;
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

    public async Task<string> ResetAsync()
    {
        var result = await ChromaClient.System.ResetAsync();
        return result ?? "Unknown";
    }

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

    #region Tenant Management

    public async Task<ChromaDBTenant?> GetOrCreateTenantAsync(string tenantName)
    {
        ChromaDBTenant? chromaDBTenant = null;

        try
        {
            var getTenantResponse = await ChromaClient.Tenant.GetTenantAsync(tenantName);

            chromaDBTenant = new ChromaDBTenant(tenantName, ChromaClient);
        }
        catch (Exception ex1) when (ex1.Message.Contains("NotFoundError"))
        {
            try
            {
                var createTenantResponse = await ChromaClient.Tenant.CreateTenantAsync(new CreateTenantPayload
                {
                    Name = tenantName
                });

                chromaDBTenant = new ChromaDBTenant(tenantName, ChromaClient);
            }
            catch (Exception ex2)
            {
                Console.WriteLine($"Error creating a tenant: {ex2.Message}");
            }
        }

        return chromaDBTenant;
    }

    /// <summary>
    /// Creates a new tenant with the specified name.
    /// </summary>
    /// <param name="tenantName"></param>
    /// <returns></returns>
    public async Task<ChromaDBTenant?> CreateTenantAsync(string tenantName)
    {
        ChromaDBTenant? chromaDBTenant = null;

        try
        {
            var createTenantResponse = await ChromaClient.Tenant.CreateTenantAsync(
                new CreateTenantPayload {
                    Name = tenantName
                }
            );

            chromaDBTenant = new ChromaDBTenant(tenantName, ChromaClient);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating a tenant: {ex.Message}");
        }

        return chromaDBTenant;
    }

    public async Task<ChromaDBTenant?> GetTenantAsync(string tenantName)
    {
        ChromaDBTenant? chromaDBTenant = null;

        try
        {
            var getTenantResponse = await ChromaClient.Tenant.GetTenantAsync(tenantName);
            
            chromaDBTenant = new ChromaDBTenant(tenantName, ChromaClient);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting a tenant: {ex.Message}");
        }

        return chromaDBTenant;
    }


    /// <summary>
    /// Seems not to work, but the API is there. It should update the tenant name.
    /// </summary>
    /// <param name="oldTenantName"></param>
    /// <param name="newTenantName"></param>
    /// <returns></returns>
    public async Task UpdateTenantAsync(string oldTenantName, string newTenantName)
    {
        var updateTenantResponse = await ChromaClient.Tenant.UpdateTenantAsync(oldTenantName, 
            request: new UpdateTenantPayload
            {
                ResourceName = newTenantName
            }
        );
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

    /// <summary>
    /// Get a collection by its name. Returns null if the collection does not exist.
    /// </summary>
    /// <param name="collectionName">The name of the collection to retrieve.</param>
    /// <param name="database">The name of the database containing the collection.</param>
    /// <param name="tenant">The tenant name.</param>
    /// <returns>The collection if found; otherwise, null.</returns>
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

    /// <summary>
    /// Get or create a collection by its name. If the collection does not exist, it will be created.
    /// </summary>
    /// <param name="collectionName">The name of the collection to retrieve or create.</param>
    /// <param name="database">The name of the database containing the collection.</param>
    /// <param name="tenant">The tenant name.</param>
    /// <returns>The collection if found or created.</returns>
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

    /// <summary>
    /// List all collections in a given database for a specific tenant.
    /// </summary>
    /// <param name="databaseName">The name of the database containing the collections.</param>
    /// <param name="tenant">The name of the tenant containing the database.</param>
    /// <returns>A list of collections in the specified database for the given tenant.</returns>
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
    /// Delete a collection by its name.
    /// </summary>
    /// <param name="collectionName">The name of the collection to delete.</param>
    /// <param name="database">The name of the database containing the collection.</param>
    /// <param name="tenant">The name of the tenant containing the database.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
    /// Changes the collection name.
    /// </summary>
    /// <param name="oldCollectionName">The current name of the collection.</param>
    /// <param name="newCollectionName">The new name for the collection.</param>
    /// <param name="tenant">The name of the tenant containing the database.</param>
    /// <param name="database">The name of the database containing the collection.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
    /// Get records from a collection based on various parameters such as ids, include, where conditions, limit, and offset.
    /// </summary>
    /// <param name="collectionName">The name of the collection to query.</param>
    /// <param name="ids">If indicated, restrict the query to the list of ids.</param>
    /// <param name="include">If indicated, specify which related data to include in the query.</param>
    /// <param name="where">If indicated, restrict the query to the specified conditions in metadatas.</param>
    /// <param name="whereDocument">If indicated, restrict the query to the specified conditions in documents.</param>
    /// <param name="limit">If indicated, limit the number of results returned.</param>
    /// <param name="offset">If indicated, specify the number of results to skip.</param>
    /// <param name="tenant">The name of the tenant containing the database.</param>
    /// <param name="database">The name of the database containing the collection.</param>
    /// <returns>A list of ChromaDocument objects matching the query parameters.</returns>
    public async Task<List<ChromaDbDocument>> CollectionGetAsync(string collectionName,
        List<string>? ids,
        List<Include>? include,
        WhereFilter? where,
        WhereDocumentFilter? whereDocument,
        int? limit,
        int? offset,
        string database = "default_database",
        string tenant = "default_tenant")
    {
        List<ChromaDbDocument> result = new List<ChromaDbDocument>();

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

        if (where is not null || whereDocument is not null)
        {
            // Handle the where and whereDocument conditions here
            rawWhereFields = new RawWhereFields
            {
                // It is mandatory to convert the WhereFilter to a JsonElement for the RawWhereFields
                Where = where?.ToJsonElement(),
                // It is mandatory to convert the WhereDocumentFilter to a JsonElement for the RawWhereFields
                WhereDocument = whereDocument?.ToJsonElement()
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
        }

        return result;
    }

    /// <summary>
    /// Converts a collection of Chroma metadata items into a list of dictionaries with string keys and object values.
    /// </summary>
    /// <param name="Metadatas">The collection of metadata items to convert, where each item can be either a plain object or a HashMap. Can be
    /// null.</param>
    /// <returns>A list of dictionaries containing the converted metadata. Returns an empty list if <paramref name="Metadatas"/>
    /// is null.</returns>
    private IList<IDictionary<string, object>> ConvertMetadatas(IList<Chroma.HashMap?>? Metadatas)
    {
        var result = new List<IDictionary<string, object>>();

        if (Metadatas != null)
        {
            foreach (var metadata in Metadatas)
            {
                Dictionary<string, object>? dict = null;

                if (metadata is not null)
                {
                    dict = new Dictionary<string, object>();
                    foreach (var kvp in metadata.AdditionalProperties)
                    {
                        dict[kvp.Key] = kvp.Value;
                    }
                }


                //metadata.Switch(
                //    obj =>
                //    {
                //        /* Handle object case */

                //        // The runtime type of obj is a System.Text.Json.JsonElement
                //        // representing a JSON object; convert it to Dictionary<string, object>.
                //        if (obj is JsonElement { ValueKind: JsonValueKind.Object } element)
                //        {
                //            dict = new Dictionary<string, object>();

                //            foreach (var property in element.EnumerateObject())
                //            {
                //                dict[property.Name] = ConvertJsonElement(property.Value)!;
                //            }
                //        }
                //    },
                //    hashMap =>
                //    {

                //        dict = new Dictionary<string, object>();

                //        // Handles the HashMap case and adds its properties to the result dictionary
                //        foreach (var kvp in hashMap.AdditionalProperties)
                //        {
                //            dict[kvp.Key] = kvp.Value;
                //        }
                //    }
                //);

                if (dict is not null)
                {
                    result.Add(dict);
                }
            }
        }

        return result;
    }



    /// <summary>
    /// Recursively converts a <see cref="JsonElement"/> into its closest .NET representation:
    /// objects become <see cref="Dictionary{TKey, TValue}"/>, arrays become <see cref="List{T}"/>,
    /// and primitives become their matching CLR types.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> to convert.</param>
    /// <returns>The closest .NET representation of the <paramref name="element"/>.</returns>
    private static object? ConvertJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var dict = new Dictionary<string, object>();
                foreach (var property in element.EnumerateObject())
                {
                    dict[property.Name] = ConvertJsonElement(property.Value)!;
                }
                return dict;

            case JsonValueKind.Array:
                var list = new List<object?>();
                foreach (var item in element.EnumerateArray())
                {
                    list.Add(ConvertJsonElement(item));
                }
                return list;

            case JsonValueKind.String:
                return element.GetString();

            case JsonValueKind.Number:
                if (element.TryGetInt64(out var l))
                {
                    return l;
                }
                return element.GetDouble();

            case JsonValueKind.True:
            case JsonValueKind.False:
                return element.GetBoolean();

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
            default:
                return null;
        }
    }

    /// <summary>
    /// Given a list of query embeddings, finds the documents nearest to them in the collection.
    /// The result is a list of documents, one for each embedding in the query.
    /// </summary>
    /// <param name="collectionName">The name of the collection to query.</param>
    /// <param name="queryEmbeddings">A list of query embeddings to find the nearest documents for.</param>
    /// <param name="include">Specifies which related data to include in the query.</param>
    /// <param name="ids">If indicated, restrict the query to the list of ids.</param>
    /// <param name="nResults">The number of nearest results to return for each query embedding.</param>
    /// <param name="where">If indicated, restrict the query to the specified conditions in metadatas.</param>
    /// <param name="whereDocument">If indicated, restrict the query to the specified conditions in documents.</param>
    /// <param name="limit">If indicated, limit the number of results returned.</param>
    /// <param name="offset">If indicated, specify the number of results to skip.</param>
    /// <param name="database">The name of the database containing the collection.</param>
    /// <param name="tenant">The name of the tenant containing the database.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CollectionQueryAsync(string collectionName,
        IList<IList<float>> queryEmbeddings,
        IList<Include>? include,
        IList<string>? ids,
        int? nResults,
        WhereFilter? where,
        WhereDocumentFilter? whereDocument,
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

        if (where is not null || whereDocument is not null)
        {
            // Handle the where and whereDocument conditions here
            rawWhereFields = new RawWhereFields
            {
                // It is mandatory to convert the WhereFilter to a JsonElement for the RawWhereFields
                Where = where?.ToJsonElement(),
                // It is mandatory to convert the WhereDocumentFilter to a JsonElement for the RawWhereFields
                WhereDocument = whereDocument?.ToJsonElement()
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
                IList<string?>? documents = queryResponse.Documents[0];
            }

            if (queryResponse.Distances != null && queryResponse.Distances.Count > 0)
            {
                IList<float?>? distances = queryResponse.Distances[0];
            }

            if (queryResponse.Metadatas != null && queryResponse.Metadatas.Count > 0)
            {
                IList<Chroma.HashMap?>? metadatas = queryResponse.Metadatas[0];
            }

            if (queryResponse.Uris != null && queryResponse.Uris.Count > 0)
            {
                IList<string?>? uris = queryResponse.Uris[0];
            }

        }

    }

    /// <summary>
    /// Adds records with embeddings and optional metadata to a Chroma collection, 
    /// creating the collection if it doesn't exist.
    /// </summary>
    /// <param name="collectionName">Name of the collection to add records to.</param>
    /// <param name="ids">List of unique identifiers for the records.</param>
    /// <param name="embeddings">List of embedding vectors for each record.</param>
    /// <param name="documents">Optional list of document contents.</param>
    /// <param name="uris">Optional list of URIs associated with the records.</param>
    /// <param name="metadatas">Optional list of metadata dictionaries for each record.</param>
    /// <param name="database">Database name. Defaults to "default_database".</param>
    /// <param name="tenant">Tenant name. Defaults to "default_tenant".</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CollectionAddAsync(string collectionName,
        IList<string> ids,
        IList<IList<float>> embeddings,
        IList<string?>? documents,
        IList<string?>? uris,
        IList<IDictionary<string, object>>? metadatas,
        string database = "default_database",
        string tenant = "default_tenant")
    {
        var embeddingsPayload = new EmbeddingsPayload
        {
            EmbeddingsPayloadVariant1 = embeddings,
            EmbeddingsPayloadVariant2 = null
        };

        IList<Chroma.HashMap?>? metas = null;

        if (metadatas is not null)
        {
            metas = new List<Chroma.HashMap?>();

            foreach (var metadata in metadatas)
            {
                HashMap hashMap = new HashMap
                {
                    AdditionalProperties = metadata
                };

                metas.Add(hashMap);
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

        // Add records in the collection
        var response = await ChromaClient.Record.CollectionAddAsync(tenant: tenant,
            database: database,
            collectionId: collection.Id.ToString(),
            request: addCollectionRecordsPayload);
    }

    /// <summary>
    /// Upserts records with embeddings and optional metadata to a Chroma collection, 
    /// creating the collection if it doesn't
    /// </summary>
    /// <param name="collectionName">Name of the collection to add records to.</param>
    /// <param name="ids">List of unique identifiers for the records.</param>
    /// <param name="embeddings">List of embedding vectors for each record.</param>
    /// <param name="documents">Optional list of document contents.</param>
    /// <param name="uris">Optional list of URIs associated with the records.</param>
    /// <param name="metadatas">Optional list of metadata dictionaries for each record.</param>
    /// <param name="database">Database name. Defaults to "default_database".</param>
    /// <param name="tenant"></param>
    /// <returns></returns>
    public async Task CollectionUpsertAsync(string collectionName,
        IList<string> ids,
        IList<IList<float>> embeddings,
        IList<string?>? documents,
        IList<string?>? uris,
        IList<IDictionary<string, object>>? metadatas,
        string database = "default_database",
        string tenant = "default_tenant")
    {
        var embeddingsPayload = new EmbeddingsPayload
        {
            EmbeddingsPayloadVariant1 = embeddings,
            EmbeddingsPayloadVariant2 = null
        };

        IList<Chroma.HashMap?>? metas = null;

        if (metadatas is not null)
        {
            metas = new List<Chroma.HashMap?>();

            foreach (var metadata in metadatas)
            {
                HashMap hashMap = new HashMap
                {
                    AdditionalProperties = metadata
                };

                metas.Add(hashMap);
            }
        }

        var upsertPayload = new UpsertCollectionRecordsPayload
        {
            // required fields
            Ids = ids,
            Embeddings = embeddingsPayload,
            // optional fields
            Documents = documents,
            Metadatas = metas,
            Uris = uris
        };

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

        // Upsert records in the collection
        await ChromaClient.Record.CollectionUpsertAsync(tenant: tenant,
            database: database,
            collectionId: collection.Id.ToString(),
            request: upsertPayload);
    }


    /// <summary>
    /// Updates records with embeddings and optional metadata in a Chroma collection,
    /// creating the collection if it doesn't exist. 
    /// This method is used to modify existing records in the collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection to add records to</param>
    /// <param name="ids">List of unique identifiers for the records.</param>
    /// <param name="embeddings">List of embedding vectors for each record.</param>
    /// <param name="documents">Optional list of document contents.</param>
    /// <param name="uris">Optional list of URIs associated with the records.</param>
    /// <param name="metadatas">Optional list of metadata dictionaries for each record.</param>
    /// <param name="database">Database name. Defaults to "default_database".</param>
    /// <param name="tenant"></param>
    /// <returns></returns>
    public async Task CollectionUpdateAsync(string collectionName,
        IList<string> ids,
        IList<IList<float>?>? embeddings,
        IList<string?>? documents,
        IList<string?>? uris,
        IList<IDictionary<string, object>>? metadatas,
        string database = "default_database",
        string tenant = "default_tenant")
    {
        var embeddingsPayload = new UpdateEmbeddingsPayload
        {
            UpdateEmbeddingsPayloadVariant1 = embeddings,
            UpdateEmbeddingsPayloadVariant2 = null
        };

        IList<Chroma.HashMap?>? metas = null;

        if (metadatas is not null)
        {
            metas = new List<Chroma.HashMap?>();

            foreach (var metadata in metadatas)
            {
                HashMap hashMap = new HashMap
                {
                    AdditionalProperties = metadata
                };

                metas.Add(hashMap);
            }
        }

        var updatePayload = new UpdateCollectionRecordsPayload
        {
            // required fields
            Ids = ids,
            Embeddings = embeddingsPayload,
            // optional fields
            Documents = documents,
            Metadatas = metas,
            Uris = uris
        };

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

        // Update records in the collection
        await ChromaClient.Record.CollectionUpdateAsync(tenant: tenant,
           database: database,
           collectionId: collection.Id.ToString(),
           request: updatePayload);
    }

    /// <summary>
    /// Delete items from a collection by id.
    /// </summary>
    /// <param name="collectionName">Name of the collection.</param>
    /// <param name="ids">List of ids to delete.</param>
    /// <param name="limit">Optional limit on the number of items to delete.</param>
    /// <param name="database">Database name. Defaults to "default_database".</param>
    /// <param name="tenant">Tenant name. Defaults to "default_tenant".</param>
    /// <returns></returns>
    public async Task CollectionDeleteAsync(string collectionName,
        IList<string> ids,
        int? limit,
        string database = "default_database",
        string tenant = "default_tenant")
    {

        DeleteCollectionRecordsPayloadVariant2 payloadVariant2 = new DeleteCollectionRecordsPayloadVariant2
        {
            Ids = ids,
            Limit = limit
        };

        await ChromaClient.Record.CollectionDeleteAsync(tenant: tenant,
            database: database,
            collectionId: collectionName,
            request: new DeleteCollectionRecordsPayload
            {
                DeleteCollectionRecordsPayloadVariant2 = payloadVariant2,
                RawWhereFields = null
            });
    }

    /// <summary>
    /// Delete all items in the collection that match the where filter.
    /// </summary>
    /// <param name="collectionName">Name of the collection.</param>
    /// <param name="whereFilter">Filter to match items for deletion.</param>
    /// <param name="database">Database name. Defaults to "default_database".</param>
    /// <param name="tenant">Tenant name. Defaults to "default_tenant".</param>
    /// <returns></returns>
    public async Task CollectionDeleteAsync(string collectionName,
        WhereFilter whereFilter,
        string database = "default_database",
        string tenant = "default_tenant")
    {
        RawWhereFields rawWhereFields = new RawWhereFields
        {
            Where = whereFilter,
            WhereDocument = null
        };

        await ChromaClient.Record.CollectionDeleteAsync(tenant: tenant,
            database: database,
            collectionId: collectionName,
            request: new DeleteCollectionRecordsPayload
            {
                DeleteCollectionRecordsPayloadVariant2 = null,
                RawWhereFields = rawWhereFields
            });
    }
    #endregion

}

