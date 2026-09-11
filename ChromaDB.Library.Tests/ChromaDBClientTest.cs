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
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

            ChromaDBClient chromaDBClient = new ChromaDBClient(host: "localhost", port: 8000);

            var version = await chromaDBClient.GetVersionAsync(cancellationToken);
            Assert.IsNotNull(version);
            Console.WriteLine($"Chroma version: {version}");

            var heartbeat = await chromaDBClient.GetHeartbeatAsync(cancellationToken);
            Assert.IsNotNull(heartbeat);
            Console.WriteLine($"Heartbeat: {heartbeat.Nanosecond_heartbeat}");

            var healthcheck = await chromaDBClient.GetHealthcheckAsync(cancellationToken);
            Assert.IsNotNull(healthcheck);
            Console.WriteLine($"Healthcheck: {healthcheck ?? "Unknown"}");
            var preFlightChecks = await chromaDBClient.GetPreFlightChecksAsync(cancellationToken);
            Assert.IsNotNull(preFlightChecks);
            Console.WriteLine($"MaxBatchSize: {preFlightChecks.MaxBatchSize}");
            Console.WriteLine($"SupportsBase64Encoding: {preFlightChecks.SupportsBase64Encoding}");
            Console.WriteLine($"AdditionalProperties: {preFlightChecks.AdditionalProperties}");
        }

        [TestMethod]
        public async Task TestResetAsync()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

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

            await chromaDBClient.ResetAsync(cancellationToken);

            // We start with only one database, which is the default database for the default tenant.
            var databases = await chromaDBClient.ListDatabasesAsync("default_tenant", cancellationToken);
            Assert.IsNotNull(databases);
            Assert.HasCount(1, databases);
            Assert.AreEqual("default_tenant", databases[0].TenantName);
            Assert.AreEqual("default_database", databases[0].DatabaseName);
        }

        [TestMethod]
        public async Task TestCreateTenantAsync()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

            ChromaDBClient chromaDBClient = new ChromaDBClient(host: "localhost", port: 8000);

            // Reset the ChromaDB server to its initial state. This will delete all databases and collections.
            await chromaDBClient.ResetAsync(cancellationToken);

            ChromaDBTenant? chromaDBTenant= await chromaDBClient.CreateTenantAsync("tenant1", cancellationToken);
            Assert.IsNotNull(chromaDBTenant);

            chromaDBTenant = await chromaDBClient.CreateTenantAsync("tenant1", cancellationToken);
            Assert.IsNull(chromaDBTenant);
        }

        [TestMethod]
        public async Task TestGetTenantAsync()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

            ChromaDBClient chromaDBClient = new ChromaDBClient(host: "localhost", port: 8000);

            // Reset the ChromaDB server to its initial state. This will delete all databases and collections.
            await chromaDBClient.ResetAsync(cancellationToken);

            ChromaDBTenant? chromaDBTenant = await chromaDBClient.CreateTenantAsync("tenant1", cancellationToken);
            Assert.IsNotNull(chromaDBTenant);

            chromaDBTenant = await chromaDBClient.GetTenantAsync("tenant1", cancellationToken);
            Assert.IsNotNull(chromaDBTenant);
        }

        [TestMethod]
        public async Task TestGetOrCreateTenantAsync()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

            ChromaDBClient chromaDBClient = new ChromaDBClient(host: "localhost", port: 8000);

            // Reset the ChromaDB server to its initial state. This will delete all databases and collections.
            await chromaDBClient.ResetAsync(cancellationToken);

            ChromaDBTenant? chromaDBTenant = await chromaDBClient.GetOrCreateTenantAsync("tenant1", cancellationToken);
            Assert.IsNotNull(chromaDBTenant);

            chromaDBTenant = await chromaDBClient.GetOrCreateTenantAsync("tenant1", cancellationToken);
            Assert.IsNotNull(chromaDBTenant);
        }

        [TestMethod]
        public async Task TestCreateDatabaseAsync()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

            ChromaDBClient chromaDBClient = new ChromaDBClient(host: "localhost", port: 8000);

            // Reset the ChromaDB server to its initial state. This will delete all databases and collections.
            await chromaDBClient.ResetAsync(cancellationToken);

            var databases = await chromaDBClient.ListDatabasesAsync("default_tenant", cancellationToken);
            Assert.IsNotNull(databases);
            Assert.HasCount(1, databases);
            Assert.AreEqual("default_tenant", databases[0].TenantName);
            Assert.AreEqual("default_database", databases[0].DatabaseName);


            await chromaDBClient.CreateDatabaseAsync("default_tenant", "database1", cancellationToken);
            await chromaDBClient.CreateDatabaseAsync("default_tenant", "database2", cancellationToken);
            await chromaDBClient.CreateDatabaseAsync("default_tenant", "database3", cancellationToken);


            // Refresh the list of databases after creation
            databases = await chromaDBClient.ListDatabasesAsync("default_tenant", cancellationToken);
            Assert.HasCount(4, databases);
            Assert.IsTrue(databases.Any(db => db.DatabaseName == "database1"));
            Assert.IsTrue(databases.Any(db => db.DatabaseName == "database2"));
            Assert.IsTrue(databases.Any(db => db.DatabaseName == "database3"));
        }

        [TestMethod]
        public async Task TestDeleteDatabaseAsync()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

            ChromaDBClient chromaDBClient = new ChromaDBClient(host: "localhost", port: 8000);

            // Reset the ChromaDB server to its initial state. This will delete all databases and collections.
            await chromaDBClient.ResetAsync(cancellationToken);

            var databases = await chromaDBClient.ListDatabasesAsync("default_tenant", cancellationToken);
            Assert.IsNotNull(databases);
            Assert.HasCount(1, databases);
            Assert.AreEqual("default_tenant", databases[0].TenantName);
            Assert.AreEqual("default_database", databases[0].DatabaseName);
            Assert.IsFalse(databases.Any(db => db.DatabaseName == "database3"));

            await chromaDBClient.CreateDatabaseAsync("default_tenant", "database3", cancellationToken);

            databases = await chromaDBClient.ListDatabasesAsync("default_tenant", cancellationToken);
            Assert.IsNotNull(databases);
            Assert.IsTrue(databases.Any(db => db.DatabaseName == "database3"));

            await chromaDBClient.DeleteDatabaseAsync("default_tenant", "database3", cancellationToken);

            databases = await chromaDBClient.ListDatabasesAsync("default_tenant", cancellationToken);
            Assert.IsNotNull(databases);
            Assert.IsFalse(databases.Any(db => db.DatabaseName == "database3"));
        }

        [TestMethod]
        public async Task TestManageCollectionsAsync()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

            ChromaDBClient chromaDBClient = new ChromaDBClient(host: "localhost", port: 8000);

            // Reset the ChromaDB server to its initial state. This will delete all databases and collections.
            await chromaDBClient.ResetAsync(cancellationToken);


            await chromaDBClient.CreateDatabaseAsync("default_tenant", "database1", 
                cancellationToken: cancellationToken);

            // Count collections in each database
            int count = await chromaDBClient.CountCollectionsAsync("default_tenant", "database1", 
                cancellationToken: cancellationToken);
            Assert.AreEqual(0, count);


            var collections = await chromaDBClient.ListCollectionsAsync("default_tenant", "database1", 
                cancellationToken: cancellationToken);
            Assert.IsNotNull(collections);
            Assert.IsEmpty(collections);


            var c1 = await chromaDBClient.GetOrCreateCollection("default_tenant", "database1", "collection1", 
                collectionConfiguration: ChromaDBClient.SetupCollectionConfiguration(Chroma.Space.Cosine),
                cancellationToken: cancellationToken);
            var c2 = await chromaDBClient.GetOrCreateCollection("default_tenant", "database1", "collection2", 
                collectionConfiguration: ChromaDBClient.SetupCollectionConfiguration(Chroma.Space.Cosine),
                cancellationToken: cancellationToken);

            count = await chromaDBClient.CountCollectionsAsync("default_tenant", "database1",
                cancellationToken: cancellationToken);
            Assert.AreEqual(2, count);

            var collectionDb1s = await chromaDBClient.ListCollectionsAsync("default_tenant", "database1", 
                cancellationToken: cancellationToken);
            Assert.IsNotNull(collectionDb1s);
            Assert.HasCount(2, collectionDb1s);
            Assert.IsTrue(collectionDb1s.Any(c => c.CollectionName == "collection1"));
            Assert.IsTrue(collectionDb1s.Any(c => c.CollectionName == "collection2"));

            var myCollection = await chromaDBClient.GetCollectionAsync("default_tenant", "database1", "collection2", 
                cancellationToken: cancellationToken);
            Assert.IsNotNull(myCollection);

            await chromaDBClient.DeleteCollectionAsync("default_tenant", "database1", "collection2", 
                cancellationToken: cancellationToken);

            count = await chromaDBClient.CountCollectionsAsync("default_tenant", "database1", 
                cancellationToken: cancellationToken);
            Assert.AreEqual(1, count);

            collectionDb1s = await chromaDBClient.ListCollectionsAsync("default_tenant", "database1", 
                cancellationToken: cancellationToken);
            Assert.IsNotNull(collectionDb1s);
            Assert.HasCount(1, collectionDb1s);
            Assert.IsTrue(collectionDb1s.Any(c => c.CollectionName == "collection1"));
            Assert.IsFalse(collectionDb1s.Any(c => c.CollectionName == "collection2"));
        }

        [TestMethod]
        public async Task TestCollectionAddAsync()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

            var ids = new List<string> { "id1", "id2" };

            ChromaDBClient chromaDBClient = new ChromaDBClient(host: "localhost", port: 8000);

            // Reset the ChromaDB server to its initial state. This will delete all databases and collections.
            await chromaDBClient.ResetAsync(cancellationToken);

            await chromaDBClient.CreateDatabaseAsync("default_tenant", "database1", cancellationToken);
            await chromaDBClient.GetOrCreateCollection("default_tenant", "database1", "collection1", 
                collectionConfiguration: ChromaDBClient.SetupCollectionConfiguration(Chroma.Space.Cosine),
                cancellationToken: cancellationToken);

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
            await chromaDBClient.CollectionAddAsync("default_tenant",
                "database1", "collection1", ids, embeddings, documents,
                uris, metadatas, cancellationToken);

            // Retrieve the documents from the collection to verify they were added correctly
            var result = await chromaDBClient.CollectionGetAsync("default_tenant",
                "database1", "collection1", null, include, null, null,
                10, 0, cancellationToken);

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

        [TestMethod]
        public async Task TestCollectionGetAsync_WithoutFilter()
        {
            await SetupCollection();
        }

        [TestMethod]
        public async Task TestCollectionGetAsync_WithFilter()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

            ChromaDBClient chromaDBClient = new ChromaDBClient(host: "localhost", port: 8000);

            await SetupCollection();

            // Include all fields in the result, but you can choose to include only the fields you need.
            var include = new List<Include> { Include.Documents,
                    Include.Embeddings,
                    Include.Distances,
                    Include.Metadatas,
                    Include.Uris };

            var whereFilter = new WhereFilter()
                    .GreaterThan("page", 10);
            JsonElement whereAsJsonElement = whereFilter.ToJsonElement();
            string whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
            // {"page":{"$gt":10}}
            Assert.AreEqual("""{"page":{"$gt":10}}""", whereAsJson);

            // Retrieve the documents from the collection to verify they were added correctly
            var result = await chromaDBClient.CollectionGetAsync("default_tenant",
                "database1", "collection1", null, include, whereFilter, null,
                10, 0, cancellationToken);

            Assert.IsNotNull(result);
            Assert.HasCount(1, result);
        }

        [TestMethod]
        public async Task TestCollectionUpsertAsync()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

            ChromaDBClient chromaDBClient = new ChromaDBClient(host: "localhost", port: 8000);

            await SetupCollection();

            // Changes to the existing collection, so we need to use the upsert method instead of add.

            var ids = new List<string> { "id1", "id2" };

            var documents = new List<string?> { "This book is about lemons", "This book is about mangos" };
            var uris = new List<string?> { "http://localhost/document1", "http://localhost/document2" };

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

            var meta1 = new Dictionary<string, object>
            {
                { "page", 6L },
                { "category", "Botanic books" },
                { "book", "All about lemons" }
            };

            var meta2 = new Dictionary<string, object>
            {
                { "page", 16L },
                { "category", "Botanic books" },
                { "book", "All about mangos" }
            };

            IList<IDictionary<string, object>> metadatas = new List<IDictionary<string, object>>
            {
                meta1,
                meta2
            };

            await chromaDBClient.CollectionUpsertAsync("default_tenant",
                "database1", "collection1", ids, embeddings, documents,
                uris, metadatas);


            // Retrieve the documents from the collection to verify they were modified correctly

            // Include all fields in the result, but you can choose to include only the fields you need.
            var include = new List<Include> { Include.Documents,
                    Include.Embeddings,
                    Include.Distances,
                    Include.Metadatas,
                    Include.Uris };

            var result = await chromaDBClient.CollectionGetAsync("default_tenant",
                "database1", "collection1", null, include, null, null,
                10, 0, cancellationToken);

            Assert.IsNotNull(result);
            Assert.HasCount(2, result);

            // Verify the retrieved documents match the added documents
            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(ids[i], result[i].Id);
                Assert.AreEqual(documents[i], result[i].Text);
                Assert.AreEqual(uris[i], result[i].Uri);

                Assert.AreEqual(metadatas[i]["page"], result[i].Metadata!["page"]);
                Assert.AreEqual(metadatas[i]["category"], result[i].Metadata!["category"]);
                Assert.AreEqual(metadatas[i]["book"], result[i].Metadata!["book"]);

                for (int j = 0; j < embeddings[i].Count; j++)
                {
                    Assert.AreEqual(embeddings[i][j], result[i].Embeddings![j], 1e-6, $"Embedding mismatch at index {j} for document {i}");
                }
            }
        }

        [TestMethod]
        public async Task TestCollectionQueryAsync()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

            ChromaDBClient chromaDBClient = new ChromaDBClient(host: "localhost", port: 8000);

            await SetupCollection();

            var documents = new List<string?> { "This is a document about lemons", "This is a document about mangos" };

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

            // Include all fields in the result, but you can choose to include only the fields you need.
            var include = new List<Include> { Include.Documents,
                    Include.Embeddings,
                    Include.Distances,
                    Include.Metadatas,
                    Include.Uris };

            IList<IList<ChromaDbDocument>> listOfDocumentList =
                await chromaDBClient.CollectionQueryAsync("default_tenant", "database1", "collection1", embeddings, include, null, 2, null, null,
                10, 0, cancellationToken);

            foreach (var documentList in listOfDocumentList)
            {
                foreach (var document in documentList.OrderBy(d => d.Distance))
                {
                    Console.WriteLine($"Document Id: {document.Id}");
                    Console.WriteLine($"Document Content: {document.Text}");
                    Console.WriteLine($"Document Distance: {document.Distance}");
                    Console.WriteLine($"Document Embedding: {string.Join(", ", document.Embeddings ?? new List<float>())}");
                    Console.WriteLine($"Document Metadata: {string.Join(", ", document.Metadata ?? new Dictionary<string, object?>())}");
                    Console.WriteLine($"Document Uri: {document.Uri}");
                }
            }
        }

        private async Task SetupCollection()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

            ChromaDBClient chromaDBClient = new ChromaDBClient(host: "localhost", port: 8000);

            // Reset the ChromaDB server to its initial state. This will delete all databases and collections.
            await chromaDBClient.ResetAsync(cancellationToken);

            await chromaDBClient.CreateDatabaseAsync("default_tenant", "database1", 
                cancellationToken: cancellationToken);

            await chromaDBClient.GetOrCreateCollection("default_tenant", "database1", "collection1", 
                collectionConfiguration: ChromaDBClient.SetupCollectionConfiguration(Chroma.Space.Cosine),
                collectionMetadata: null,
                cancellationToken: cancellationToken);

            var ids = new List<string> { "id1", "id2" };

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
            await chromaDBClient.CollectionAddAsync("default_tenant",
                "database1", "collection1", ids, embeddings, documents,
                uris, metadatas, cancellationToken);

            // Retrieve the documents from the collection to verify they were added correctly

            // Include all fields in the result, but you can choose to include only the fields you need.
            var include = new List<Include> { Include.Documents,
                    Include.Embeddings,
                    Include.Distances,
                    Include.Metadatas,
                    Include.Uris };

            var result = await chromaDBClient.CollectionGetAsync("default_tenant",
                "database1", "collection1", null, include, null, null,
                10, 0, cancellationToken);

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
