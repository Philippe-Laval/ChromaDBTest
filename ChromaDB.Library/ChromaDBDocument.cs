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
    /// Distance associated with the document
    /// </summary>
    public float? Distance { get; set; }

    /// <summary>
    /// Document text content
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Document URI
    /// </summary>
    public string? Uri { get; set; }
}