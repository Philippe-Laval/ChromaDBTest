using Chroma;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

// Launch chroma server with the following command in the terminal:
// cd C:\Users\philippe.laval
// chroma run C:\Users\philippe.laval\single_node_full.yaml

namespace ChromaDB.Library.Tests
{
    [TestClass]
    [DoNotParallelize] // Prevents all tests in this class from running in parallel
    public sealed class ChromaDBClientTest
    {
        [TestMethod]
        public async Task TestServerManagementAsync()
        {
            ChromaDBClient chromaDBClient = new ChromaDBClient(host: "localhost", port: 8000);

            var version = await chromaDBClient.GetVersionAsync();
            Assert.IsNotNull(version);
            Console.WriteLine($"Chroma version: {version}");

            var heartbeat = await chromaDBClient.GetHeartbeatAsync();
            Assert.IsNotNull(heartbeat);
            Console.WriteLine($"Heartbeat: {heartbeat.Nanosecond_heartbeat}");

            var healthcheck = await chromaDBClient.GetHealthcheckAsync();
            Assert.IsNotNull(healthcheck);
            Console.WriteLine($"Healthcheck: {healthcheck ?? "Unknown"}");
            var preFlightChecks = await chromaDBClient.GetPreFlightChecksAsync();
            Assert.IsNotNull(preFlightChecks);
            Console.WriteLine($"MaxBatchSize: {preFlightChecks.MaxBatchSize}");
            Console.WriteLine($"SupportsBase64Encoding: {preFlightChecks.SupportsBase64Encoding}");
            Console.WriteLine($"AdditionalProperties: {preFlightChecks.AdditionalProperties}");
        }

        [TestMethod]
        public async Task TestResetAsync()
        {
            ChromaDBClient chromaDBClient = new ChromaDBClient(host: "localhost", port: 8000);

            /*
             * In order to reset the ChromaDB server, 
             * you need to set the "allow_reset" option to true in the ChromaDB configuration file (config.yaml).
 
            ########################
            # HTTP server settings #
            ########################
            port: 8000
            listen_address: "0.0.0.0"
            max_payload_size_bytes: 41943040
            cors_allow_origins: ["*"]

            ####################
            # General settings #
            ####################
            persist_path: "./chroma"
            allow_reset: true # defaults to false
            sqlitedb:
              hash_type: "md5" # or "sha256"
              migration_mode: "apply" # or "validate"
            sysdb:
              sqlite:
                log_topic_namespace: "default"
                log_tenant: "default" 
             */

            await chromaDBClient.ResetAsync();

            // We start with only one database, which is the default database for the default tenant.
            var databases = await chromaDBClient.ListDatabasesAsync("default_tenant");
            Assert.IsNotNull(databases);
            Assert.HasCount(1, databases);
            Assert.AreEqual("default_tenant", databases[0].TenantName);
            Assert.AreEqual("default_database", databases[0].DatabaseName);
        }


        [TestMethod]
        public async Task TestCreateDatabaseAsync()
        {
            ChromaDBClient chromaDBClient = new ChromaDBClient(host: "localhost", port: 8000);

            // Reset the ChromaDB server to its initial state. This will delete all databases and collections.
            await chromaDBClient.ResetAsync();

            var databases = await chromaDBClient.ListDatabasesAsync("default_tenant");
            Assert.IsNotNull(databases);
            Assert.HasCount(1, databases);
            Assert.AreEqual("default_tenant", databases[0].TenantName);
            Assert.AreEqual("default_database", databases[0].DatabaseName);


            await chromaDBClient.CreateDatabaseAsync("database1", "default_tenant");
            await chromaDBClient.CreateDatabaseAsync("database2", "default_tenant");
            await chromaDBClient.CreateDatabaseAsync("database3", "default_tenant");


            // Refresh the list of databases after creation
            databases = await chromaDBClient.ListDatabasesAsync();
            Assert.HasCount(4, databases);
            Assert.IsTrue(databases.Any(db => db.DatabaseName == "database1"));
            Assert.IsTrue(databases.Any(db => db.DatabaseName == "database2"));
            Assert.IsTrue(databases.Any(db => db.DatabaseName == "database3"));
        }

        [TestMethod]
        public async Task TestDeleteDatabaseAsync()
        {
            ChromaDBClient chromaDBClient = new ChromaDBClient(host: "localhost", port: 8000);

            // Reset the ChromaDB server to its initial state. This will delete all databases and collections.
            await chromaDBClient.ResetAsync();

            var databases = await chromaDBClient.ListDatabasesAsync("default_tenant");
            Assert.IsNotNull(databases);
            Assert.HasCount(1, databases);
            Assert.AreEqual("default_tenant", databases[0].TenantName);
            Assert.AreEqual("default_database", databases[0].DatabaseName);
            Assert.IsFalse(databases.Any(db => db.DatabaseName == "database3"));

            await chromaDBClient.CreateDatabaseAsync("database3", "default_tenant");

            databases = await chromaDBClient.ListDatabasesAsync("default_tenant");
            Assert.IsNotNull(databases);
            Assert.IsTrue(databases.Any(db => db.DatabaseName == "database3"));

            await chromaDBClient.DeleteDatabaseAsync("database3", "default_tenant");

            databases = await chromaDBClient.ListDatabasesAsync("default_tenant");
            Assert.IsNotNull(databases);
            Assert.IsFalse(databases.Any(db => db.DatabaseName == "database3"));
        }

        [TestMethod]
        public async Task TestManageCollectionsAsync()
        {
            ChromaDBClient chromaDBClient = new ChromaDBClient(host: "localhost", port: 8000);

            // Reset the ChromaDB server to its initial state. This will delete all databases and collections.
            await chromaDBClient.ResetAsync();


            await chromaDBClient.CreateDatabaseAsync("database1", "default_tenant");

            // Count collections in each database
            int count = await chromaDBClient.CountCollectionsAsync("database1", "default_tenant");
            Assert.AreEqual(0, count);


            var collections = await chromaDBClient.ListCollectionsAsync("database1", "default_tenant");
            Assert.IsNotNull(collections);
            Assert.IsEmpty(collections);


            var c1 = await chromaDBClient.GetOrCreateCollection("collection1", "database1", "default_tenant");
            var c2 = await chromaDBClient.GetOrCreateCollection("collection2", "database1", "default_tenant");

            count = await chromaDBClient.CountCollectionsAsync("database1", "default_tenant");
            Assert.AreEqual(2, count);

            var collectionDb1s = await chromaDBClient.ListCollectionsAsync("database1", "default_tenant");
            Assert.IsNotNull(collectionDb1s);
            Assert.HasCount(2, collectionDb1s);
            Assert.IsTrue(collectionDb1s.Any(c => c.CollectionName == "collection1"));
            Assert.IsTrue(collectionDb1s.Any(c => c.CollectionName == "collection2"));

            var myCollection = await chromaDBClient.GetCollectionAsync("collection2", "database1", "default_tenant");
            Assert.IsNotNull(myCollection);

            await chromaDBClient.DeleteCollectionAsync("collection2", "database1", "default_tenant");

            count = await chromaDBClient.CountCollectionsAsync("database1", "default_tenant");
            Assert.AreEqual(1, count);

            collectionDb1s = await chromaDBClient.ListCollectionsAsync("database1", "default_tenant");
            Assert.IsNotNull(collectionDb1s);
            Assert.HasCount(1, collectionDb1s);
            Assert.IsTrue(collectionDb1s.Any(c => c.CollectionName == "collection1"));
            Assert.IsFalse(collectionDb1s.Any(c => c.CollectionName == "collection2"));
        }

        [TestMethod]
        public async Task TestCollectionAddAsync()
        {
            var ids = new List<string> { "id1", "id2" };

            ChromaDBClient chromaDBClient = new ChromaDBClient(host: "localhost", port: 8000);

            // Reset the ChromaDB server to its initial state. This will delete all databases and collections.
            await chromaDBClient.ResetAsync();

            await chromaDBClient.CreateDatabaseAsync("database1", "default_tenant");
            await chromaDBClient.GetOrCreateCollection("collection1", "database1", "default_tenant");

            // Include all fields in the result, but you can choose to include only the fields you need.
            var include = new List<Include> { Include.Documents,
                    Include.Embeddings,
                    Include.Distances,
                    Include.Metadatas,
                    Include.Uris };

            var documents = new List<string?> { "This is a document about lemons", "This is a document about mangos" };
            var uris = new List<string?> { "http://localhost/doc1", "http://localhost/doc2" };

            // Fake embeddingsPayloadVariant1 for testing (384 dimensions)
            FixedEmbeddingFunction embeddingFunction = new FixedEmbeddingFunction(384);
            embeddingFunction.Value = 0.1f;

            IList<float> embeddings1 = embeddingFunction.GenerateEmbeddings(documents[0]!);
            embeddingFunction.Value = 0.2f;
            IList<float> embeddings2 = embeddingFunction.GenerateEmbeddings(documents[1]!);

            IList<IList<float>> embeddings = new List<IList<float>>
            {
                embeddings1,
                embeddings2
            };

            Dictionary<string, object> meta1 = new Dictionary<string, object>
            {
                { "page", 5L },
                { "book", "All about lemons" }
            };

            Dictionary<string, object> meta2 = new Dictionary<string, object>
            {
                { "page", 15L },
                { "book", "All about mangos" }
            };

            IList<IDictionary<string, object>> metadatas = new List<IDictionary<string, object>>
            {
                meta1,
                meta2
            };

            // Add two documents to the collection
            await chromaDBClient.CollectionAddAsync("collection10",
                ids, embeddings, documents, uris, metadatas,
                "database1", "default_tenant");

            // Retrieve the documents from the collection to verify they were added correctly
            var result = await chromaDBClient.CollectionGetAsync("collection10",
                null, include, null, null, 10, 0,
                "database1", "default_tenant");

            Assert.IsNotNull(result); 
            Assert.HasCount(2, result);

            // Verify the retrieved documents match the added documents
            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(ids[i], result[i].Id);
                Assert.AreEqual(documents[i], result[i].Text);
                Assert.AreEqual(uris[i], result[i].Uri);

                Assert.AreEqual(metadatas[i]["page"], result[i].Metadata!["page"]);
                Assert.AreEqual(metadatas[i]["book"], result[i].Metadata!["book"]);

                for (int j = 0; j < embeddings[i].Count; j++)
                {
                    Assert.AreEqual(embeddings[i][j], result[i].Embeddings![j], 1e-6, $"Embedding mismatch at index {j} for document {i}");
                }
            }
        }

    }
}
