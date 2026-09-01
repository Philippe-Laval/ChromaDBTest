using System;
using System.Collections.Generic;
using System.Text;

namespace ChromaDB.Library.Tests
{
    [TestClass]
    [DoNotParallelize] // Prevents all tests in this class from running in parallel
    public sealed class ChromaDBTenantTest
    {
        [TestMethod]
        public async Task TestServerManagementAsync()
        {
            ChromaDBClient chromaDBClient = new ChromaDBClient(host: "localhost", port: 8000);


        }
    }
}
