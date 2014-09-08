using System.Collections.Generic;
using System.Linq;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Shards;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Repositories
{
    internal class AzureRangeShardRepository : AzureTableRepositoryBase
    {
        #region constants

        private const string _connection = "rangeshardtable";

        #endregion

        #region fields

        private readonly CloudTable _table;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureRangeShardRepository"/> class.
        /// </summary>
        public AzureRangeShardRepository()
        {
            var tableName = GetTableName();

            _table = TableClient.GetTableReference(tableName);
        }

        #endregion

        #region methods

        /// <summary>
        /// Deletes the specified azure rangeShard.
        /// </summary>
        /// <param name="azureRangeShard">The azure shardlet.</param>
        public void Delete(AzureRangeShard azureRangeShard)
        {
            RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy()
                .ExecuteAction(() => _table.Execute(TableOperation.Delete(azureRangeShard)));
        }

        /// <summary>
        /// Gets the specified range shard using the shard set name and the row key.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <param name="rowKey">The row key.</param>
        /// <returns>AzureRangeShard.</returns>
        public AzureRangeShard Get(string shardSetName, string rowKey)
        {
            var condition1 = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, shardSetName);
            var condition2 = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, rowKey);
            var condition = TableQuery.CombineFilters(condition1, TableOperators.And, condition2);

            var query =
                new TableQuery<AzureRangeShard>()
                    .Where(condition);

            IEnumerable<AzureRangeShard> result = null;
            RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy()
                .ExecuteAction(() => result = _table.ExecuteQuery(query));

            return result.FirstOrDefault();
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
        /// Inserts the specified azure rangeShard.
        /// </summary>
        /// <param name="azureRangeShard">The azure rangeShard.</param>
        public void Insert(AzureRangeShard azureRangeShard)
        {
            RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy()
                .ExecuteAction(() => _table.Execute(TableOperation.Insert(azureRangeShard)));
        }

        /// <summary>
        /// Merges the specified azure rangeShard.
        /// </summary>
        /// <param name="azureRangeShard">The azure rangeShard.</param>
        public void Merge(AzureRangeShard azureRangeShard)
        {
            RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy()
                .ExecuteAction(() => _table.Execute(TableOperation.Merge(azureRangeShard)));

        }

        /// <summary>
        /// Gets the range shards using shard set name.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <returns>AzureRangeShard[].</returns>
        private AzureRangeShard[] Get(string shardSetName)
        {
            var condition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, shardSetName);

            var query =
                new TableQuery<AzureRangeShard>()
                    .Where(condition);

            IEnumerable<AzureRangeShard> result = null;
            RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy()
                .ExecuteAction(() => result = _table.ExecuteQuery(query));

            return result.ToArray();
        }

        private static string GetTableName()
        {
            return _connection;
        }

        #endregion
    }
}