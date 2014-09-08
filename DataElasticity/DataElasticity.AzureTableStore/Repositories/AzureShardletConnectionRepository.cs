using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.CacheModels;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Shards;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Repositories
{
    /// <summary>
    /// Class AzureShardletConnectionRepository is the repository class for the AzureShardletConnection
    /// model.
    /// </summary>
    internal class AzureShardletConnectionRepository : AzureTableRepositoryBase
    {
        #region constants

        private const string _cacheType = "ShardletConnection";
        private const string _connectionSuffix = "shardletconnection";

        #endregion

        #region fields

        private readonly CloudTable _table;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureShardletConnectionRepository"/> class.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        public AzureShardletConnectionRepository(string shardSetName)
        {
            var tableName = GetTableName(shardSetName);

            _table = TableClient.GetTableReference(tableName);
        }

        #endregion

        #region methods

        /// <summary>
        /// Deletes the specified azure shardlet connection.
        /// </summary>
        /// <param name="azureShardletConnection">The azure shardlet connection.</param>
        public void Delete(AzureShardletConnection azureShardletConnection)
        {
            RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy()
                .ExecuteAction(() => _table.Execute(TableOperation.Delete(azureShardletConnection)));

            AzureCache.Remove(GetCacheKey(_cacheType, azureShardletConnection));
        }

        /// <summary>
        /// Deletes the shardlet connections older than the specified UTC Date and Time.
        /// </summary>
        /// <param name="utcDateTime">The UTC date time.</param>
        public void Delete(DateTime utcDateTime)
        {
            var condition = TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThanOrEqual,
                utcDateTime);

            var query =
                new TableQuery()
                    .Where(condition)
                    .Select(new[] {"PartitionKey", "RowKey"});

            var result = _table.ExecuteQuery(query).ToArray();

            var array = result.ToArray();
            var batches = Enumerable.Range(0, array.Count()).GroupBy(i => i/BatchSize, i => array[i]);

            foreach (var batch in batches)
            {
                var batchOperation = new TableBatchOperation();
                foreach (var connection in batch)
                {
                    var entity =
                        new DynamicTableEntity
                        {
                            RowKey = connection.RowKey,
                            PartitionKey = connection.PartitionKey,
                            ETag = "*"
                        };

                    batchOperation.Delete(entity);
                }

                RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy()
                    .ExecuteAction(() => _table.ExecuteBatch(batchOperation));

                foreach (var connection in batch)
                {
                    AzureCache.Remove(GetCacheKey(_cacheType, connection.PartitionKey, connection.RowKey));
                }
            }
        }

        /// <summary>
        /// Deletes the rows associated with the collection of SPIDs for the specified shardlet.
        /// </summary>
        /// <param name="shardlet">The shardlet.</param>
        /// <param name="spids">The spids.</param>
        public void Delete(Shardlet shardlet, IEnumerable<short> spids)
        {
            var array = spids.ToArray();
            var batches = Enumerable.Range(0, array.Count()).GroupBy(i => i/BatchSize, i => array[i]);

            var partitionKey = shardlet.DistributionKey.ToString(CultureInfo.InvariantCulture);

            foreach (var batch in batches)
            {
                var batchOperation = new TableBatchOperation();
                foreach (var spid in batch)
                {
                    var rowKey = AzureShardletConnection.GetRowKey(shardlet.Catalog, spid);
                    var entity =
                        new DynamicTableEntity
                        {
                            RowKey = rowKey,
                            PartitionKey = partitionKey,
                            ETag = "*"
                        };

                    batchOperation.Delete(entity);
                }

                RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy()
                    .ExecuteAction(() => _table.ExecuteBatch(batchOperation));

                foreach (var spid in batch)
                {
                    AzureCache.Remove(GetCacheKey(_cacheType,
                        partitionKey,
                        AzureShardletConnection.GetRowKey(shardlet.Catalog, spid)));
                }
            }
        }

        /// <summary>
        /// Gets the specified shardlet mapped to a specific spid.
        /// </summary>
        /// <param name="shardlet">The shardlet.</param>
        /// <param name="spid">The spid.</param>
        /// <returns>AzureShardletConnection.</returns>
        public AzureShardletConnection Get(Shardlet shardlet, int spid)
        {
            var partitionKey = shardlet.DistributionKey.ToString(CultureInfo.InvariantCulture);
            var rowKey = AzureShardletConnection.GetRowKey(shardlet.Catalog, spid);

            var cacheShardletConnection =
                AzureCache.Get<CacheShardletConnection>(GetCacheKey(_cacheType, partitionKey, rowKey));

            if (cacheShardletConnection != null)
                return cacheShardletConnection.ToAzureShardletConnection();

            var condition1 = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            var condition2 = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey);
            var condition = TableQuery.CombineFilters(condition1, TableOperators.And, condition2);

            var query =
                new TableQuery<AzureShardletConnection>()
                    .Where(condition);

            IEnumerable<AzureShardletConnection> result = null;

            RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy()
                .ExecuteAction(() => result = _table.ExecuteQuery(query));

            var azureShardletConnection = result.FirstOrDefault();

            Cache(azureShardletConnection);

            return azureShardletConnection;
        }

        /// <summary>
        /// Gets the SQL Server SPIDs currently connected to a specific shardlet.
        /// </summary>
        /// <param name="shardlet">The shardlet.</param>
        /// <returns>IEnumerable&lt;System.Int32&gt;.</returns>
        public IEnumerable<short> Get(Shardlet shardlet)
        {
            // todo - dynamic parameter
            var condition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                shardlet.DistributionKey.ToString(CultureInfo.InvariantCulture));

            var query =
                new TableQuery<AzureShardletConnection>()
                    .Where(condition);

            IEnumerable<AzureShardletConnection> result = null;

            RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy()
                .ExecuteAction(() => result = _table.ExecuteQuery(query));

            return
                result
                    .Select(asc => (short) asc.Spid)
                    .ToArray();
        }

        /// <summary>
        /// Initializes the azure table.
        /// </summary>
        /// <param name="drop">if set to <c>true</c> [drop].</param>
        public void InitializeAzureTable(bool drop = false)
        {
            if (drop)
            {
                _table.DeleteIfExists();
            }

            _table.CreateIfNotExists();
        }

        /// <summary>
        /// Inserts the specified azure shardlet connection.
        /// </summary>
        /// <param name="azureShardletConnection">The azure shardlet connection.</param>
        public void Insert(AzureShardletConnection azureShardletConnection)
        {
            _table.Execute(TableOperation.Insert(azureShardletConnection));

            Cache(azureShardletConnection);
        }

        /// <summary>
        /// Merges the specified azure shardlet connection.
        /// </summary>
        /// <param name="azureShardletConnection">The azure shardlet connection.</param>
        public void Merge(AzureShardletConnection azureShardletConnection)
        {
            _table.Execute(TableOperation.Merge(azureShardletConnection));

            Cache(azureShardletConnection);
        }

        private void Cache(AzureShardletConnection azureShardletConnection)
        {
            if (azureShardletConnection == null) return;

            AzureCache.Put(GetCacheKey(_cacheType, azureShardletConnection),
                azureShardletConnection.ToCacheShardletConnection());
        }

        private static string GetTableName(string shardSetName)
        {
            // the table name is a combination of the the shard set name and a suffix
            return String.Format("{0}{1}", shardSetName.ToLower(), _connectionSuffix);
        }

        #endregion
    }
}