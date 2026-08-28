using Chroma;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ChromaDB.Library
{
    public class ChromaDBCollection
    {
        private readonly ChromaClient _client;
        private readonly Collection _collection;
        public IEmbeddingFunction? EmbeddingFunction { get; set; }

        private static readonly JsonSerializerOptions WhereFilterSerializerOptions = new JsonSerializerOptions
        {
            Converters = { new ChromaDBWhereFilterConverter() }
        };

        public string Name => _collection.Name;
        public int? Dimension => _collection.Dimension;
        public string Database => _collection.Database;
        public string Tenant => _collection.Tenant;

        public ChromaDBCollection(Collection collection, ChromaClient client)
        {
            _collection = collection;
            _client = client;
        }



        public async Task CollectionAddAsync(string collectionName,
        string tenant = "default_tenant",
        string database = "default_database")
        {
            // Fake embeddingsPayloadVariant1 for testing (384 dimensions)
            IList<IList<float>> embeddingsPayloadVariant1 = new List<IList<float>>
            {
                GetEmbeddingForDoc3(),
                GetEmbeddingForDoc4()
            };

            var embeddingsPayload = new EmbeddingsPayload
            {
                EmbeddingsPayloadVariant1 = embeddingsPayloadVariant1,
                EmbeddingsPayloadVariant2 = null
            };

            AddCollectionRecordsPayload addCollectionRecordsPayload = new AddCollectionRecordsPayload
            {
                // required fields
                Ids = new List<string> { "id3", "id4" },
                Embeddings = embeddingsPayload,
                // optional fields
                Documents = new List<string> { "This is a document about lemons", "This is a document about mangos" }
            };

 
            // Add records to the vecItem
            await _client.Record.CollectionAddAsync(tenant: tenant,
                database: database,
                collectionId: _collection.Id.ToString(),
                request: addCollectionRecordsPayload);
        }



        public static IList<float> GetEmbeddingForDoc1()
        {
            return GetEmbedding(0.1f);
        }
        public static IList<float> GetEmbeddingForDoc2()
        {
            return GetEmbedding(0.2f);
        }

        public static IList<float> GetEmbeddingForDoc3()
        {
            return GetEmbedding(0.3f);
        }

        public static IList<float> GetEmbeddingForDoc4()
        {
            return GetEmbedding(0.4f);
        }

        public static IList<float> GetEmbedding(float value)
        {
            // Fake embeddingsPayloadVariant1 for testing (384 dimensions)
            List<float> embedding = new List<float>();

            for (int i = 0; i < 384; i++)
            {
                embedding.Add(value);
            }

            return embedding;
        }
    }
}
