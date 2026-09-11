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

// Launch chroma server with the following command in the terminal:
// cd C:\Users\philippe.laval
// chroma run C:\Users\philippe.laval\single_node_full.yaml

// ChromaDB Defaults to L2 Distance — Why that might not be the best choice
// https://razikus.substack.com/p/chromadb-defaults-to-l2-distance-why-that-might-not-be-the-best-choice-ac3d47461245

// https://docs.trychroma.com/docs/collections/configure
/* "cosine" or "ip" or "l2"
 * 
openai_collection = client.create_collection(
    name="my_openai_collection",
    embedding_function=OpenAIEmbeddingFunction(
        model_name="text-embedding-3-small"
    ),
    configuration={"hnsw": {"space": "cosine"}}
)
*/




public class ChromaDBClient
{
    public ChromaClient ChromaClient { get; private set; }

    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="ChromaDBClient"/> avec l’hôte, le port et la clé API
    /// spécifiés.
    /// </summary>
    /// <remarks>Par défaut, la connexion cible http://127.0.0.1:8000 et utilise "NotNeededForLocalhost" comme
    /// clé API.</remarks>
    /// <param name="host">Nom d’hôte ou adresse IP du serveur ChromaDB.</param>
    /// <param name="port">Port du serveur ChromaDB.</param>
    /// <param name="apiKey">Clé API utilisée pour authentifier les requêtes auprès du serveur ChromaDB.</param>
    public ChromaDBClient(string host = "127.0.0.1", int port = 8000, string apiKey = "NotNeededForLocalhost")
    {
        ChromaClient = new ChromaClient(
             apiKey: apiKey,
             baseUri: new Uri($"http://{host}:{port}"));
    }

    #region Server Management

    /// <summary>
    /// Réinitialise le système du serveur de manière asynchrone.
    /// </summary>
    /// <param name="cancellationToken">Jeton utilisé pour propager une demande d’annulation de l’opération asynchrone.</param>
    /// <returns>Message de résultat de la réinitialisation, ou "Unknown" si aucun message n’est renvoyé.</returns>
    public async Task<string> ResetAsync(CancellationToken cancellationToken = default)
    {
        var result = await ChromaClient.System.ResetAsync(cancellationToken: cancellationToken);
        return result ?? "Unknown";
    }

    public async Task<string> GetVersionAsync(CancellationToken cancellationToken = default)
    {
        string version = await ChromaClient.System.VersionAsync(cancellationToken: cancellationToken);
        return version ?? "Unknown";
    }

    public async Task<HeartbeatResponse> GetHeartbeatAsync(CancellationToken cancellationToken = default)
    {
        HeartbeatResponse heartbeat = await ChromaClient.System.HeartbeatAsync(cancellationToken: cancellationToken);
        return heartbeat;
    }

    public async Task<string> GetHealthcheckAsync(CancellationToken cancellationToken = default)
    {
        string healthcheck = await ChromaClient.System.HealthcheckAsync(cancellationToken: cancellationToken);
        return healthcheck ?? "Unknown";
    }

    public async Task<ChecklistResponse> GetPreFlightChecksAsync(CancellationToken cancellationToken = default)
    {
        ChecklistResponse checklistResponse = await ChromaClient.System.PreFlightChecksAsync(cancellationToken: cancellationToken);
        return checklistResponse;
    }

    #endregion

    #region Tenant Management

    /// <summary>
    /// Obtient un locataire existant par nom ou le crée s’il n’existe pas.
    /// </summary>
    /// <remarks>Retourne <see langword="null"/> si la création échoue après qu’aucun locataire existant n’a
    /// été trouvé.</remarks>
    /// <param name="tenantName">Nom du locataire à récupérer ou à créer (for example : "default_tenant").</param>
    /// <param name="cancellationToken">Jeton utilisé pour annuler l’opération asynchrone.</param>
    /// <returns>Instance de <c>ChromaDBTenant</c> correspondant au nom fourni si la récupération ou la création réussit ; sinon
    /// <see langword="null"/>.</returns>
    public async Task<ChromaDBTenant?> GetOrCreateTenantAsync(string tenantName, CancellationToken cancellationToken = default)
    {
        ChromaDBTenant? chromaDBTenant = null;

        try
        {
            var getTenantResponse = await ChromaClient.Tenant.GetTenantAsync(tenantName, cancellationToken: cancellationToken);

            chromaDBTenant = new ChromaDBTenant(tenantName, ChromaClient);
        }
        catch (Exception ex1) when (ex1.Message.Contains("NotFoundError"))
        {
            try
            {
                var createTenantResponse = await ChromaClient.Tenant.CreateTenantAsync(new CreateTenantPayload
                {
                    Name = tenantName
                }, cancellationToken: cancellationToken);

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
    /// <param name="tenantName">For example : "default_tenant"</param>
    /// <param name="cancellationToken">Jeton utilisé pour annuler l’opération asynchrone.</param>
    /// <returns>Instance de <c>ChromaDBTenant</c> correspondant au nom du locataire créé si l’opération réussit ; sinon <see langword="null"/>.</returns>
    public async Task<ChromaDBTenant?> CreateTenantAsync(string tenantName, CancellationToken cancellationToken = default)
    {
        ChromaDBTenant? chromaDBTenant = null;

        try
        {
            var createTenantResponse = await ChromaClient.Tenant.CreateTenantAsync(
                new CreateTenantPayload {
                    Name = tenantName
                }
            , cancellationToken: cancellationToken);

            chromaDBTenant = new ChromaDBTenant(tenantName, ChromaClient);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating a tenant: {ex.Message}");
        }

        return chromaDBTenant;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tenantName">for example : "default_tenant"</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ChromaDBTenant?> GetTenantAsync(string tenantName, CancellationToken cancellationToken = default)
    {
        ChromaDBTenant? chromaDBTenant = null;

        try
        {
            var getTenantResponse = await ChromaClient.Tenant.GetTenantAsync(tenantName, cancellationToken: cancellationToken);
            
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
    /// <param name="oldTenantName">Nom actuel du locataire.</param>
    /// <param name="newTenantName">Nouveau nom du locataire.</param>
    /// <param name="cancellationToken">Jeton utilisé pour annuler l’opération asynchrone.</param>
    /// <returns></returns>
    public async Task UpdateTenantAsync(string oldTenantName, string newTenantName, CancellationToken cancellationToken = default)
    {
        var updateTenantResponse = await ChromaClient.Tenant.UpdateTenantAsync(oldTenantName, 
            request: new UpdateTenantPayload
            {
                ResourceName = newTenantName
            }
        , cancellationToken: cancellationToken);
    }


    #endregion

    #region Database Management

    /// <summary>
    /// Creates a new database for the specified tenant.
    /// </summary>
    /// <param name="tenant">For example : "default_tenant"</param>
    /// <param name="databaseName">Nom de la base de données à créer.</param>
    /// <param name="cancellationToken">Jeton utilisé pour annuler l’opération asynchrone.</param>
    /// <returns>Instance de <c>ChromaDBDatabase</c> correspondant à la base de données créée si l’opération réussit ; sinon <see langword="null"/>.</returns>
    public async Task<ChromaDBDatabase?> CreateDatabaseAsync(string tenant,
        string databaseName, 
        CancellationToken cancellationToken = default)
    {
        ChromaDBDatabase? chromaDBDatabase = null;

        try
        {
            var createDatabaseResponse = await ChromaClient.Database.CreateDatabaseAsync(tenant, databaseName, cancellationToken: cancellationToken);

            chromaDBDatabase = new ChromaDBDatabase(null, databaseName, tenant, ChromaClient);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating a database: {ex.Message}");
        }

        return chromaDBDatabase;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tenant">For example : "default_tenant"</param>
    /// <param name="databaseName">Nom de la base de données à supprimer.</param>
    /// <param name="cancellationToken">Jeton utilisé pour annuler l’opération asynchrone.</param>
    /// <returns></returns>
    public async Task DeleteDatabaseAsync(string tenant, string databaseName, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleteDatabaseResponse = await ChromaClient.Database.DeleteDatabaseAsync(tenant, databaseName, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting a database: {ex.Message}");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tenant">For example: "default_tenant"</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<ChromaDBDatabase>> ListDatabasesAsync(string tenant, CancellationToken cancellationToken = default)
    {
        List<ChromaDBDatabase> result = new List<ChromaDBDatabase>();

        try
        {
            var databases = await ChromaClient.Database.ListDatabasesAsync(tenant, cancellationToken: cancellationToken);
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tenant">for example : "default_tenant"</param>
    /// <param name="databaseName"></param>
    /// <returns></returns>
    public async Task<int> CountCollectionsAsync(string tenant,
       string databaseName, CancellationToken cancellationToken = default)
    {
        var count = await ChromaClient.Collection.CountCollectionsAsync(tenant: tenant, database: databaseName, cancellationToken: cancellationToken);
        return count;
    }

    #endregion

    #region Collection Management

    /// <summary>
    /// Get a collection by its name. Returns null if the collection does not exist.
    /// </summary>
    /// <param name="tenant">The tenant name (for example : "default_tenant").</param>
    /// <param name="database">The name of the database containing the collection (for example : "default_database").</param>
    /// <param name="collectionName">The name of the collection to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The collection if found; otherwise, null.</returns>
    public async Task<ChromaDBCollection?> GetCollectionAsync(string tenant,
        string database,
        string collectionName, CancellationToken cancellationToken = default)
    {
        ChromaDBCollection? chromaDBCollection = null;
        try
        {
            // Warning : the parameter "collectionName" is the vecItem name, not the vecItem id.
            // The vecItem id is a guid, but the vecItem name is a string.

            // Bug for now : throw an exception when the vecItem does not exist
            Collection? myCollection = await ChromaClient.Collection.GetCollectionAsync(
                tenant: tenant, database: database, collectionId: collectionName, cancellationToken: cancellationToken);
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
    /// Setup the collection to use the specified distance.
    /// Other parameters will use their default values.
    /// </summary>
    /// <param name="space">The distance to use in the collection</param>
    /// <returns></returns>
    public static CollectionConfiguration SetupCollectionConfiguration(Chroma.Space space = Chroma.Space.Cosine)
    {
        Chroma.EmbeddingFunctionConfiguration? chromaEmbeddingFunction = null;
        Chroma.HnswConfiguration? chromaHnsw = new HnswConfiguration
        {
            Space = space
        };
        Chroma.SpannConfiguration? chromaSpann = null;
        var collectionConfiguration = new CollectionConfiguration(chromaEmbeddingFunction, chromaHnsw, chromaSpann);
        return collectionConfiguration;
    }

    /// <summary>
    /// Get or create a collection by its name. If the collection does not exist, it will be created.
    /// </summary>
    /// <param name="tenant">The tenant name (for example : "default_tenant").</param>
    /// <param name="database">The name of the database containing the collection (for example : "default_database").</param>
    /// <param name="collectionName">The name of the collection to retrieve or create.</param>
    /// <param name="collectionConfiguration">Optional configuration for the collection. If not provided, default configuration will be used.</param>
    /// <param name="collectionMetadata">Optional metadata for the collection.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The collection if found or created.</returns>
    public async Task<ChromaDBCollection> GetOrCreateCollection(string tenant,
        string database,
        string collectionName,
        CollectionConfiguration? collectionConfiguration = null,
        Chroma.HashMap? collectionMetadata = null,
        CancellationToken cancellationToken = default)
    {
        Collection collection = await ChromaClient.Collection.CreateCollectionAsync(tenant: tenant,
            database: database,
            request: new CreateCollectionPayload
            {
                Name = collectionName,
                GetOrCreate = true,
                Metadata = collectionMetadata,
                Configuration = collectionConfiguration
            }, cancellationToken: cancellationToken);

        return new ChromaDBCollection(collection, ChromaClient);
    }

    /// <summary>
    /// List all collections in a given database for a specific tenant.
    /// </summary>
    /// <param name="tenant">The name of the tenant containing the database (for example : "default_tenant")².</param>
    /// <param name="databaseName">The name of the database containing the collections.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of collections in the specified database for the given tenant.</returns>
    public async Task<List<ChromaDBCollection>> ListCollectionsAsync(string tenant,
        string databaseName, CancellationToken cancellationToken = default)
    {
        List<ChromaDBCollection> result = new List<ChromaDBCollection>();

        var vecItems = await ChromaClient.Collection.ListCollectionsAsync(
            tenant: tenant,
            database: databaseName, cancellationToken: cancellationToken);

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
    /// <param name="tenant">The name of the tenant containing the database (for example : "default_tenant").</param>
    /// <param name="database">The name of the database containing the collection (for example : "default_database").</param>
    /// <param name="collectionName">The name of the collection to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteCollectionAsync(string tenant,
        string database,
        string collectionName, CancellationToken cancellationToken = default)
    {
        try
        {
            Collection? collection = await ChromaClient.Collection.GetCollectionAsync(
                tenant: tenant, database: database, collectionId: collectionName);
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
    /// <param name="tenant">The name of the tenant containing the database.</param>
    /// <param name="database">The name of the database containing the collection.</param>
    /// <param name="oldCollectionName">The current name of the collection.</param>
    /// <param name="newCollectionName">The new name for the collection.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateCollectionAsync(string tenant, string database,
        string oldCollectionName,
        string newCollectionName, CancellationToken cancellationToken = default)
    {
        Collection? oldCollection = await ChromaClient.Collection.GetCollectionAsync(
            tenant: tenant, database: database, collectionId: oldCollectionName, cancellationToken: cancellationToken);
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
    /// <param name="tenant">The name of the tenant containing the database.</param>
    /// <param name="database">The name of the database containing the collection.</param>
    /// <param name="collectionName">The name of the collection to query.</param>
    /// <param name="ids">If indicated, restrict the query to the list of ids.</param>
    /// <param name="include">If indicated, specify which related data to include in the query.</param>
    /// <param name="where">If indicated, restrict the query to the specified conditions in metadatas.</param>
    /// <param name="whereDocument">If indicated, restrict the query to the specified conditions in documents.</param>
    /// <param name="limit">If indicated, limit the number of results returned.</param>
    /// <param name="offset">If indicated, specify the number of results to skip.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of ChromaDocument objects matching the query parameters.</returns>
    public async Task<List<ChromaDbDocument>> CollectionGetAsync(string tenant,
        string database,
        string collectionName,
        List<string>? ids,
        List<Include>? include,
        WhereFilter? where,
        WhereDocumentFilter? whereDocument,
        int? limit,
        int? offset,
        CancellationToken cancellationToken = default)
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
             },
             cancellationToken: cancellationToken);

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
            request: requestPayload,
            cancellationToken: cancellationToken);

        if (response != null)
        {
            GetResult getResult = new GetResult
            {
                Ids = response.Ids,
                Embeddings = response.Embeddings,
                Documents = response.Documents,
                Metadatas = ConvertMetadatas(response.Metadatas),
                Uris = response.Uris
            };

            result = getResult.ToDocuments();
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
    private IList<IDictionary<string, object?>?>? ConvertMetadatas(IList<Chroma.HashMap?>? Metadatas)
    {
        List<IDictionary<string, object?>?>? result = null;

        if (Metadatas != null)
        {
            result = new List<IDictionary<string, object?>?>();

            foreach (var metadata in Metadatas)
            {
                Dictionary<string, object?>? dict = null;

                if (metadata is not null)
                {
                    dict = new Dictionary<string, object?>();
                    foreach (var kvp in metadata.AdditionalProperties)
                    {
                        if (kvp.Value is JsonElement jsonElement)
                        {
                            dict[kvp.Key] = ConvertJsonElement(jsonElement);
                        }
                        else
                        {
                            dict[kvp.Key] = null;
                        }
                    }
                }

                result.Add(dict);
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
    /// <param name="tenant">Name of the tenant containing the database.</param>
    /// <param name="database">Name of the database containing the collection.</param>
    /// <param name="collectionName">Name of the collection to query.</param>
    /// <param name="queryEmbeddings">A list of query embeddings to find the nearest documents for.</param>
    /// <param name="include">Specifies which related data to include in the query.</param>
    /// <param name="ids">If indicated, restrict the query to the list of ids.</param>
    /// <param name="nResults">The number of nearest results to return for each query embedding.</param>
    /// <param name="where">If indicated, restrict the query to the specified conditions in metadatas.</param>
    /// <param name="whereDocument">If indicated, restrict the query to the specified conditions in documents.</param>
    /// <param name="limit">If indicated, limit the number of results returned.</param>
    /// <param name="offset">If indicated, specify the number of results to skip.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<IList<IList<ChromaDbDocument>>> CollectionQueryAsync(string tenant,
        string database,
        string collectionName,
        IList<IList<float>> queryEmbeddings,
        IList<Include>? include,
        IList<string>? ids,
        int? nResults,
        WhereFilter? where,
        WhereDocumentFilter? whereDocument,
        int? limit,
        int? offset,
        CancellationToken cancellationToken = default)
    {
        List<IList<ChromaDbDocument>> result = new List<IList<ChromaDbDocument>>();

        // Get our collection
        Collection collection = await ChromaClient.Collection.CreateCollectionAsync(tenant: tenant,
              database: database,
              request: new CreateCollectionPayload
              {
                  Name = collectionName,
                  GetOrCreate = true,
                  Metadata = null,
                  Configuration = null
              },
              cancellationToken: cancellationToken);

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

        QueryResponse queryResponse = await ChromaClient.Record.CollectionQueryAsync(tenant: tenant,
            database: database,
            collectionId: collection.Id.ToString(),
            request: queryRequestPayload,
            limit: limit,
            offset: offset,
            cancellationToken: cancellationToken);

        if (queryResponse != null)
        {
            int count = queryResponse.Ids.Count;

            for (int index = 0; index < count; index++)
            {
                QueryResult queryResult = new QueryResult
                {
                    Ids = queryResponse.Ids[index],
                    Embeddings = queryResponse.Embeddings?[index],
                    Distances = queryResponse.Distances?[index],
                    Documents = queryResponse.Documents?[index],
                    Metadatas = ConvertMetadatas(queryResponse.Metadatas?[index]),
                    Uris = queryResponse.Uris?[index]
                };

                var documents = queryResult.ToDocuments();
                result.Add(documents);  
            }
        }

        return result;
    }

    /// <summary>
    /// Adds records with embeddings and optional metadata to a Chroma collection, 
    /// creating the collection if it doesn't exist.
    /// </summary>
    /// <param name="tenant">Tenant name. For example : "default_tenant".</param>
    /// <param name="database">Database name. For example : "default_database".</param>
    /// <param name="collectionName">Name of the collection to add records to.</param>
    /// <param name="ids">List of unique identifiers for the records.</param>
    /// <param name="embeddings">List of embedding vectors for each record.</param>
    /// <param name="documents">Optional list of document contents.</param>
    /// <param name="uris">Optional list of URIs associated with the records.</param>
    /// <param name="metadatas">Optional list of metadata dictionaries for each record.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CollectionAddAsync(string tenant,
        string database,
        string collectionName,
        IList<string> ids,
        IList<IList<float>> embeddings,
        IList<string?>? documents,
        IList<string?>? uris,
        IList<IDictionary<string, object>>? metadatas,
        CancellationToken cancellationToken = default)
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
              },
              cancellationToken: cancellationToken);

        // Add records in the collection
        var response = await ChromaClient.Record.CollectionAddAsync(tenant: tenant,
            database: database,
            collectionId: collection.Id.ToString(),
            request: addCollectionRecordsPayload,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Upserts records with embeddings and optional metadata to a Chroma collection, 
    /// creating the collection if it doesn't
    /// </summary>
    /// <param name="tenant">Tenant name. For example : "default_tenant".</param>
    /// <param name="database">Database name. For example : "default_database".</param>
    /// <param name="collectionName">Name of the collection to add records to.</param>
    /// <param name="ids">List of unique identifiers for the records.</param>
    /// <param name="embeddings">List of embedding vectors for each record.</param>
    /// <param name="documents">Optional list of document contents.</param>
    /// <param name="uris">Optional list of URIs associated with the records.</param>
    /// <param name="metadatas">Optional list of metadata dictionaries for each record.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns></returns>
    public async Task CollectionUpsertAsync(string tenant,
        string database,
        string collectionName,
        IList<string> ids,
        IList<IList<float>> embeddings,
        IList<string?>? documents,
        IList<string?>? uris,
        IList<IDictionary<string, object>>? metadatas,
        CancellationToken cancellationToken = default)
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
              },
              cancellationToken: cancellationToken);

        // Upsert records in the collection
        await ChromaClient.Record.CollectionUpsertAsync(tenant: tenant,
            database: database,
            collectionId: collection.Id.ToString(),
            request: upsertPayload,
            cancellationToken: cancellationToken);
    }


    /// <summary>
    /// Updates records with embeddings and optional metadata in a Chroma collection,
    /// creating the collection if it doesn't exist. 
    /// This method is used to modify existing records in the collection.
    /// </summary>
    /// <param name="tenant">Tenant name. For example : "default_tenant".</param>
    /// <param name="database">Database name. For example : "default_database".</param>
    /// <param name="collectionName">Name of the collection to add records to</param>
    /// <param name="ids">List of unique identifiers for the records.</param>
    /// <param name="embeddings">List of embedding vectors for each record.</param>
    /// <param name="documents">Optional list of document contents.</param>
    /// <param name="uris">Optional list of URIs associated with the records.</param>
    /// <param name="metadatas">Optional list of metadata dictionaries for each record.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns></returns>
    public async Task CollectionUpdateAsync(string collectionName,
        IList<string> ids,
        IList<IList<float>?>? embeddings,
        IList<string?>? documents,
        IList<string?>? uris,
        IList<IDictionary<string, object>>? metadatas,
        string database,
        string tenant,
        CancellationToken cancellationToken = default)
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
              },
              cancellationToken: cancellationToken);

        // Update records in the collection
        await ChromaClient.Record.CollectionUpdateAsync(tenant: tenant,
           database: database,
           collectionId: collection.Id.ToString(),
           request: updatePayload,
           cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Delete items from a collection by id.
    /// </summary>
    /// <param name="tenant">Tenant name. For example : "default_tenant".</param>
    /// <param name="database">Database name. For example : "default_database".</param>
    /// <param name="collectionName">Name of the collection.</param>
    /// <param name="ids">List of ids to delete.</param>
    /// <param name="limit">Optional limit on the number of items to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns></returns>
    public async Task CollectionDeleteAsync(string tenant,
        string database,
        string collectionName,
        IList<string> ids,
        int? limit,
        CancellationToken cancellationToken = default)
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
            },
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Delete all items in the collection that match the where filter.
    /// </summary>
    /// <param name="tenant">Tenant name. For example : "default_tenant".</param>
    /// <param name="database">Database name. For example : "default_database".</param>
    /// <param name="collectionName">Name of the collection.</param>
    /// <param name="whereFilter">Filter to match items for deletion.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns></returns>
    public async Task CollectionDeleteAsync(string tenant,
        string database,
        string collectionName,
        WhereFilter whereFilter,
        CancellationToken cancellationToken = default)
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
            },
            cancellationToken: cancellationToken);
    }
    #endregion

}

