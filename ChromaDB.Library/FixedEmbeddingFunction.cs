using System;
using System.Collections.Generic;
using System.Text;

namespace ChromaDB.Library;

/// <summary>
/// A fixed embedding function that always returns the same embeddings
/// </summary>
public class FixedEmbeddingFunction : IEmbeddingFunction
{
    private readonly int _dimension;
    public float Value { get; set; } = 0.1f;

    public FixedEmbeddingFunction(int dimension, float value = 0.1f)
    {
        _dimension = dimension;
        Value = value;
    }

    public object Configuration => new { Type = "FixedEmbeddingFunction", Dimension = _dimension };


    public float[][] GenerateEmbeddings(IEnumerable<string> documents)
    {
        if (documents == null)
            throw new ArgumentNullException(nameof(documents));

        return documents
            .Select(doc => GenerateSingleEmbedding(doc ?? string.Empty, Value))
            .ToArray();
    }

    private float[] GenerateSingleEmbedding(string document, float value)
    {
        if (string.IsNullOrWhiteSpace(document))
        {
            // For empty documents, return a zero vector
            return new float[_dimension];
        }

        var result = new float[_dimension];

        for (int index = 0; index < _dimension; index++)
        {
            result[index] = value;
        }

        return result;
    }

}