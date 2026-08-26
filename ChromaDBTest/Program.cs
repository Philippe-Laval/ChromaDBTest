// https://github.com/ssone95/ChromaDB.Client

using ChromaDBTest;
using Microsoft.VisualBasic;

// Make sure you have a ChromaDB server running at http://localhost:8000 before running this program.


// Tests using the new ChromaClient, which is using the new chroma api v2, so we need to use the v2 endpoint for testing.

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


await TryAGIChromaTest.TestCollectionUpsertAsync();


await TryAGIChromaTest.TestListingOfDatabases();
await TryAGIChromaTest.TestCreationOfDatabase();
await TryAGIChromaTest.TestDeleteCollectionAsync();

int count = await TryAGIChromaTest.TestCountCollectionsAsync();
Console.WriteLine($"Collection count: {count}");

await TryAGIChromaTest.TestCreateCollectionAsyncWithExistingCollection();



// Not working yet.
//await TryAGIChromaTest.TestCollectionSearchAsync();

await TryAGIChromaTest.TestCollectionQueryAsync();

/*
 * Tests using the ChromaDB.Client library.
 */

// For now ChromaDB.Client is using chroma api v1, so we need to use the v1 endpoint for testing.

//await ChromaDBClientTest.Run1();
//await ChromaDBClientTest.Run2();