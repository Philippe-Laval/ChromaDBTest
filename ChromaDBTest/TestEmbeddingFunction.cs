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


    public IList<IList<float>> GenerateEmbeddings(IEnumerable<string> documents)
    {
        if (documents == null)
            throw new ArgumentNullException(nameof(documents));

        return documents
            .Select(doc => GenerateSingleEmbedding())
            .ToList<IList<float>>();
    }

    public IList<float> GenerateEmbeddings(string document)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        return GenerateSingleEmbedding();
    }

    public IList<float> GenerateSingleEmbedding()
    {
        return Enumerable.Range(0, _dimensions)
            .Select(__ => (float)_random.NextDouble())
            .ToList<float>();
    }




    public object Configuration => new { model_name = "test_embeddings" };
}
