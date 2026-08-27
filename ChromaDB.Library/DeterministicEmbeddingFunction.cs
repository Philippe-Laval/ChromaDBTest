using Chroma;

namespace ChromaDB.Library;

/// <summary>
/// Deterministic embedding function that creates reproducible embeddings
/// based on word frequencies for testing purposes.
/// </summary>
public class DeterministicEmbeddingFunction : IEmbeddingFunction
{
    private readonly int _dimension;
    private readonly Random _random;

    /// <summary>
    /// Creates a new deterministic embedding function with the specified dimension.
    /// </summary>
    /// <param name="dimension">Dimension of the embeddings to generate</param>
    public DeterministicEmbeddingFunction(int dimension)
    {
        _dimension = dimension;
        _random = new Random(42); // Fixed seed for deterministic results
    }

    /// <summary>
    /// Configuration for serialization
    /// </summary>
    public object Configuration => new { Type = "DeterministicEmbeddingFunction", Dimension = _dimension };

    /// <summary>
    /// Generates embeddings based on word frequencies in the documents.
    /// </summary>
    /// <param name="documents">Documents to generate embeddings for</param>
    /// <returns>Array of embedding vectors</returns>
    public IList<IList<float>> GenerateEmbeddings(IEnumerable<string> documents)
    {
        if (documents == null)
            throw new ArgumentNullException(nameof(documents));

        return documents
            .Select(doc => GenerateSingleEmbedding(doc ?? string.Empty))
            .ToList<IList<float>>();
    }

    public IList<float> GenerateEmbeddings(string document)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        return GenerateSingleEmbedding(document);   
    }

    private IList<float> GenerateSingleEmbedding(string document)
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

        // Normalize the text: lowercase, remove punctuation, split to words
        var words = new string(document.ToLowerInvariant()
            .Select(c => char.IsPunctuation(c) ? ' ' : c)
            .ToArray())
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
            return result;

        // Create a deterministic mapping from words to vector positions
        var wordIndices = new Dictionary<string, int>();

        foreach (var word in words.Distinct())
        {
            // Deterministically assign each unique word an index based on its hash
            int hash = word.GetHashCode();
            int index = Math.Abs(hash % _dimension);

            if (!wordIndices.ContainsKey(word))
            {
                wordIndices[word] = index;
            }
        }

        // Count word frequencies
        var wordCounts = words
            .GroupBy(w => w)
            .ToDictionary(g => g.Key, g => g.Count());

        // Populate the embedding based on word frequencies
        foreach (var word in wordCounts.Keys)
        {
            int index = wordIndices[word];
            float value = (float)wordCounts[word] / words.Length; // Normalize by document length
            result[index] += value;
        }

        // Normalize the vector to unit length
        float magnitude = (float)Math.Sqrt(result.Sum(x => x * x));
        if (magnitude > 0)
        {
            for (int i = 0; i < result.Count; i++)
            {
                result[i] /= magnitude;
            }
        }

        return result;
    }
}