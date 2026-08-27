// https://github.com/ssone95/ChromaDB.Client

using ChromaDB.Library;
using ChromaDBTest;
using Microsoft.VisualBasic;

// Make sure you have a ChromaDB server running at http://localhost:8000 before running this program.
// Tests using the new ChromaClient, which is using the new chroma api v2, so we need to use the v2 endpoint for testing.

ChromaDBClient chromaDBClient = new ChromaDBClient(host: "localhost", port: 8000);

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

    var collectionDb1s = await database1.ListCollectionsAsync();
    foreach (var collection in collectionDb1s)
    {
        Console.WriteLine($"Collection: {collection.Name} {collection.Dimension} {collection.Database} {collection.Tenant}");
    }

    var myCollection = await database1.GetCollectionAsync("collection10");
}

if (database2 != null)
{
    var c20 = await database2.GetOrCreateCollection("collection20");
    var c21 = await database2.GetOrCreateCollection("collection21");

    var collectionDb2s = await database2.ListCollectionsAsync();
    foreach (var collection in collectionDb2s)
    {
        Console.WriteLine($"Collection: {collection.Name} {collection.Dimension} {collection.Database} {collection.Tenant}");
    }

    var myCollection = await database2.GetCollectionAsync("collection20");
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