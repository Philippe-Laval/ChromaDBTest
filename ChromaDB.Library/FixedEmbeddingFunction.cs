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


    public IList<IList<float>> GenerateEmbeddings(IEnumerable<string> documents)
    {
        if (documents == null)
            throw new ArgumentNullException(nameof(documents));

        return documents
            .Select(doc => GenerateSingleEmbedding(doc ?? string.Empty, Value))
            .ToList<IList<float>>();
    }

    public IList<float> GenerateEmbeddings(string document)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        return GenerateSingleEmbedding(document, Value);
    }

    private IList<float> GenerateSingleEmbedding(string document, float value)
    {
        var result = new List<float>();
        for (var i = 0; i < _dimension; i++)
        {
            result.Add(0f);
        }

        if (string.IsNullOrWhiteSpace(document))
        {
            // For empty documents, return a zero vector
            return result;
        }

        for (int index = 0; index < _dimension; index++)
        {
            result[index] = value;
        }

        return result;
    }

}