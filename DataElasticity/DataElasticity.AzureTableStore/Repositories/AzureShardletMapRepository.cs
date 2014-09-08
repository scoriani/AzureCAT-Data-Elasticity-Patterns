using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.CacheModels;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Shards;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Repositories
{
    internal class AzureShardletMapRepository : AzureTableRepositoryBase
    {
        private readonly string _shardSetName;

        #region constants

        private const string _connectionSuffix = "shardletmap";
        private const string _cacheType = "ShardletMap";

        #endregion

        #region fields

        private readonly CloudTable _table;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureShardletMapRepository"/> class.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        public AzureShardletMapRepository(string shardSetName)
        {
            _shardSetName = shardSetName;
            var tableName = GetTableName(shardSetName);

            _table = TableClient.GetTableReference(tableName);
        }

        #endregion

        #region methods

        /// <summary>
        /// Deletes the specified azure shardlet.
        /// </summary>
        /// <param name="azureShardlet">The azure shardlet.</param>
        public void Delete(AzureShardlet azureShardlet)
        {
            _table.Execute(TableOperation.Delete(azureShardlet));

            AzureCache.Remove(GetCacheKey(_cacheType, azureShardlet));
        }

        /// <summary>
        /// Gets a list of all shardlets for the shard set.
        /// </summary>
        /// <returns>IEnumerable&lt;AzureShardlet&gt;.</returns>
        public IEnumerable<AzureShardlet> Get()
        {
            var query = new TableQuery<AzureShardlet>();

            IEnumerable<AzureShardlet> result = null;

            RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy()
                .ExecuteAction(() => result = _table.ExecuteQuery(query));

            return result;
        }

        /// <summary>
        /// Gets the shardlet from the specified shard set name and distribution key.
        /// </summary>
        /// <param name="distributionKey">The distribution key.</param>
        /// <returns>AzureShardlet.</returns>
        public AzureShardlet Get(long distributionKey)
        {
            var cachedShardlet = AzureCache.Get<CacheShardlet>(GetCacheKey(_cacheType, _shardSetName, LongBasedRowKeyEntity.MakeRowKeyFromLong(distributionKey)));

            if (cachedShardlet != null)
                return cachedShardlet.ToAzureShardlet();

            var rowKey = LongBasedRowKeyEntity.MakeRowKeyFromLong(distributionKey);

            var condition1 = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _shardSetName);
            var condition2 = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey);
            var condition = TableQuery.CombineFilters(condition1, TableOperators.And, condition2);

            var query =
                new TableQuery<AzureShardlet>()
                    .Where(condition);

            IEnumerable<AzureShardlet> result = null;

            RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy()
                .ExecuteAction(() => result = _table.ExecuteQuery(query));

            var azureShardlet = result.FirstOrDefault();

            Cache(azureShardlet);

            return azureShardlet;

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
        /// <param name="azureShardlet">The azure shardlet.</param>
        public void Insert(AzureShardlet azureShardlet)
        {
            RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy()
                .ExecuteAction(() => _table.Execute(TableOperation.Insert(azureShardlet)));

            Cache(azureShardlet);
        }

        /// <summary>
        /// Merges the specified azure shardlet.
        /// </summary>
        /// <param name="azureShardlet">The azure shardlet.</param>
        public void Merge(AzureShardlet azureShardlet)
        {
            RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy()
                .ExecuteAction(() => _table.Execute(TableOperation.Merge(azureShardlet)));

            Cache(azureShardlet);
        }

        private void Cache(AzureShardlet azureShardlet)
        {
            if (azureShardlet == null) return;

            AzureCache.Put(GetCacheKey(_cacheType, azureShardlet), azureShardlet.ToCacheShardlet());
        }

        private static string GetTableName(string shardSetName)
        {
            // the table name is a combination of the the shard set name and a suffix
            return String.Format("{0}{1}", shardSetName.ToLower(), _connectionSuffix);
        }

        #endregion
    }
}