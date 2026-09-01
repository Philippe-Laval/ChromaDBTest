using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

// Launch chroma server with the following command in the terminal:
// cd C:\Users\philippe.laval
// chroma run C:\Users\philippe.laval\single_node_full.yaml

namespace ChromaDB.Library.Tests
{
    [TestClass]
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
    }
}
