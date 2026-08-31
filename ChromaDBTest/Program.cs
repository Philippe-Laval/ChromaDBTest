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


var databases = await chromaDBClient.ListDatabasesAsync();
foreach (var db in databases)
{
    Console.WriteLine($"Database: {db.Id} {db.Name} {db.Tenant}");
}

if (!databases.Any(db => db.Name == "database1"))
{
    await chromaDBClient.CreateDatabaseAsync("database1", "default_tenant");
}

if (!databases.Any(db => db.Name == "database2"))
{
    await chromaDBClient.CreateDatabaseAsync("database2", "default_tenant");
}

if (!databases.Any(db => db.Name == "database3"))
{
    await chromaDBClient.CreateDatabaseAsync("database3", "default_tenant");
}

// Refresh the list of databases after creation
databases = await chromaDBClient.ListDatabasesAsync();

if (databases.Any(db => db.Name == "database3"))
{
    await chromaDBClient.DeleteDatabaseAsync("database3", "default_tenant");
}

// Refresh the list of databases after deletion
databases = await chromaDBClient.ListDatabasesAsync();
foreach (var db in databases)
{
    Console.WriteLine($"Database: {db.Id} {db.Name} {db.Tenant}");
}

// Count collections in each database
int count = await chromaDBClient.CountCollectionsAsync("database1", "default_tenant");
Console.WriteLine($"Collection count: {count}");

count = await chromaDBClient.CountCollectionsAsync("database2", "default_tenant");
Console.WriteLine($"Collection count: {count}");

var collections = await chromaDBClient.ListCollectionsAsync("database1", "default_tenant");
foreach (var collection in collections)
{
    Console.WriteLine($"Collection: {collection.Name} {collection.Dimension} {collection.Database} {collection.Tenant}");
}

var database1 = databases.FirstOrDefault(db => db.Name == "database1");
var database2 = databases.FirstOrDefault(db => db.Name == "database2");

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
        Console.WriteLine($"Collection: {collection.Name} {collection.Dimension} {collection.Database} {collection.Tenant}");
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
        Console.WriteLine($"Collection: {collection.Name} {collection.Dimension} {collection.Database} {collection.Tenant}");
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

WhereFilter whereFilter = new WhereFilter()
    .Equals("category", "Botanic books")
    .GreaterThan("page", 10);
JsonElement whereAsJsonElement = whereFilter.ToJsonElement();
string whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
// {"$and":[{"category":"Botanic books"},{"page":{"$gt":10}}]}
Console.WriteLine($"Where Filter as JSON: {whereAsJson}");

whereFilter = new WhereFilter()
           .Equals("category", "Botanic books")
           .GreaterThan("page", 10)
           .In("language", ["en", "fr"]);
whereAsJsonElement = whereFilter.ToJsonElement();
whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
// {"$and":[{"category":"Botanic books"},{"page":{"$gt":10}},{"language":{"$in":["en","fr"]}}]}
Console.WriteLine($"Where Filter as JSON: {whereAsJson}");

whereFilter = new WhereFilter()
           .Equals("published", true)
           .Any(
               new WhereFilter().Equals("language", "en"),
               new WhereFilter().Equals("language", "fr"));
whereAsJsonElement = whereFilter.ToJsonElement();
whereAsJson = JsonSerializer.Serialize(whereAsJsonElement);
// {"$and":[{"published":true},{"$or":[{"$and":[{"language":"en"}]},{"$and":[{"language":"fr"}]}]}]}
Console.WriteLine($"Where Filter as JSON: {whereAsJson}");



// all records whose document contains a search string
WhereDocumentFilter whereDocumentFilter1 = new WhereDocumentFilter()
    .Contains("search string");
JsonElement whereDocumentAsJsonElement = whereDocumentFilter1.ToJsonElement();
string whereDocumentAsJson = JsonSerializer.Serialize(whereDocumentAsJsonElement);
// {"$and":[{"$contains":"search string"}]}
Console.WriteLine($"Where Document Filter as JSON: {whereDocumentAsJson}");

// records whose documents match the regex pattern for an email address
WhereDocumentFilter whereDocumentFilter2 = new WhereDocumentFilter()
    .Regex("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$");
whereDocumentAsJsonElement = whereDocumentFilter2.ToJsonElement();
whereDocumentAsJson = JsonSerializer.Serialize(whereDocumentAsJsonElement);
// {"$and":[{"$regex":"^[a-zA-Z0-9._%\u002B-]\u002B@[a-zA-Z0-9.-]\u002B\\.[a-zA-Z]{2,}$"}]}
Console.WriteLine($"Where Document Filter as JSON: {whereDocumentAsJson}");

// An $and operator will return results that match all the filters in the list
WhereDocumentFilter whereDocumentFilter3 = new WhereDocumentFilter()
    .Or()
    .NotContains("search_string_2")
    .NotRegex("[0-9]+")
    .All(
        new WhereDocumentFilter().Contains("search_string_1"),
        new WhereDocumentFilter().Regex("[a-z]+")
    );
whereDocumentAsJsonElement = whereDocumentFilter3.ToJsonElement();
whereDocumentAsJson = JsonSerializer.Serialize(whereDocumentAsJsonElement);
// {"$or":[{"$not_contains":"search_string_2"},{"$not_regex":"[0-9]\u002B"},{"$and":[{"$and":[{"$contains":"search_string_1"}]},{"$and":[{"$regex":"[a-z]\u002B"}]}]}]}
Console.WriteLine($"Where Document Filter as JSON: {whereDocumentAsJson}");

// An $or operator will return results that match any of the filters in the list
WhereDocumentFilter whereDocumentFilter4 = new WhereDocumentFilter()
    .Regex("[a-z]+")
    .NotRegex("[0-9]+")
    .Any(
        new WhereDocumentFilter().Contains("search_string_1"), 
        new WhereDocumentFilter().NotContains("search_string_2")
    );
whereDocumentAsJsonElement = whereDocumentFilter4.ToJsonElement();
whereDocumentAsJson = JsonSerializer.Serialize(whereDocumentAsJsonElement);
Console.WriteLine($"Where Document Filter as JSON: {whereDocumentAsJson}");

WhereDocumentFilter whereDocumentFilter5 = new WhereDocumentFilter()
    .Contains("search_string_1")
    .Or()
    .NotContains("search_string_2");
whereDocumentAsJsonElement = whereDocumentFilter5.ToJsonElement();
whereDocumentAsJson = JsonSerializer.Serialize(whereDocumentAsJsonElement);
// {"$or":[{"$contains":"search_string_1"},{"$not_contains":"search_string_2"}]}
Console.WriteLine($"Where Document Filter as JSON: {whereDocumentAsJson}");

WhereDocumentFilter whereDocumentFilter6= new WhereDocumentFilter()
    .Contains("search_string_1")
    .Regex("[a-z]+");
whereDocumentAsJsonElement = whereDocumentFilter6.ToJsonElement();
whereDocumentAsJson = JsonSerializer.Serialize(whereDocumentAsJsonElement);
// {"$and":[{"$contains":"search_string_1"},{"$regex":"[a-z]\u002B"}]}
Console.WriteLine($"Where Document Filter as JSON: {whereDocumentAsJson}");


// No restriction on ids, so we can pass null for the ids parameter.
// BUG : for now the include paramter are not sent to the server nor the paging parameters :
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