using Chroma;
using ChromaDB.Library;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ChromaDBTest
{
    /// <summary>
    /// Uses the ChromaClient to interact with the ChromaDB API, demonstrating various operations such as creating collections, adding records, querying, and retrieving system information.
    /// https://github.com/tryAGI/Chroma
    /// </summary>
    public static class TryAGIChromaTest
    {
        public static async Task GetClientVersion(ChromaClient? client = null)
        {
            client ??= CreateClient();

            string version = await client.System.VersionAsync();
            Console.WriteLine($"Chroma version: {version}");

            HeartbeatResponse heartbeat = await client.System.HeartbeatAsync();
            Console.WriteLine($"Heartbeat: {heartbeat.Nanosecond_heartbeat}");

            string? healthcheck = await client.System.HealthcheckAsync();
            Console.WriteLine($"Healthcheck: {healthcheck ?? "Unknown"}");

            ChecklistResponse checklistResponse = await client.System.PreFlightChecksAsync();
            Console.WriteLine($"MaxBatchSize: {checklistResponse.MaxBatchSize}");
            Console.WriteLine($"SupportsBase64Encoding: {checklistResponse.SupportsBase64Encoding}");
            Console.WriteLine($"AdditionalProperties: {checklistResponse.AdditionalProperties}");
        }

        public static async Task ResetTheServer(ChromaClient? client = null)
        {
            client ??= CreateClient();

            Console.WriteLine("Resetting the server...");

            await client.System.ResetAsync();
        }

        public static async Task TestCreationOfDatabase(ChromaClient? client = null)
        {
            client ??= CreateClient();

            //var response = await client.Database.CreateDatabaseAsync("default_tenant", "database1");

            try
            {
                // Create a new database named "database2" for the tenant "default_tenant"
                var createDatabaseResponse = await client.Database.CreateDatabaseAsync("default_tenant", "database2");

                // Add a break point and look with a sqlite browser to see the database2 created in the ChromaDB data folder

                // Delete the database after creation for cleanup
                var deleteDatabaseResponse = await client.Database.DeleteDatabaseAsync("default_tenant", "database2");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating or deleting database: {ex.Message}");
            }
        }

        public static async Task TestListingOfDatabases(ChromaClient? client = null)
        {
            client ??= CreateClient();

            try
            {
                // Bug for now
                var databases = await client.Database.ListDatabasesAsync("default_tenant");
                foreach (var database in databases)
                {
                    Console.WriteLine($"Database: {database.Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing databases: {ex.Message}");
            }
        }

        public static async Task TestGetCollectionCountOfDatabase(ChromaClient? client = null)
        {
            client ??= CreateClient();

            var count = await client.Collection.CountCollectionsAsync(tenant: "default_tenant", database: "default_database");
            Console.WriteLine($"Collection count: {count}");
        }

        public static async Task TestGetCollectionAsync(ChromaClient? client = null)
        {
            client ??= CreateClient();

            try
            {
                // Bug for now : throw an exception when the collection does not exist
                Collection? myCollection = await client.Collection.GetCollectionAsync(tenant: "default_tenant", database: "default_database", collectionId: "c359bdb6-c29d-4fe3-87f1-b13ec62061e5");
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

        public static async Task TestListCollectionsAsync(ChromaClient? client = null)
        {
            client ??= CreateClient();

            var collections = await client.Collection.ListCollectionsAsync(
                tenant: "default_tenant",
                database: "default_database");

            foreach (var collection in collections)
            {
                Console.WriteLine($"Collection: {collection.Id} {collection.Name} {collection.Dimension} {collection.Database} {collection.Tenant}");
            }
        }

        /// <summary>
        /// Count the entries in the collection of a database for a tenant.
        /// </summary>
        /// <returns></returns>
        public static async Task TestCollectionCountAsync(ChromaClient? client = null)
        {
            client ??= CreateClient();

            var recordCount = await client.Record.CollectionCountAsync(tenant: "default_tenant",
                database: "default_database",
                collectionId: "c359bdb6-c29d-4fe3-87f1-b13ec62061e5");
            Console.WriteLine($"Record count: {recordCount}");
        }

        public static async Task TestCreateCollectionAsync(ChromaClient? client = null)
        {
            client ??= CreateClient();

            // Missing column "dimension" 384 - actually NULL
            Collection collection3 = await client.Collection.CreateCollectionAsync(tenant: "default_tenant",
                database: "default_database",
                request: new CreateCollectionPayload
                {
                    Name = "my_collection3",
                    GetOrCreate = true,
                    //Metadata = null,
                    //Configuration = null
                });

            // collection3.Dimension = 384;
        }

        /// <summary>
        /// GetOrCreate = true will ensure the collection is created if needed, or return the existing collection if it already exists. 
        /// This is useful for avoiding errors when trying to create a collection that may already exist.
        /// </summary>
        /// <returns></returns>
        public static async Task <Collection> TestCreateCollectionAsyncWithExistingCollection(string collectionName = "my_collection2", ChromaClient? client = null)
        {
            client ??= CreateClient();

            Collection collection = await client.Collection.CreateCollectionAsync(tenant: "default_tenant",
                database: "default_database",
                request: new CreateCollectionPayload
                {
                    Name = collectionName,
                    GetOrCreate = true,
                    //Metadata = null,
                    //Configuration = null
                });

            return collection;
        }

        /*
         await client.Collection.UpdateCollectionAsync(tenant: "default_tenant",
               database: "default_database",
               collectionId: collection3.Id.ToString(),
               request: new UpdateCollectionPayload
               {
                   NewName = null,
                   NewConfiguration = null,
                   NewMetadata = null
               });
         */

        /// <summary>
        /// Delete a collection
        /// </summary>
        /// <returns></returns>
        public static async Task TestDeleteCollectionAsync(ChromaClient? client = null)
        {
            client ??= CreateClient();

            try
            {
                // Get the collection to delete
                Collection collection3 = await client.Collection.CreateCollectionAsync(tenant: "default_tenant",
                database: "default_database",
                request: new CreateCollectionPayload
                {
                    Name = "my_collection3",
                    GetOrCreate = true,
                    //Metadata = null,
                    //Configuration = null
                });

                // Delete the collection
                var deleteCollectionResponse = await client.Collection.DeleteCollectionAsync(tenant: "default_tenant",
                    database: "default_database",
                    collectionId: collection3.Name.ToString());

                Console.WriteLine($"Delete collection response: {deleteCollectionResponse}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting collection: {ex.Message}");
            }

        }

        public static async Task TestCollectionAddAsync(string collectionName, ChromaClient? client = null)
        {
            client ??= CreateClient();

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
            Collection collection = await GetOrCreateCollection(collectionName, client);

            // Add records to the collection
            await client.Record.CollectionAddAsync(tenant: "default_tenant",
                database: "default_database",
                collectionId: collection.Id.ToString(),
                request: addCollectionRecordsPayload);
        }


        public static async Task TestUpdateCollectionAsync(ChromaClient? client = null)
        {
            client ??= CreateClient();

            // Changes the collection name

            await client.Collection.UpdateCollectionAsync(tenant: "default_tenant",
                database: "default_database",
                collectionId: "c359bdb6-c29d-4fe3-87f1-b13ec62061e5",
                request: new UpdateCollectionPayload
                {
                    NewName = "my_collection2",
                    NewConfiguration = null,
                    NewMetadata = null
                });

        }

        public static async Task<int> TestCountCollectionsAsync(ChromaClient? client = null)
        {
            client ??= CreateClient();

            int countCollection = await client.Collection.CountCollectionsAsync(tenant: "default_tenant", database: "default_database");

            return countCollection;
        }

        /// <summary>
        /// TODO : does not find the collection, even if it exists. 
        /// The collection is created with GetOrCreate = true, but can not be found with GetCollectionAsync.
        /// </summary>
        /// <returns></returns>
        public static async Task<Collection> GetCollectionAsync_KO(ChromaClient? client = null) 
        {
            client ??= CreateClient();

            // Dos not work : we should use the name not the id
            var myCollection = await client.Collection.GetCollectionAsync(tenant: "default_tenant",
                database: "default_database", 
                collectionId: "c359bdb6-c29d-4fe3-87f1-b13ec62061e5");

            return myCollection;
        }

        public static async Task<Collection> GetCollectionAsync(string collectionName = "my_collection2", ChromaClient? client = null)
        {
            client ??= CreateClient();

            // Warning : the parameter "collectionId" is the collection name, not the collection id.
            // The collection id is a guid, but the collection name is a string.
            var myCollection = await client.Collection.GetCollectionAsync(tenant: "default_tenant",
                database: "default_database",
                collectionId: collectionName);

            return myCollection;
        }

        public static async Task TestCollectionUpsertAsync(string collectionName, ChromaClient? client = null)
        {
            client ??= CreateClient();

            IList<IList<float>> embeddingsPayLoadVariant1 = new List<IList<float>>
            {
                GetEmbeddingForDoc1(),
                GetEmbeddingForDoc2()
            };

            var embeddings = new EmbeddingsPayload
            {
                EmbeddingsPayloadVariant1 = embeddingsPayLoadVariant1,
                EmbeddingsPayloadVariant2 = null
            };

            var upsertPayload = new UpsertCollectionRecordsPayload
            {
                Embeddings = embeddings,
                Ids = new List<string> { "id1", "id2" },
                Documents = new List<string> { "This is a document about nice pineapple", "This is a document about juicy oranges" }
            };

            // Get our collection
            Collection collection = await GetOrCreateCollection(collectionName, client);

            await client.Record.CollectionUpsertAsync(tenant: "default_tenant",
                database: "default_database",
                collectionId: collection.Id.ToString(),
                request: upsertPayload);
        }


        //QueryRequestPayload queryRequestPayload = new QueryRequestPayload();


        //QueryResponse queryResponse = await client.Record.CollectionQueryAsync(
        //    tenant: "default_tenant", 
        //    database: "default_database",
        //    collectionId: "c359bdb6-c29d-4fe3-87f1-b13ec62061e5", 
        //    request: queryRequestPayload);

        //QueryResponse queryResponse = await client.Record.CollectionQueryAsync(tenant: "default_tenant", database: "default_database", collectionId: "c359bdb6-c29d-4fe3-87f1-b13ec62061e5", query: new CollectionQuery
        //{
        //    Query = new List<List<float>>
        //    {
        //        new List<float> { 0.1f, 0.2f, 0.3f },
        //        new List<float> { 0.4f, 0.5f, 0.6f }
        //    },
        //    NResults = 2,
        //    Include = new List<string> { "metadatas", "documents", "distances" }     // "metadatas", "documents", "distances"
        //});


        public static async Task TestCollectionGetAsync(string collectionName, ChromaClient? client = null)
        {
            client ??= CreateClient();

            // Get our collection
            Collection collection = await GetOrCreateCollection(collectionName, client);

            GetRequestPayloadVariant2 getRequestPayloadVariant2 = new GetRequestPayloadVariant2
            {
                Ids = new List<string> { "id1", "id2" },
                Include = new List<Include> { Include.Documents, 
                    Include.Embeddings, 
                    Include.Distances,
                    Include.Metadatas,
                    Include.Uris }
            };

            GetRequestPayload requestPayload = new GetRequestPayload
            {
                GetRequestPayloadVariant2 = getRequestPayloadVariant2,
                RawWhereFields = null
            };

            GetResponse response = await client.Record.CollectionGetAsync(tenant: "default_tenant",
                database: "default_database",
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

                foreach(var document in queryResult.ToDocuments())
                {
                    Console.WriteLine($"Document Id: {document.Id}");
                    Console.WriteLine($"Document Content: {document.Text}");
                    Console.WriteLine($"Document Embedding: {string.Join(", ", document.Embeddings ?? new List<float>())}");
                    Console.WriteLine($"Document Metadata: {string.Join(", ", document.Metadata ?? new Dictionary<string, object>())}");
                    Console.WriteLine($"Document Uri: {document.Uri}");
                }

            }
        }

        private static IList<IDictionary<string, object>> ConvertMetadatas(IList<global::Chroma.OneOf<object, global::Chroma.HashMap>>? Metadatas)
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
        public static async Task TestCollectionQueryAsync(string collectionName, ChromaClient? client = null)
        {
            client ??= CreateClient();

            // Get our collection
            Collection collection = await GetOrCreateCollection(collectionName, client);

            // Fake embeddingsPayloadVariant1 for testing (384 dimensions)
            QueryRequestPayloadVariant2 queryRequestPayloadVariant2 = new QueryRequestPayloadVariant2
            {
                QueryEmbeddings = new List<IList<float>>
                {
                    GetEmbeddingForDoc1(),
                    GetEmbeddingForDoc2()
                },
                NResults = 10,
                Include = new List<Include> {
                    Include.Documents, Include.Distances, Include.Embeddings, Include.Metadatas, Include.Uris
                }
            };

            QueryRequestPayload queryRequestPayload = new QueryRequestPayload
            {
                QueryRequestPayloadVariant2 = queryRequestPayloadVariant2,
                RawWhereFields = null
            };


            QueryResponse queryResponse = await client.Record.CollectionQueryAsync(tenant: "default_tenant",
                database: "default_database",
                collectionId: collection.Id.ToString(),
                request: queryRequestPayload,
                limit: 10,
                offset: 0
                );

            if (queryResponse != null)
            {
                if (queryResponse.Ids != null && queryResponse.Ids.Count > 0)
                {
                    IList<string>? ids = queryResponse.Ids[0];
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


        public static async Task TestCollectionSearchAsync(ChromaClient? client = null)
        {
            client ??= CreateClient();

            // Get our collection
            Collection collection2 = await client.Collection.CreateCollectionAsync(tenant: "default_tenant",
                database: "default_database",
                request: new CreateCollectionPayload
                {
                    Name = "my_collection2",
                    GetOrCreate = true,
                    //Metadata = null,
                    //Configuration = null
                });

            SearchPayloadFilter searchPayloadFilter = new SearchPayloadFilter
            {
                QueryIds = new List<string> { "id1", "id2" },
                WhereClause = null
            };

            SearchPayload searchPayload = new SearchPayload
            {
                Filter = searchPayloadFilter,
                //GroupBy = null,
                Limit = new SearchPayloadLimit { Limit = 2, Offset = 0 },
                //Rank = null,
                //Select = new SearchPayloadSelect { Keys = new List<string> { "#id", "#document", "#embedding", "#metadata", "#score" } }
                Select = new SearchPayloadSelect { Keys = new List<string> { "#id", "#document" } }
            };

            SearchRequestPayload searchRequestPayload = new SearchRequestPayload
            {
                Searches = new List<SearchPayload>
                {
                    searchPayload
                }
                //ReadLevel = ReadLevel.IndexOnly
            };

            SearchResponse searchResponse = await client.Record.CollectionSearchAsync(tenant: "default_tenant",
                database: "default_database",
                collectionId: collection2.Id.ToString(),
                request: searchRequestPayload);
        }



        public static ChromaClient CreateClient()
        {
            var client = new ChromaClient(
                 apiKey: "NotNeededForLocalhost",
                 baseUri: new Uri($"http://127.0.0.1:8000"));

            return client;
        }

        public static Task<Collection> GetOrCreateCollection1(ChromaClient? client = null)
        {
            return GetOrCreateCollection("my_collection1", client);
        }

        public static Task<Collection> GetOrCreateCollection2(ChromaClient? client = null)
        {
            return GetOrCreateCollection("my_collection2", client);
        }

        public static Task<Collection> GetOrCreateCollection3(ChromaClient? client = null)
        {
            return GetOrCreateCollection("my_collection3", client);
        }

        public static async Task<Collection> GetOrCreateCollection(string collectionName, ChromaClient? client = null)
        {
            client ??= CreateClient();

            // Get our collection
            Collection collection = await client.Collection.CreateCollectionAsync(tenant: "default_tenant",
                database: "default_database",
                request: new CreateCollectionPayload
                {
                    Name = collectionName,
                    GetOrCreate = true,
                    //Metadata = null,
                    //Configuration = null
                });

            return collection;
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
}