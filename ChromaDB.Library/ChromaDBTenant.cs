using Chroma;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChromaDB.Library
{
    public record ChromaDBTenant(string TenantName, ChromaClient ChromaClient)
    {

        /// <summary>
        /// Creates a new database in the tenant.
        /// </summary>
        /// <param name="databaseName">The name of the database to create.</param>
        /// <returns>The created database if successful; otherwise, null.</returns>
        public async Task<ChromaDBDatabase?> CreateDatabaseAsync(string databaseName)
        {
            ChromaDBDatabase? chromaDBDatabase = null;

            try
            {
                var createDatabaseResponse = await ChromaClient.Database.CreateDatabaseAsync(TenantName, databaseName);

                chromaDBDatabase = new ChromaDBDatabase(null, databaseName, TenantName, ChromaClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating a database: {ex.Message}");
            }

            return chromaDBDatabase;
        }

        /// <summary>
        /// Delete a database in the tenant.
        /// </summary>
        /// <param name="databaseName">The name of the database to delete.</param>
        /// <returns></returns>
        public async Task DeleteDatabaseAsync(string databaseName)
        {
            try
            {
                var deleteDatabaseResponse = await ChromaClient.Database.DeleteDatabaseAsync(TenantName, databaseName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting a database: {ex.Message}");
            }
        }

        /// <summary>
        /// List the databases in the tenant.
        /// </summary>
        /// <returns>A list of databases in the tenant.</returns>
        public async Task<List<ChromaDBDatabase>> ListDatabasesAsync()
        {
            List<ChromaDBDatabase> result = new List<ChromaDBDatabase>();

            try
            {
                var databases = await ChromaClient.Database.ListDatabasesAsync(TenantName);
                foreach (var database in databases)
                {
                    result.Add(new ChromaDBDatabase(database.Id, database.Name, database.Tenant, ChromaClient));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing databases: {ex.Message}");
            }

            return result;
        }


    }
}
