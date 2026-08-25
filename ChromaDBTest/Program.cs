// https://github.com/ssone95/ChromaDB.Client

using ChromaDBTest;
using Microsoft.VisualBasic;

// Make sure you have a ChromaDB server running at http://localhost:8000 before running this program.


// Tests using the new ChromaClient, which is using the new chroma api v2, so we need to use the v2 endpoint for testing.

await TryAGIChromaTest.GetClientVersion();
await TryAGIChromaTest.TestListingOfDatabases();
await TryAGIChromaTest.TestCreationOfDatabase();
//await TryAGIChromaTest.TestDeleteCollectionAsync();



await TryAGIChromaTest.TestCollectionQueryAsync();

/*
 * Tests using the ChromaDB.Client library.
 */

// For now ChromaDB.Client is using chroma api v1, so we need to use the v1 endpoint for testing.

//await ChromaDBClientTest.Run1();
//await ChromaDBClientTest.Run2();