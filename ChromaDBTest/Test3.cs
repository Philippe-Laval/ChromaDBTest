using Chroma;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ChromaDBTest
{
    public class Test3
    {
        public async Task Run1()
        {
            var client = new ChromaClient(
                  apiKey: "test",
                  baseUri: new Uri($"http://127.0.0.1:8000"));

            string version = await client.System.VersionAsync();
            Console.WriteLine($"Chroma version: {version}");

            HeartbeatResponse heartbeat = await client.System.HeartbeatAsync();
            Console.WriteLine($"Heartbeat: {heartbeat.Nanosecond_heartbeat}");
        }

        public async Task Run2()
        {
            var client = new ChromaClient(
                  apiKey: "test",
                  baseUri: new Uri($"http://127.0.0.1:8000"));

            //var response = await client.Database.CreateDatabaseAsync("default_tenant", "database1");

            //var createDatabaseResponse = await client.Database.CreateDatabaseAsync("default_tenant", "database2");
            //var deleteDatabaseResponse = await client.Database.DeleteDatabaseAsync("default_tenant", "database2");

            // Bug for now
            //var databases = await client.Database.ListDatabasesAsync("default_tenant");
            //foreach (var database in databases)
            //{
            //    Console.WriteLine($"Database: {database.Name}");
            //}
        }

        public async Task Run3()
        {
            var client = new ChromaClient(
                  apiKey: "test",
                  baseUri: new Uri($"http://127.0.0.1:8000"));

            var count = await client.Collection.CountCollectionsAsync(tenant: "default_tenant", database: "default_database");
            Console.WriteLine($"Collection count: {count}");

            // Bug for now : throw an exception when the collection does not exist
            //Collection? myCollection = await client.Collection.GetCollectionAsync(tenant: "default_tenant", database: "default_database", collectionId: "c359bdb6-c29d-4fe3-87f1-b13ec62061e5");
            //if (myCollection != null)
            //{
            //    Console.WriteLine($"Collection: {myCollection.Name} {myCollection.Dimension} {myCollection.Database} {myCollection.Tenant}");
            //}
            //else
            //{
            //    Console.WriteLine("Collection not found.");
            //}

            var collections = await client.Collection.ListCollectionsAsync(
                tenant: "default_tenant",
                database: "default_database");

            foreach (var collection in collections)
            {
                Console.WriteLine($"Collection: {collection.Id} {collection.Name} {collection.Dimension} {collection.Database} {collection.Tenant}");
            }


            var recordCount = await client.Record.CollectionCountAsync(tenant: "default_tenant", database: "default_database", collectionId: "c359bdb6-c29d-4fe3-87f1-b13ec62061e5");
            Console.WriteLine($"Record count: {recordCount}");

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

            Collection collection2 = await client.Collection.CreateCollectionAsync(tenant: "default_tenant",
                database: "default_database",
                request: new CreateCollectionPayload
                {
                    Name = "my_collection2",
                    GetOrCreate = true,
                    //Metadata = null,
                    //Configuration = null
                });

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
        }

        public async Task Run4()
        {
            var client = new ChromaClient(
                  apiKey: "test",
                  baseUri: new Uri($"http://127.0.0.1:8000"));
            /*
             * Delete a collection
             * 
            var deleteCollectionResponse = await client.Collection.DeleteCollectionAsync(tenant: "default_tenant",
                database: "default_database",
                collectionId: collection3.Id.ToString());
            Console.WriteLine($"Delete collection response: {deleteCollectionResponse}");
            */
        }

        public async Task Run5()
        {
            var client = new ChromaClient(
                  apiKey: "test",
                  baseUri: new Uri($"http://127.0.0.1:8000"));

            // Fake embeddings for testing (384 dimensions)
            global::System.Collections.Generic.List<float> e1 = new global::System.Collections.Generic.List<float>();
            for (int i = 0; i < 384; i++)
            {
                e1.Add(0.1f);
            }

            global::System.Collections.Generic.List<float> e2 = new global::System.Collections.Generic.List<float>();
            for (int i = 0; i < 384; i++)
            {
                e2.Add(0.2f);
            }

            global::System.Collections.Generic.IList<global::System.Collections.Generic.IList<float>> embeddings = new global::System.Collections.Generic.List<global::System.Collections.Generic.IList<float>>
            {
                e1,
                e2
            };


            var embeddingsPayload = new EmbeddingsPayload
            {
                EmbeddingsPayloadVariant1 = embeddings,
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

            Collection collection2 = await client.Collection.CreateCollectionAsync(tenant: "default_tenant",
                database: "default_database",
                request: new CreateCollectionPayload
                {
                    Name = "my_collection2",
                    GetOrCreate = true,
                    //Metadata = null,
                    //Configuration = null
                });

            // Add records to the collection
            await client.Record.CollectionAddAsync(tenant: "default_tenant",
                database: "default_database",
                collectionId: collection2.Id.ToString(),
                request: addCollectionRecordsPayload);
        }


        public async Task Run6()
        {
            var client = new ChromaClient(
                  apiKey: "test",
                  baseUri: new Uri($"http://127.0.0.1:8000"));


            /* 
             * Changes the collection name
             * 
            await client.Collection.UpdateCollectionAsync(tenant: "default_tenant",
                database: "default_database",
                collectionId: "c359bdb6-c29d-4fe3-87f1-b13ec62061e5",
                request: new UpdateCollectionPayload
                {
                    NewName = "my_collection2",
                    NewConfiguration = null,
                    NewMetadata = null
                });
            */
        }

        public async Task Run7()
        {
            var client = new ChromaClient(
                  apiKey: "test",
                  baseUri: new Uri($"http://127.0.0.1:8000"));

            var countCollection = await client.Collection.CountCollectionsAsync(tenant: "default_tenant", database: "default_database");
            Console.WriteLine($"Collection count: {countCollection}");

            // Dos not work : can not found the collection
            var myCollection = await client.Collection.GetCollectionAsync(tenant: "default_tenant", database: "default_database", collectionId: "c359bdb6-c29d-4fe3-87f1-b13ec62061e5");

            List<List<float>> embeddings1 = new List<List<float>>
            {
                new List<float> { 0.1f, 0.2f },
                new List<float> { 0.3f, 0.4f }
            };

            var embeddingsPayload1 = new EmbeddingsPayload
            {
                EmbeddingsPayloadVariant1 = (IList<IList<float>>)embeddings1,
                EmbeddingsPayloadVariant2 = null
            };

            var upsertPayload = new UpsertCollectionRecordsPayload
            {
                Embeddings = embeddingsPayload1,
                Ids = new List<string> { "id1", "id2" },
                Documents = new List<string> { "This is a document about pineapple", "This is a document about oranges" }
            };


            await client.Record.CollectionUpsertAsync(tenant: "default_tenant",
                database: "default_database",
                collectionId: "c359bdb6-c29d-4fe3-87f1-b13ec62061e5",
                request: upsertPayload);


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

        }

        public async Task Run8()
        {
            var client = new ChromaClient(
                  apiKey: "test",
                  baseUri: new Uri($"http://127.0.0.1:8000"));


            Collection collection2 = await client.Collection.CreateCollectionAsync(tenant: "default_tenant",
                database: "default_database",
                request: new CreateCollectionPayload
                {
                    Name = "my_collection2",
                    GetOrCreate = true,
                    //Metadata = null,
                    //Configuration = null
                });


            //SearchPayloadFilter searchPayloadFilter = new SearchPayloadFilter
            //{
            //    QueryIds = new List<string> { "id1", "id2" },
            //    WhereClause = null
            //};

            //SearchPayloadFilter searchPayloadFilter = new SearchPayloadFilter
            //{
            //    QueryIds = null,
            //    WhereClause = null
            //};

            //SearchPayload searchPayload = new SearchPayload
            //{
            //    Filter = searchPayloadFilter,
            //    GroupBy = null,
            //    Limit = new SearchPayloadLimit { Limit = 2, Offset = 0 },
            //    Rank = null,
            //    Select = new SearchPayloadSelect { Keys = new List<string> { "metadatas", "documents", "distances" } }
            //};

            //SearchRequestPayload searchRequestPayload = new SearchRequestPayload
            //{
            //    Searches = new List<SearchPayload>
            //    {
            //        searchPayload
            //    }
            //};

            //SearchResponse searchResponse = await client.Record.CollectionSearchAsync(tenant: "default_tenant",
            //    database: "default_database",
            //    collectionId: collection2.Id.ToString(),
            //    request: searchRequestPayload);

            // Fake embeddings for testing (384 dimensions)
            global::System.Collections.Generic.List<float> e1 = new global::System.Collections.Generic.List<float>();
            for (int i = 0; i < 384; i++)
            {
                e1.Add(0.1f);
            }

            global::System.Collections.Generic.List<float> e2 = new global::System.Collections.Generic.List<float>();
            for (int i = 0; i < 384; i++)
            {
                e2.Add(0.2f);
            }

            QueryRequestPayloadVariant2 queryRequestPayloadVariant2 = new QueryRequestPayloadVariant2
            {
                QueryEmbeddings = new List<IList<float>>
                {
                    e1,
                    e2
                },
            };

            QueryRequestPayload queryRequestPayload = new QueryRequestPayload
            {
                QueryRequestPayloadVariant2 = queryRequestPayloadVariant2,
                RawWhereFields = null
            };


            QueryResponse queryResponse = await client.Record.CollectionQueryAsync(tenant: "default_tenant",
                database: "default_database",
                collectionId: collection2.Id.ToString(),
                request: queryRequestPayload,
                limit: 10,
                offset: 0
                );

            if (queryResponse != null)
            {
                {
                    if (queryResponse.Documents != null && queryResponse.Documents.Count > 0)
                    {
                        IList<string>? documents = queryResponse.Documents[0];
                    }
                }
            }

        }


    }
}