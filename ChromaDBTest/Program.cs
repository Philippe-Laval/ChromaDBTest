// https://github.com/ssone95/ChromaDB.Client

using ChromaDBTest;


/*
 * This is a test program to test the ChromaDB.Client library.
 * It will run the tests in the Test1, Test2, and Test3 classes.
 * 
 * Make sure you have a ChromaDB server running at http://localhost:8000/api/v1/
 * before running this program.
 */

// For now ChromaDB.Client is using chromad api v1, so we need to use the v1 endpoint for testing.

//var test1 = new Test1();
//await test1.Run();

//var test2 = new Test2();
//await test2.Run();

// Test3 is using the new ChromaClient, which is using the new chroma api v2, so we need to use the v2 endpoint for testing.

var test3 = new Test3();
await test3.Run8();
