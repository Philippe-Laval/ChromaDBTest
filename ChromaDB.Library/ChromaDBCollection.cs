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
        private readonly IEmbeddingFunction _embeddingFunction;

        private static readonly JsonSerializerOptions WhereFilterSerializerOptions = new JsonSerializerOptions
        {
            Converters = { new WhereFilterConverter() }
        };

        public ChromaDBCollection(ChromaClient client, IEmbeddingFunction embeddingFunction)
        {
            _client = client;
            _embeddingFunction = embeddingFunction;
        }
    }
}
