// https://github.com/ssone95/ChromaDB.Client

using ChromaDBTest;

// Make sure you have a ChromaDB server running at http://localhost:8000 before running this program.


// Tests using the new ChromaClient, which is using the new chroma api v2, so we need to use the v2 endpoint for testing.

var tryAGIChromaTest = new TryAGIChromaTest();
await tryAGIChromaTest.Run8();
await tryAGIChromaTest.GetClientVersion();

/*
 * Tests using the ChromaDB.Client library.
 */

// For now ChromaDB.Client is using chromad api v1, so we need to use the v1 endpoint for testing.

var chromaDBClientTest = new ChromaDBClientTest();
await chromaDBClientTest.Run1();
await chromaDBClientTest.Run2();