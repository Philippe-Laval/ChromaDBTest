using System;
using System.Collections.Generic;
using System.Text;

namespace ChromaDB.Library;


/// <summary>
/// A document to be stored in ChromaDB
/// </summary>
public class ChromaDbDocument
{
    /// <summary>
    /// Unique identifier for the document
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Vector embedding representation of the document
    /// </summary>
    public IList<float>? Embeddings { get; set; }

    /// <summary>
    /// Metadata associated with the document
    /// </summary>
    public IDictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Document text content
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Document URI
    /// </summary>
    public string? Uri { get; set; }

    public ChromaDbDocument()
    {
    }


    public static ChromaDbDocument Create(string id, IList<float>? embeddings, string text, IDictionary<string, object>? metadata = null)
    {
        return new ChromaDbDocument
        {
            Id = id,
            Embeddings = embeddings,
            Text = text,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Creates a new document with the specified ID and text
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="text">Document text</param>
    /// <param name="metadata">Optional metadata</param>
    /// <returns>A new ChromaDocument</returns>
    public static ChromaDbDocument Create(string id, string text, Dictionary<string, object>? metadata = null)
    {
        return new ChromaDbDocument
        {
            Id = id,
            Text = text,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Creates a new document with the specified ID and embedding
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="embedding">Embedding vector</param>
    /// <param name="metadata">Optional metadata</param>
    /// <returns>A new ChromaDocument</returns>
    public static ChromaDbDocument CreateWithEmbedding(string id, IList<float>? embeddings, IDictionary<string, object>? metadata = null)
    {
        return new ChromaDbDocument
        {
            Id = id,
            Embeddings = embeddings,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }
}