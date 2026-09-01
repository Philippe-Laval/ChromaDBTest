using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ChromaDB.Library;

/// <summary>
/// Result of a query operation
/// </summary>
public class QueryResult : IEnumerable<ChromaDbDocument>
{
    /// <summary>
    /// Document IDs
    /// </summary>
    public IList<string> Ids { get; set; } = new List<string>();

    /// <summary>
    /// Document embeddings
    /// </summary>
    public IList<IList<float>?>? Embeddings { get; set; }

    /// <summary>
    /// Distance scores (lower is more similar)
    /// </summary>
    public IList<float?>? Distances { get; set; }

    /// <summary>
    /// Document metadata
    /// </summary>
    public IList<IDictionary<string, object?>?>? Metadatas { get; set; }

    /// <summary>
    /// Document contents
    /// </summary>
    public IList<string?>? Documents { get; set; }

    /// <summary>
    /// Document URIs (if available)
    /// </summary>
    public IList<string?>? Uris { get; set; }
    

    /// <summary>
    /// Gets the number of results
    /// </summary>
    public int Count => Ids.Count;

    /// <summary>
    /// Gets the documents as a list of ChromaDocument objects
    /// </summary>
    /// <returns>List of ChromaDocument objects</returns>
    public List<ChromaDbDocument> ToDocuments()
    {
        var results = new List<ChromaDbDocument>();

        for (int i = 0; i < Ids.Count; i++)
        {
            var doc = new ChromaDbDocument
            {
                Id = Ids[i],
                Embeddings = (Embeddings != null && i < Embeddings.Count) ? Embeddings[i] : null,
                Distance = (Distances != null && i < Distances.Count) ? Distances[i] : null,
                Text = (Documents != null && i < Documents.Count) ? Documents[i] : null,
                Metadata = (Metadatas != null && i < Metadatas.Count) ? Metadatas[i] : null,
                Uri = (Uris != null && i < Uris.Count) ? Uris[i] : null
            };

            results.Add(doc);
        }

        return results;
    }

    /// <summary>
    /// Gets the first document from the results
    /// </summary>
    /// <returns>First document or null if no results</returns>
    public ChromaDbDocument? FirstOrDefault()
    {
        if (Ids.Count == 0)
            return null;

        return new ChromaDbDocument
        {
            Id = Ids[0],
            Embeddings = (Embeddings != null && Embeddings.Count > 0) ? Embeddings[0] : null,
            Distance = (Distances != null && Distances.Count > 0) ? Distances[0] : null,
            Text = (Documents != null && Documents.Count > 0) ? Documents[0] : null,
            Metadata = (Metadatas != null && Metadatas.Count > 0) ? Metadatas[0] : null,
            Uri = (Uris != null && Uris.Count > 0) ? Uris[0] : null
        };
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection of ChromaDocuments
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection</returns>
    public IEnumerator<ChromaDbDocument> GetEnumerator()
    {
        for (int i = 0; i < Ids.Count; i++)
        {
            yield return new ChromaDbDocument
            {
                Id = Ids[i],
                Embeddings = (Embeddings != null && i < Embeddings.Count) ? Embeddings[i] : null,
                Distance = (Distances != null && i < Distances.Count) ? Distances[i] : null,
                Text = (Documents != null && i < Documents.Count) ? Documents[i] : null,
                Metadata = (Metadatas != null && i < Metadatas.Count) ? Metadatas[i] : null,
                Uri = (Uris != null && i < Uris.Count) ? Uris[i] : null
            };
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection of ChromaDocuments
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
