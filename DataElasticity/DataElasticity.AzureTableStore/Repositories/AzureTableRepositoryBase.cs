using System;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Repositories
{
    /// <summary>
    /// Class AzureTableRepositoryBase is a common base class for simple repositories interacting with Azure tables.
    /// </summary>
    internal abstract class AzureTableRepositoryBase
    {
        #region constants

        /// <summary>
        /// The batch size for batch operations.
        /// Azure performs operations on batches up to 100
        /// </summary>
        protected const int BatchSize = 100;

        #endregion

        #region fields

        /// <summary>
        /// The table client connection.
        /// </summary>
        internal readonly CloudTableClient TableClient;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableRepositoryBase"/> class.
        /// </summary>
        protected AzureTableRepositoryBase()
        {
            var connectionString = GetConnectionString();
            var storageAccount = CloudStorageAccount.Parse(connectionString.ConnectionString);

            TableClient = storageAccount.CreateCloudTableClient();
        }

        #endregion

        #region methods

        /// <summary>
        /// Gets the cache key.
        /// </summary>
        /// <param name="cacheType">Type of the cache.</param>
        /// <param name="tableEntity">The table entity.</param>
        /// <returns>System.String.</returns>
        protected string GetCacheKey(string cacheType, ITableEntity tableEntity)
        {
            // the cache key is a combination of the the shard set name and distribution key
            return GetCacheKey(cacheType, tableEntity.PartitionKey, tableEntity.RowKey);
        }

        /// <summary>
        /// Gets the cache key.
        /// </summary>
        /// <param name="cacheType">Type of the cache.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        /// <returns>System.String.</returns>
        protected string GetCacheKey(string cacheType, string partitionKey, string rowKey)
        {
            // the cache key is a combination of the the shard set name and distribution key
            return string.Format("{0}|{1}|{2}", cacheType, partitionKey, rowKey);
        }

        /// <summary>
        /// Gets the connection string to Azure storage from configuration.
        /// </summary>
        /// <returns>ConnectionStringSettings.</returns>
        /// <exception cref="System.InvalidOperationException">Connection string to azure storage is required in your app.config</exception>
        private ConnectionStringSettings GetConnectionString()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["AzureStorage"];

            if (connectionString == null)
            {
                throw new InvalidOperationException("Connection string to azure storage is required in your app.config");
            }

            return connectionString;
        }

        #endregion
    }
}