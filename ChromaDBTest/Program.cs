// https://github.com/ssone95/ChromaDB.Client

using Chroma;
using ChromaDB.Library;
using ChromaDBTest;
using System.Text.Json;

// Make sure you have a ChromaDB server running at http://localhost:8000 before running this program.
// Tests using the new ChromaClient, which is using the new chroma api v2, so we need to use the v2 endpoint for testing.

ChromaDBClient chromaDBClient = new ChromaDBClient(host: "localhost", port: 8000);

var version = await chromaDBClient.GetVersionAsync();
Console.WriteLine($"Chroma version: {version}");

var heartbeat = await chromaDBClient.GetHeartbeatAsync();
Console.WriteLine($"Heartbeat: {heartbeat.Nanosecond_heartbeat}");

var healthcheck = await chromaDBClient.GetHealthcheckAsync();
Console.WriteLine($"Healthcheck: {healthcheck ?? "Unknown"}");

var preFlightChecks = await chromaDBClient.GetPreFlightChecksAsync();
Console.WriteLine($"MaxBatchSize: {preFlightChecks.MaxBatchSize}");
Console.WriteLine($"SupportsBase64Encoding: {preFlightChecks.SupportsBase64Encoding}");
Console.WriteLine($"AdditionalProperties: {preFlightChecks.AdditionalProperties}");

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

#region Tenants

var tenant1 = await chromaDBClient.GetOrCreateTenantAsync("Tenant1");

var tenant2 = await chromaDBClient.CreateTenantAsync("Tenant2");
// Should fail since the tenant already exists (returns null).
var tenant21 = await chromaDBClient.CreateTenantAsync("Tenant2");
var tenant22 = await chromaDBClient.GetTenantAsync("Tenant2");
// Should fail since the tenant does not exist (returns null).
var tenant31 = await chromaDBClient.GetTenantAsync("Tenant3");
var tenant32 = await chromaDBClient.CreateTenantAsync("Tenant3");
// Seems not to work (not renamed)
await chromaDBClient.UpdateTenantAsync("Tenant3", "Tenant4");

#endregion

var databases = await chromaDBClient.ListDatabasesAsync("default_tenant");
foreach (var db in databases)
{
    Console.WriteLine($"CollectionDatabase: {db.Id} {db.DatabaseName} {db.TenantName}");
}

if (!databases.Any(db => db.DatabaseName == "database1"))
{
    await chromaDBClient.CreateDatabaseAsync("database1", "default_tenant");
}

if (!databases.Any(db => db.DatabaseName == "database2"))
{
    await chromaDBClient.CreateDatabaseAsync("database2", "default_tenant");
}

if (!databases.Any(db => db.DatabaseName == "database3"))
{
    await chromaDBClient.CreateDatabaseAsync("database3", "default_tenant");
}

// Refresh the list of databases after creation
databases = await chromaDBClient.ListDatabasesAsync();

if (databases.Any(db => db.DatabaseName == "database3"))
{
    await chromaDBClient.DeleteDatabaseAsync("database3", "default_tenant");
}

// Refresh the list of databases after deletion
databases = await chromaDBClient.ListDatabasesAsync();
foreach (var db in databases)
{
    Console.WriteLine($"CollectionDatabase: {db.Id} {db.DatabaseName} {db.TenantName}");
}

// Count collections in each database
int count = await chromaDBClient.CountCollectionsAsync("database1", "default_tenant");
Console.WriteLine($"Collection count: {count}");

count = await chromaDBClient.CountCollectionsAsync("database2", "default_tenant");
Console.WriteLine($"Collection count: {count}");

var collections = await chromaDBClient.ListCollectionsAsync("database1", "default_tenant");
foreach (var collection in collections)
{
    Console.WriteLine($"Collection: {collection.CollectionName} {collection.Dimension} {collection.CollectionDatabase} {collection.CollectionTenant}");
}

var database1 = databases.FirstOrDefault(db => db.DatabaseName == "database1");
var database2 = databases.FirstOrDefault(db => db.DatabaseName == "database2");

if (database1 != null)
{
    var c10 = await database1.GetOrCreateCollection("collection10");
    var c11 = await database1.GetOrCreateCollection("collection11");

    var myCollection = await database1.GetCollectionAsync("collection10");
    await database1.DeleteCollectionAsync("collection11");

    //await database1.UpdateCollectionAsync("collection10", "collection1");

    var collectionDb1s = await database1.ListCollectionsAsync();
    foreach (var collection in collectionDb1s)
    {
        Console.WriteLine($"Collection: {collection.CollectionName} {collection.Dimension} {collection.CollectionDatabase} {collection.CollectionTenant}");
    }
}

if (database2 != null)
{
    var c20 = await database2.GetOrCreateCollection("collection20");
    var c21 = await database2.GetOrCreateCollection("collection21");

    var myCollection = await database2.GetCollectionAsync("collection20");
    await database2.DeleteCollectionAsync("collection21");

    //await database2.UpdateCollectionAsync("collection20", "collection2");

    var collectionDb2s = await database2.ListCollectionsAsync();
    foreach (var collection in collectionDb2s)
    {
        Console.WriteLine($"Collection: {collection.CollectionName} {collection.Dimension} {collection.CollectionDatabase} {collection.CollectionTenant}");
    }
}

var ids = new List<string> { "id1", "id2" };


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
    { "page", 5 },
    { "book", "All about lemons" }
};

Dictionary<string, object> meta2 = new Dictionary<string, object>
{
    { "page", 15 },
    { "book", "All about mangos" }
};

IList<IDictionary<string, object>> metadatas = new List<IDictionary<string, object>>
{
    meta1,
    meta2
};

await chromaDBClient.CollectionAddAsync("collection10", 
    ids, embeddings, documents, uris, metadatas,
    "database1", "default_tenant");



// Changes to the existing collection, so we need to use the upsert method instead of add.
documents = new List<string?> { "This book is about lemons", "This book is about mangos" };
uris = new List<string?> { "http://localhost/document1", "http://localhost/document2" };
meta1 = new Dictionary<string, object>
{
    { "page", 5 },
    { "category", "Botanic books" },
    { "book", "All about lemons" }
};

meta2 = new Dictionary<string, object>
{
    { "page", 15 },
    { "category", "Botanic books" },
    { "book", "All about mangos" }
};

metadatas = new List<IDictionary<string, object>>
{
    meta1,
    meta2
};

await chromaDBClient.CollectionUpsertAsync("collection10",
    ids, embeddings, documents, uris, metadatas,
    "database1", "default_tenant");



// CollectionGetAsync with a where filter to get documents with category "Botanic books" and page greater than 10.

var whereFilter = new WhereFilter()
    .Equals("category", "Botanic books")
    .GreaterThan("page", 10);
JsonElement whereAsJsonElement = whereFilter.ToJsonElement();
string whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
// {"$and":[{"category":"Botanic books"},{"page":{"$gt":10}}]}
Console.WriteLine($"Where Filter as JSON: {whereAsJson}");





// No restriction on ids, so we can pass null for the ids parameter.
// {"where":{"$and":[{"category":"Botanic books"},{"page":{"$gt":10}}]}}
var result = await chromaDBClient.CollectionGetAsync("collection10", 
    null, include, whereFilter, null, 10, 0,
    "database1", "default_tenant");

foreach (var document in result)
{
    Console.WriteLine($"Document Id: {document.Id}");
    Console.WriteLine($"Document Content: {document.Text}");
    Console.WriteLine($"Document Embedding: {string.Join(", ", document.Embeddings ?? new List<float>())}");
    Console.WriteLine($"Document Metadata: {string.Join(", ", document.Metadata ?? new Dictionary<string, object>())}");
    Console.WriteLine($"Document Uri: {document.Uri}");
}

IList<IList<ChromaDbDocument>> listOfDocumentList = 
    await chromaDBClient.CollectionQueryAsync("collection10", embeddings, include, null, 2, null, null, 10, 0,
    "database1", "default_tenant");

foreach (var documentList in listOfDocumentList)
{
    foreach (var document in documentList.OrderBy(d => d.Distance))
    {
        Console.WriteLine($"Document Id: {document.Id}");
        Console.WriteLine($"Document Content: {document.Text}");
        Console.WriteLine($"Document Distance: {document.Distance}");
        Console.WriteLine($"Document Embedding: {string.Join(", ", document.Embeddings ?? new List<float>())}");
        Console.WriteLine($"Document Metadata: {string.Join(", ", document.Metadata ?? new Dictionary<string, object>())}");
        Console.WriteLine($"Document Uri: {document.Uri}");
    }
}


var client = TryAGIChromaTest.CreateClient();

await TryAGIChromaTest.GetClientVersion(client);


// Not working buy get same result than ChromaDB.http file
//var myCollection = await TryAGIChromaTest.GetCollectionAsync();

// Throws an exception if the collection does not exist, so we need to create it first.


try
{
    var myCollection = await TryAGIChromaTest.GetOrCreateCollection("my_collection", client);
}
catch (Exception ex)
{
    Console.WriteLine($"E: {ex.Message}");
}

var c1 = await TryAGIChromaTest.GetOrCreateCollection("my_collection1", client);
var c2 = await TryAGIChromaTest.GetOrCreateCollection("my_collection2", client);
var c3 = await TryAGIChromaTest.GetOrCreateCollection("my_collection3", client);


var myCollection1 = await TryAGIChromaTest.GetCollectionAsync("my_collection1");
var myCollection2 = await TryAGIChromaTest.GetCollectionAsync("my_collection2");
var myCollection3 = await TryAGIChromaTest.GetCollectionAsync("my_collection3");

await TryAGIChromaTest.TestCollectionAddAsync("my_collection1");
await TryAGIChromaTest.TestCollectionAddAsync("my_collection2");
await TryAGIChromaTest.TestCollectionAddAsync("my_collection3");

await TryAGIChromaTest.TestCollectionGetAsync("my_collection1", client);
await TryAGIChromaTest.TestCollectionGetAsync("my_collection2", client);
await TryAGIChromaTest.TestCollectionGetAsync("my_collection3", client);


await TryAGIChromaTest.TestCollectionQueryAsync("my_collection1");
await TryAGIChromaTest.TestCollectionQueryAsync("my_collection2");
await TryAGIChromaTest.TestCollectionQueryAsync("my_collection3");

await TryAGIChromaTest.TestCollectionUpsertAsync("my_collection1");
await TryAGIChromaTest.TestCollectionUpsertAsync("my_collection2");
await TryAGIChromaTest.TestCollectionUpsertAsync("my_collection3");




await TryAGIChromaTest.TestListingOfDatabases();
await TryAGIChromaTest.TestCreationOfDatabase();
await TryAGIChromaTest.TestDeleteCollectionAsync();

count = await TryAGIChromaTest.TestCountCollectionsAsync();
Console.WriteLine($"Collection count: {count}");

await TryAGIChromaTest.TestCreateCollectionAsyncWithExistingCollection();



// Not working yet.
//await TryAGIChromaTest.TestCollectionSearchAsync();



/*
 * Tests using the ChromaDB.Client library.
 */

// For now ChromaDB.Client is using chroma api v1, so we need to use the v1 endpoint for testing.

//await ChromaDBClientTest.Run1();
//await ChromaDBClientTest.Run2();