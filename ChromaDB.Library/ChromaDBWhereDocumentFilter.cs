using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

// https://docs.trychroma.com/docs/querying-collections/full-text-search
// We support full-text search with the $contains and $not_contains operators.
// We also support regular expression pattern matching with the $regex and $not_regex operators.
// You can also use the logical operators $and and $or to combine multiple filters.
// .get and .query can handle where_document search combined with metadata filtering:

/*
 * Python doc
 
collection.get(
   where_document={"$contains": "search string"}
) 

collection.get(
   where_document={
       "$regex": "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"
   }
)

collection.query(
    query_texts=["query1", "query2"],
    where_document={
        "$and": [
            {"$contains": "search_string_1"},
            {"$regex": "[a-z]+"},
        ]
    }
)

collection.query(
    query_texts=["query1", "query2"],
    where_document={
        "$or": [
            {"$contains": "search_string_1"},
            {"$not_contains": "search_string_2"},
        ]
    }
)

collection.query(
    query_texts=["doc10", "thus spake zarathustra", ...],
    n_results=10,
    where={"metadata_field": "is_equal_to_this"},
    where_document={"$contains":"search_string"}
)
*/
namespace ChromaDB.Library;

[JsonConverter(typeof(ChromaDBWhereDocumentFilterConverter))]
public class ChromaDBWhereDocumentFilter
{
    private readonly Dictionary<string, object> _filter = new Dictionary<string, object>();
    private bool _combineWithOr = false; // Flag to indicate OR combination

    // Internal accessor for the converter
    internal bool CombineWithOr => _combineWithOr;
    internal IReadOnlyDictionary<string, object> FilterDictionary => _filter;

    /// <summary>
    /// Creates a new filter
    /// </summary>
    public ChromaDBWhereDocumentFilter() { }

    /// <summary>
    /// Specifies that the conditions added to this filter instance should be combined using OR logic.
    /// If not called, conditions are combined using AND logic (default).
    /// Note: This applies only when multiple conditions are added directly to this ChromaDBWhereFilter instance.
    /// </summary>
    /// <returns>The current ChromaDBWhereFilter instance for chaining.</returns>
    public ChromaDBWhereDocumentFilter Or()
    {
        _combineWithOr = true;
        return this;
    }

    public ChromaDBWhereDocumentFilter And()
    {
        _combineWithOr = false;
        return this;
    }

    /// <summary>
    /// Adds a contains condition to the filter
    /// </summary>
    /// <param name="searchString">Search string to match</param>
    /// <returns>This filter instance for chaining</returns>
    public ChromaDBWhereDocumentFilter Contains(string searchString)
    {
        _filter["$contains"] = searchString;
        return this;
    }

    public ChromaDBWhereDocumentFilter NotContains(string searchString)
    {
        _filter["$not_contains"] = searchString;
        return this;
    }

    public ChromaDBWhereDocumentFilter Regex(Regex regex)
    {
        _filter["$regex"] = regex.ToString();
        return this;
    }
    
    public ChromaDBWhereDocumentFilter Regex(string pattern)
    {
        _filter["$regex"] = pattern;
        return this;
    }

    public ChromaDBWhereDocumentFilter NotRegex(Regex regex)
    {
        _filter["$not_regex"] = regex.ToString();
        return this;
    }

    public ChromaDBWhereDocumentFilter NotRegex(string pattern)
    {
        _filter["$not_regex"] = pattern;
        return this;
    }

    /// <summary>
    /// Explicitly combines multiple filters with an AND operator
    /// </summary>
    /// <param name="filters">The filters to combine</param>
    /// <returns>A new filter representing the AND combination</returns>
    public ChromaDBWhereDocumentFilter And(params ChromaDBWhereDocumentFilter[] filters)
    {
        _filter["$and"] = filters.Select(f => f.ToDictionary()).ToList();
        return this;
    }

    /// <summary>
    /// Explicitly combines multiple filters with an OR operator
    /// </summary>
    /// <param name="filters">The filters to combine</param>
    /// <returns>A new filter representing the OR combination</returns>
    public ChromaDBWhereDocumentFilter Or(params ChromaDBWhereDocumentFilter[] filters)
    {
        _filter["$or"] = filters.Select(f => f.ToDictionary()).ToList();
        return this;
    }

    /// <summary>
    /// Converts this filter to a dictionary
    /// </summary>
    public Dictionary<string, object> ToDictionary() => new Dictionary<string, object>(_filter);

    /// <summary>
    /// Implicitly converts a ChromaDBWhereDocumentFilter to a Dictionary
    /// </summary>
    public static implicit operator Dictionary<string, object>(ChromaDBWhereDocumentFilter filter) => filter.ToDictionary();
}
