using ChromaDB.Library;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChromaDBTest;

// Simple embedding function for testing
public class TestEmbeddingFunction : IEmbeddingFunction
{
    private readonly int _dimensions;
    private readonly Random _random;

    public TestEmbeddingFunction()
    {
        _dimensions = 3;
        _random = new Random(42); // Fixed seed for reproducibility
    }

    public TestEmbeddingFunction(int dimension)
    {
        _dimensions = dimension;
        _random = new Random(42); // Fixed seed for reproducibility
    }

    public float[][] GenerateEmbeddings(IEnumerable<string> documents)
    {
        return documents.Select(_ => Enumerable.Range(0, _dimensions)
            .Select(__ => (float)_random.NextDouble())
            .ToArray())
            .ToArray();
    }

    public object Configuration => new { model_name = "test_embeddings" };
}
