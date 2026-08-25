using ChromaDB.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChromaDBTest
{
    public class ChromaDBClientTest
    {
        public async Task Run1()
        {
            // create a ChromaClient with configuration options
            var configOptions = new ChromaConfigurationOptions(uri: "http://localhost:8000/api/v1/");
            using var httpClient = new HttpClient();
            var client = new ChromaClient(configOptions, httpClient);

            // create a collection to store your data.

            var collection = await client.GetOrCreateCollection("movies");
            var collectionClient = new ChromaCollectionClient(collection, configOptions, httpClient);

            // Add data to your collection.
            // The data includes movie IDs, embeddings representing the movie description,
            // and metadata containing the movie title.

            List<string> movieIds = new List<string> { "1", "2", "3", "4", "5" };

            List<ReadOnlyMemory<float>> descriptionEmbeddings = new List<ReadOnlyMemory<float>>
            {
               new float[] { 0.10022575f, -0.23998135f },
               new float[] { 0.10327095f, 0.2563685f },
               new float[] { 0.095857024f, -0.201278f },
               new float[] { 0.106827796f, 0.21676421f },
               new float[] { 0.09568083f, -0.21177962f }
            };
            
            List<Dictionary<string, object>> metadata = new List<Dictionary<string, object>>
            {
               new Dictionary<string, object> { ["Title"] = "The Lion King" },
               new Dictionary<string, object> { ["Title"] = "Inception" },
               new Dictionary<string, object> { ["Title"] = "Toy Story" },
               new Dictionary<string, object> { ["Title"] = "Pulp Fiction" },
               new Dictionary<string, object> { ["Title"] = "Shrek" }
            };

            await collectionClient.Add(movieIds, descriptionEmbeddings, metadata);

            // perform a vector search to query the data

            List<ReadOnlyMemory<float>> queryEmbedding = new List<ReadOnlyMemory<float>> { new float[] { 0.12217915f, -0.034832448f } };
            var queryResult = await collectionClient.Query(
                queryEmbeddings: queryEmbedding,
                nResults: 2,
                include: ChromaQueryInclude.Metadatas | ChromaQueryInclude.Distances
            );

            foreach (var result in queryResult)
            {
                foreach (var item in result)
                {
                    Console.WriteLine($"Title: {(string)item.Metadata["Title"] ?? string.Empty} {(item.Distance)}");
                }
            }

        }

        public async Task Run2()
        {
            var configOptions = new ChromaConfigurationOptions(uri: "http://localhost:8000/api/v1/");
            using var httpClient = new HttpClient();
            var client = new ChromaClient(configOptions, httpClient);

            //Console.WriteLine(await client.GetVersion());

            var string5Collection = await client.GetOrCreateCollection("string5");
            var string5Client = new ChromaCollectionClient(string5Collection, configOptions, httpClient);

            await string5Client.Add(["340a36ad-c38a-406c-be38-250174aee5a4"], embeddings: [new([1f, 0.5f, 0f, -0.5f, -1f])]);

            var getResult = await string5Client.Get("340a36ad-c38a-406c-be38-250174aee5a4", include: ChromaGetInclude.Metadatas | ChromaGetInclude.Documents | ChromaGetInclude.Embeddings);
            Console.WriteLine($"ID: {getResult!.Id}");

            var queryData = await string5Client.Query([new([1f, 0.5f, 0f, -0.5f, -1f]), new([1.5f, 0f, 2f, -1f, -1.5f])], include: ChromaQueryInclude.Metadatas | ChromaQueryInclude.Distances);
            foreach (var item in queryData)
            {
                foreach (var entry in item)
                {
                    Console.WriteLine($"ID: {entry.Id} | Distance: {entry.Distance}");
                }
            }
        }

    }
}
