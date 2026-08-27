using System;
using System.Collections.Generic;
using System.Text;

namespace ChromaDB.Library;

/// <summary>
/// Interface for embedding functions
/// </summary>
public interface IEmbeddingFunction
{
    IList<float> GenerateEmbeddings(string document);

    /// <summary>
    /// Generate embeddings for a list of documents
    /// </summary>
    /// <param name="documents">List of document texts</param>
    /// <returns>Array of embedding vectors</returns>
    IList<IList<float>> GenerateEmbeddings(IEnumerable<string> documents);

    /// <summary>
    /// Configuration details for serialization
    /// </summary>
    object Configuration { get; }
}