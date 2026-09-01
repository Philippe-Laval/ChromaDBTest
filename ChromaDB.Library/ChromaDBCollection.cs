using Chroma;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ChromaDB.Library
{
    public class ChromaDBCollection
    {
        private readonly ChromaClient _client;
        private readonly Collection _collection;
        public IEmbeddingFunction? EmbeddingFunction { get; set; }

        public Guid Id => _collection.Id;
        public string CollectionName => _collection.Name;
        public int? Dimension => _collection.Dimension;
        public string CollectionDatabase => _collection.Database;
        public string CollectionTenant => _collection.Tenant;

        public ChromaDBCollection(Collection collection, ChromaClient client)
        {
            _collection = collection;
            _client = client;
        }

        /// <summary>
        /// Get records from a collection based on various parameters such as ids, include, where conditions, limit, and offset.
        /// </summary>
        /// <param name="ids">If indicated, restrict the query to the list of ids.</param>
        /// <param name="include">If indicated, specify which related data to include in the query.</param>
        /// <param name="where">If indicated, restrict the query to the specified conditions in metadatas.</param>
        /// <param name="whereDocument">If indicated, restrict the query to the specified conditions in documents.</param>
        /// <param name="limit">If indicated, limit the number of results returned.</param>
        /// <param name="offset">If indicated, specify the number of results to skip.</param>
        /// <returns>A list of ChromaDocument objects matching the query parameters.</returns>
        public async Task<List<ChromaDbDocument>> CollectionGetAsync(List<string>? ids,
            List<Include>? include,
            WhereFilter? where,
            WhereDocumentFilter? whereDocument,
            int? limit,
            int? offset)
        {
            List<ChromaDbDocument> result = new List<ChromaDbDocument>();

            // Get our collection
            Collection collection = await _client.Collection.CreateCollectionAsync(tenant: CollectionTenant,
                 database: CollectionDatabase,
                 request: new CreateCollectionPayload
                 {
                     Name = _collection.Name,
                     GetOrCreate = true,
                     Metadata = null,
                     Configuration = null
                 });

            RawWhereFields? rawWhereFields = null;

            if (where is not null || whereDocument is not null)
            {
                // Handle the where and whereDocument conditions here
                rawWhereFields = new RawWhereFields
                {
                    // It is mandatory to convert the WhereFilter to a JsonElement for the RawWhereFields
                    Where = where?.ToJsonElement(),
                    // It is mandatory to convert the WhereDocumentFilter to a JsonElement for the RawWhereFields
                    WhereDocument = whereDocument?.ToJsonElement()
                };
            }

            GetRequestPayloadVariant2 getRequestPayloadVariant2 = new GetRequestPayloadVariant2
            {
                Ids = ids,
                Include = include,
                Limit = limit,
                Offset = offset
            };

            GetRequestPayload requestPayload = new GetRequestPayload
            {
                GetRequestPayloadVariant2 = getRequestPayloadVariant2,
                RawWhereFields = rawWhereFields
            };

            GetResponse response = await _client.Record.CollectionGetAsync(tenant: CollectionTenant,
                database: CollectionDatabase,
                collectionId: _collection.Id.ToString(),
                request: requestPayload);

            if (response != null)
            {
                QueryResult queryResult = new QueryResult
                {
                    Ids = response.Ids,
                    //Distances = response.Distances,
                    Embeddings = response.Embeddings,
                    Documents = response.Documents,
                    Metadatas = ConvertMetadatas(response.Metadatas),
                    Uris = response.Uris
                };

                result = queryResult.ToDocuments();
            }

            return result;
        }

        /// <summary>
        /// Converts a collection of Chroma metadata items into a list of dictionaries with string keys and object values.
        /// </summary>
        /// <param name="Metadatas">The collection of metadata items to convert, where each item can be either a plain object or a HashMap. Can be
        /// null.</param>
        /// <returns>A list of dictionaries containing the converted metadata. Returns an empty list if <paramref name="Metadatas"/>
        /// is null.</returns>
        private IList<IDictionary<string, object>> ConvertMetadatas(IList<Chroma.HashMap?>? Metadatas)
        {
            var result = new List<IDictionary<string, object>>();

            if (Metadatas != null)
            {
                foreach (var metadata in Metadatas)
                {
                    Dictionary<string, object>? dict = null;

                    if (metadata is not null)
                    {
                        dict = new Dictionary<string, object>();
                        foreach (var kvp in metadata.AdditionalProperties)
                        {
                            dict[kvp.Key] = kvp.Value;
                        }
                    }

                    if (dict is not null)
                    {
                        result.Add(dict);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Given a list of query embeddings, finds the documents nearest to them in the collection.
        /// The result is a list of documents, one for each embedding in the query.
        /// </summary>
        /// <param name="queryEmbeddings">A list of query embeddings to find the nearest documents for.</param>
        /// <param name="include">Specifies which related data to include in the query.</param>
        /// <param name="ids">If indicated, restrict the query to the list of ids.</param>
        /// <param name="nResults">The number of nearest results to return for each query embedding.</param>
        /// <param name="where">If indicated, restrict the query to the specified conditions in metadatas.</param>
        /// <param name="whereDocument">If indicated, restrict the query to the specified conditions in documents.</param>
        /// <param name="limit">If indicated, limit the number of results returned.</param>
        /// <param name="offset">If indicated, specify the number of results to skip.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CollectionQueryAsync(IList<IList<float>> queryEmbeddings,
            IList<Include>? include,
            IList<string>? ids,
            int? nResults,
            WhereFilter? where,
            WhereDocumentFilter? whereDocument,
            int? limit,
            int? offset)
        {
            // Get our collection
            Collection collection = await _client.Collection.CreateCollectionAsync(tenant: CollectionTenant,
                  database: CollectionDatabase,
                  request: new CreateCollectionPayload
                  {
                      Name = _collection.Name,
                      GetOrCreate = true,
                      Metadata = null,
                      Configuration = null
                  });

            QueryRequestPayloadVariant2 queryRequestPayloadVariant2 = new QueryRequestPayloadVariant2
            {
                QueryEmbeddings = queryEmbeddings,
                NResults = nResults,
                Include = include,
                Ids = ids
            };

            RawWhereFields? rawWhereFields = null;

            if (where is not null || whereDocument is not null)
            {
                // Handle the where and whereDocument conditions here
                rawWhereFields = new RawWhereFields
                {
                    // It is mandatory to convert the WhereFilter to a JsonElement for the RawWhereFields
                    Where = where?.ToJsonElement(),
                    // It is mandatory to convert the WhereDocumentFilter to a JsonElement for the RawWhereFields
                    WhereDocument = whereDocument?.ToJsonElement()
                };
            }

            QueryRequestPayload queryRequestPayload = new QueryRequestPayload
            {
                QueryRequestPayloadVariant2 = queryRequestPayloadVariant2,
                RawWhereFields = rawWhereFields
            };

            QueryResponse queryResponse = await _client.Record.CollectionQueryAsync(tenant: CollectionTenant,
                database: CollectionDatabase,
                collectionId: _collection.Id.ToString(),
                request: queryRequestPayload,
                limit: limit,
                offset: offset
                );

            if (queryResponse != null)
            {
                if (queryResponse.Ids != null && queryResponse.Ids.Count > 0)
                {
                    IList<string>? _ids = queryResponse.Ids[0];
                }

                if (queryResponse.Documents != null && queryResponse.Documents.Count > 0)
                {
                    IList<string?>? documents = queryResponse.Documents[0];
                }

                if (queryResponse.Distances != null && queryResponse.Distances.Count > 0)
                {
                    IList<float?>? distances = queryResponse.Distances[0];
                }

                if (queryResponse.Metadatas != null && queryResponse.Metadatas.Count > 0)
                {
                    IList<Chroma.HashMap?>? metadatas = queryResponse.Metadatas[0];
                }

                if (queryResponse.Uris != null && queryResponse.Uris.Count > 0)
                {
                    IList<string?>? uris = queryResponse.Uris[0];
                }

            }

        }


        /// <summary>
        /// Adds records with embeddings and optional metadata to a Chroma collection, 
        /// creating the collection if it doesn't exist.
        /// </summary>
        /// <param name="ids">List of unique identifiers for the records.</param>
        /// <param name="embeddings">List of embedding vectors for each record.</param>
        /// <param name="documents">Optional list of document contents.</param>
        /// <param name="uris">Optional list of URIs associated with the records.</param>
        /// <param name="metadatas">Optional list of metadata dictionaries for each record.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CollectionAddAsync(IList<string> ids,
            IList<IList<float>> embeddings,
            IList<string?>? documents,
            IList<string?>? uris,
            IList<IDictionary<string, object>>? metadatas)
        {
            var embeddingsPayload = new EmbeddingsPayload
            {
                EmbeddingsPayloadVariant1 = embeddings,
                EmbeddingsPayloadVariant2 = null
            };

            IList<Chroma.HashMap?>? metas = null;

            if (metadatas is not null)
            {
                metas = new List<Chroma.HashMap?>();

                foreach (var metadata in metadatas)
                {
                    HashMap hashMap = new HashMap
                    {
                        AdditionalProperties = metadata
                    };

                    metas.Add(hashMap);
                }
            }

            AddCollectionRecordsPayload addCollectionRecordsPayload = new AddCollectionRecordsPayload
            {
                // required fields
                Ids = ids,
                Embeddings = embeddingsPayload,
                // optional fields
                Documents = documents,
                Metadatas = metas,
                Uris = uris
            };

            // Get the collection where we want to add records
            Collection collection = await _client.Collection.CreateCollectionAsync(tenant: CollectionTenant,
                  database: CollectionDatabase,
                  request: new CreateCollectionPayload
                  {
                      Name = _collection.Name,
                      GetOrCreate = true,
                      Metadata = null,
                      Configuration = null
                  });

            // Add records in the collection
            var response = await _client.Record.CollectionAddAsync(tenant: CollectionTenant,
                database: CollectionDatabase,
                collectionId: _collection.Id.ToString(),
                request: addCollectionRecordsPayload);
        }


        /// <summary>
        /// Upserts records with embeddings and optional metadata to a Chroma collection, 
        /// creating the collection if it doesn't
        /// </summary>
        /// <param name="ids">List of unique identifiers for the records.</param>
        /// <param name="embeddings">List of embedding vectors for each record.</param>
        /// <param name="documents">Optional list of document contents.</param>
        /// <param name="uris">Optional list of URIs associated with the records.</param>
        /// <param name="metadatas">Optional list of metadata dictionaries for each record.</param>
        /// <returns></returns>
        public async Task CollectionUpsertAsync(IList<string> ids,
            IList<IList<float>> embeddings,
            IList<string?>? documents,
            IList<string?>? uris,
            IList<IDictionary<string, object>>? metadatas)
        {
            var embeddingsPayload = new EmbeddingsPayload
            {
                EmbeddingsPayloadVariant1 = embeddings,
                EmbeddingsPayloadVariant2 = null
            };

            IList<Chroma.HashMap?>? metas = null;

            if (metadatas is not null)
            {
                metas = new List<Chroma.HashMap?>();

                foreach (var metadata in metadatas)
                {
                    HashMap hashMap = new HashMap
                    {
                        AdditionalProperties = metadata
                    };

                    metas.Add(hashMap);
                }
            }

            var upsertPayload = new UpsertCollectionRecordsPayload
            {
                // required fields
                Ids = ids,
                Embeddings = embeddingsPayload,
                // optional fields
                Documents = documents,
                Metadatas = metas,
                Uris = uris
            };

            // Get our collection
            Collection collection = await _client.Collection.CreateCollectionAsync(tenant: CollectionTenant,
                  database: CollectionDatabase,
                  request: new CreateCollectionPayload
                  {
                      Name = _collection.Name,
                      GetOrCreate = true,
                      Metadata = null,
                      Configuration = null
                  });

            // Upsert records in the collection
            await _client.Record.CollectionUpsertAsync(tenant: CollectionTenant,
                database: CollectionDatabase,
                collectionId: _collection.Id.ToString(),
                request: upsertPayload);
        }

        /// <summary>
        /// Updates records with embeddings and optional metadata in a Chroma collection,
        /// creating the collection if it doesn't exist. 
        /// This method is used to modify existing records in the collection.
        /// </summary>
        /// <param name="ids">List of unique identifiers for the records.</param>
        /// <param name="embeddings">List of embedding vectors for each record.</param>
        /// <param name="documents">Optional list of document contents.</param>
        /// <param name="uris">Optional list of URIs associated with the records.</param>
        /// <param name="metadatas">Optional list of metadata dictionaries for each record.</param>
        /// <returns></returns>
        public async Task CollectionUpdateAsync(IList<string> ids,
            IList<IList<float>?>? embeddings,
            IList<string?>? documents,
            IList<string?>? uris,
            IList<IDictionary<string, object>>? metadatas)
        {
            var embeddingsPayload = new UpdateEmbeddingsPayload
            {
                UpdateEmbeddingsPayloadVariant1 = embeddings,
                UpdateEmbeddingsPayloadVariant2 = null
            };

            IList<Chroma.HashMap?>? metas = null;

            if (metadatas is not null)
            {
                metas = new List<Chroma.HashMap?>();

                foreach (var metadata in metadatas)
                {
                    HashMap hashMap = new HashMap
                    {
                        AdditionalProperties = metadata
                    };

                    metas.Add(hashMap);
                }
            }

            var updatePayload = new UpdateCollectionRecordsPayload
            {
                // required fields
                Ids = ids,
                Embeddings = embeddingsPayload,
                // optional fields
                Documents = documents,
                Metadatas = metas,
                Uris = uris
            };

            // Get our collection
            Collection collection = await _client.Collection.CreateCollectionAsync(tenant: CollectionTenant,
                  database: CollectionDatabase,
                  request: new CreateCollectionPayload
                  {
                      Name = _collection.Name,
                      GetOrCreate = true,
                      Metadata = null,
                      Configuration = null
                  });

            // Update records in the collection
            await _client.Record.CollectionUpdateAsync(tenant: CollectionTenant,
               database: CollectionDatabase,
               collectionId: _collection.Id.ToString(),
               request: updatePayload);
        }

        /// <summary>
        /// Delete items from a collection by id.
        /// </summary>
        /// <param name="ids">List of ids to delete.</param>
        /// <param name="limit">Optional limit on the number of items to delete.</param>
        /// <returns></returns>
        public async Task CollectionDeleteAsync(IList<string> ids,
            int? limit)
        {

            DeleteCollectionRecordsPayloadVariant2 payloadVariant2 = new DeleteCollectionRecordsPayloadVariant2
            {
                Ids = ids,
                Limit = limit
            };

            await _client.Record.CollectionDeleteAsync(tenant: CollectionTenant,
                database: CollectionDatabase,
                collectionId: _collection.Id.ToString(),
                request: new DeleteCollectionRecordsPayload
                {
                    DeleteCollectionRecordsPayloadVariant2 = payloadVariant2,
                    RawWhereFields = null
                });
        }

        /// <summary>
        /// Delete all items in the collection that match the where filter.
        /// </summary>
        /// <param name="whereFilter">Filter to match items for deletion.</param>
        /// <returns></returns>
        public async Task CollectionDeleteAsync(WhereFilter whereFilter)
        {
            RawWhereFields rawWhereFields = new RawWhereFields
            {
                Where = whereFilter,
                WhereDocument = null
            };

            await _client.Record.CollectionDeleteAsync(tenant: CollectionTenant,
                database: CollectionDatabase,
                collectionId: _collection.Id.ToString(),
                request: new DeleteCollectionRecordsPayload
                {
                    DeleteCollectionRecordsPayloadVariant2 = null,
                    RawWhereFields = rawWhereFields
                });
        }

    }
}
