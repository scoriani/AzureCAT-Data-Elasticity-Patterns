#region usings

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Shards;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Repositories;
using Microsoft.AzureCat.Patterns.DataElasticity.Interfaces;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore
{
    /// <summary>
    /// Class AzureShardSetConnectionDriver implements IShardSetConnectionDriver using Azure table storage 
    /// as the primary tracking backing store for managing connections.
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public class AzureShardSetConnectionDriver : BaseShardSetConnectionDriver, IShardSetConnectionDriver
    {
        #region IShardSetConnectionDriver

        /// <summary>
        /// Gets the Shardlet.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <param name="dataSetName">Name of the data set.</param>
        /// <param name="uniqueValue">The unique value.</param>
        /// <returns>Shardlet.</returns>
        public override Shardlet GetShardlet(string shardSetName, string dataSetName, string uniqueValue)
        {
            /**
             * There is a scenario where the call to get the shard only has a globally unique value that has no sense 
             * of an associated DistributionKey.  Unlike the method GetDistributionKeyForShardingKey(string shardingKey), 
             *  where the shardingKey does produce a deterministic DistributionKey value.  The DistributionKey is retrieved from a lookup.
             * */

            var globallyUniqueValue = new AzureGloballyUniqueValue();
            var distributionKey = globallyUniqueValue.GetGloballyUniqueValue(dataSetName, uniqueValue).DistributionKey;

            return GetShardletByDistributionKey(shardSetName, distributionKey);
        }

        /// <summary>
        /// Gets the Shardlet by distribution key from either the pinned Shardlet list or by range lookup.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <param name="distributionKey">The distribution key.</param>
        /// <param name="isNew">if set to <c>true</c> the key is a new key and implementer should not look for existing ShardLet.</param>
        /// <returns>Shardlet.</returns>
        public override Shardlet GetShardletByDistributionKey(string shardSetName, long distributionKey,
            bool isNew = false)
        {
            var repository = new AzureShardletMapRepository(shardSetName);

            if (!isNew)
            {
                var azureShardlet = GetAzureShardlet(shardSetName, distributionKey);

                if (azureShardlet != null)
                    return azureShardlet.ToFrameworkShardlet();
            }

            var shardlet = new Shardlet();

            //todo: how to set the sharding key?
            var rangeShard = repository.Get(distributionKey);

            if (rangeShard == null) return shardlet;

            shardlet.ServerInstanceName = rangeShard.ServerInstanceName;
            shardlet.Catalog = rangeShard.Catalog;
            shardlet.ShardSetName = shardSetName;
            shardlet.DistributionKey = distributionKey;

            //TODO: Determine if the published shard should have a status and derive it from that for full shard transitions.
            shardlet.Status = ShardletStatus.Active;

            return shardlet;
        }

        /// <summary>
        /// Gets the shardlet by sharding key.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <param name="shardingKey">The sharding key.</param>
        /// <param name="distributionKey">The distribution key.</param>
        /// <returns>Shardlet.</returns>
        public override Shardlet GetShardletByShardingKey(string shardSetName, string shardingKey, long distributionKey)
        {
            Shardlet shardlet;

            var repository = new AzureShardletMapRepository(shardSetName);
            var azureShardlet = repository.Get(distributionKey);

            if (azureShardlet == null)
            {
                var rangeShard = GetAzureRangeShard(shardSetName, distributionKey);

                if (rangeShard == null)
                {
                    // would only arrive here if the shardmap is not yet
                    // published or is published incorrectly

                    return null;
                }

                // add shardlet
                shardlet =
                    new Shardlet
                    {
                        ServerInstanceName = rangeShard.ServerInstanceName,
                        Catalog = rangeShard.Catalog,
                        ShardSetName = shardSetName,
                        DistributionKey = distributionKey,
                        ShardingKey = shardingKey,
                        Status = ShardletStatus.Active,
                    };

                SaveAzureShardlet(shardlet);
            }
            else
            {
                shardlet = azureShardlet.ToFrameworkShardlet();
            }

            return shardlet;
        }

        /// <summary>
        /// Gets the shardlets in the shard set.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <returns>IEnumerable&lt;Shardlet&gt;.</returns>
        public override IEnumerable<Shardlet> GetShardlets(string shardSetName)
        {
            var repository = new AzureShardletMapRepository(shardSetName);

            return repository.Get().Select(s => s.ToFrameworkShardlet()).AsEnumerable();
        }

        /// <summary>
        /// Publishes the shard into the live shard map.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <param name="rangeShard">The shard.</param>
        public override void PublishShard(string shardSetName, RangeShard rangeShard)
        {
            SaveAzureRangeShard(shardSetName, rangeShard.HighDistributionKey, rangeShard.ServerInstanceName,
                rangeShard.Catalog);
        }

        /// <summary>
        /// Publishes the Shardlet into the pinned Shardlet list.
        /// </summary>
        /// <param name="shardlet">The Shardlet.</param>
        public override void PublishShardlet(Shardlet shardlet)
        {
            SaveAzureShardlet(shardlet);
        }

        /// <summary>
        /// Registers the shardlet connection.
        /// </summary>
        /// <param name="shardlet">The shardlet.</param>
        /// <param name="spid">The spid.</param>
        public override void PublishShardletConnection(Shardlet shardlet, short spid)
        {
            var repository = new AzureShardletConnectionRepository(shardlet.ShardSetName);

            var azureShardletConnection = repository.Get(shardlet, spid);

            if (azureShardletConnection != null)
            {
                // updates the timestamp
                repository.Merge(azureShardletConnection);

                return;
            }

            azureShardletConnection =
                new AzureShardletConnection
                {
                    Catalog = shardlet.Catalog,
                    DistributionKey = shardlet.DistributionKey,
                    ShardingKey = shardlet.ShardingKey,
                    Spid = spid
                };

            repository.Insert(azureShardletConnection);
        }

        /// <summary>
        /// Removes the shard from the live shard map.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <param name="rangeShard">The shard.</param>
        public override void RemoveShard(string shardSetName, RangeShard rangeShard)
        {
            DeleteAzureRangeShard(shardSetName, rangeShard.HighDistributionKey);
        }

        /// <summary>
        /// De-registers the shardlet connection.
        /// </summary>
        /// <param name="shardlet">The shardlet.</param>
        /// <param name="spid">The spid.</param>
        /// <returns>Microsoft.AzureCat.Patterns.DataElasticity.Models.ShardletStatus.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void RemoveShardletConnection(Shardlet shardlet, short spid)
        {
            // todo: do we have to look it up?  or can we just fail to delete?
            var repository = new AzureShardletConnectionRepository(shardlet.ShardSetName);

            var azureShardletConnection = repository.Get(shardlet, spid);
            if (azureShardletConnection == null)
                return;

            repository.Delete(azureShardletConnection);
        }

        /// <summary>
        /// Terminates the connections to the shardlet.
        /// </summary>
        /// <param name="shardlet">The shardlet.</param>
        public override void TerminateConnections(Shardlet shardlet)
        {
            var repository = new AzureShardletConnectionRepository(shardlet.ShardSetName);

            // get all connections to the shardlet
            var spids = repository.Get(shardlet);

            // terminate all connections....
            TerminateDatabaseConnections(shardlet, spids);

            // delete all connections from the shardlet repository
            repository.Delete(shardlet, spids);
        }

        #endregion

        #region methods

        /// <summary>
        /// Gets the Azure Range Shard for the shard set and distribution key
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <param name="distributionKey">The distribution key.</param>
        /// <returns>AzureRangeShard.</returns>
        public AzureRangeShard GetAzureRangeShard(string shardSetName, long distributionKey)
        {
            var rowKey = LongBasedRowKeyEntity.MakeRowKeyFromLong(distributionKey);
            var repository = new AzureRangeShardRepository();

            return repository.Get(shardSetName, rowKey);
        }

        /// <summary>
        /// Gets the connected SQL Server SPIDs for the shardlet.
        /// </summary>
        /// <param name="shardlet">The shardlet.</param>
        /// <returns>IEnumerable&lt;System.Int32&gt;.</returns>
        public IEnumerable<short> GetConnectedSpids(Shardlet shardlet)
        {
            var repository = new AzureShardletConnectionRepository(shardlet.ShardSetName);

            return repository.Get(shardlet);
        }

        /// <summary>
        /// Initializes the azure tables.  Need to run this one time.  Will check if tables already
        /// exist and leave them alone if they do.
        /// </summary>
        /// <param name="drop">if set to <c>true</c> drop any existing tables first.</param>
        public static void InitializeAzureTables(bool drop = false)
        {
            var connectionString = GetConnectionString();

            var storageAccount = CloudStorageAccount.Parse(connectionString.ConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();

            var rangeShardTable = tableClient.GetTableReference(AzureRangeShard.RangeShardTable);

            if (drop)
            {
                rangeShardTable.DeleteIfExists();
            }

            rangeShardTable.CreateIfNotExists();
        }

        /// <summary>
        /// Initializes the azure tables for a shard set.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <param name="drop">if set to <c>true</c> [drop].</param>
        public static void InitializeAzureTables(string shardSetName, bool drop = false)
        {
            var azureShardletConnectionRepository = new AzureShardletConnectionRepository(shardSetName);
            azureShardletConnectionRepository.InitializeAzureTable(drop);

            var azureShardletMapRepository = new AzureShardletMapRepository(shardSetName);
            azureShardletMapRepository.InitializeAzureTable(drop);

            var azureRangeShardRepository = new AzureRangeShardRepository();
            azureRangeShardRepository.InitializeAzureTable(drop);
        }

        /// <summary>
        /// Removes the connections older than the utcDateTime.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set to clear connections for.</param>
        /// <param name="utcDateTime">The UTC date time.</param>
        public void RemoveConnections(string shardSetName, DateTime utcDateTime)
        {
            // delete all connections from the shardlet repository
            var repository = new AzureShardletConnectionRepository(shardSetName);
            repository.Delete(utcDateTime);
        }

        /// <summary>
        /// Removes the Shardlet from the pinned Shardlet list.
        /// </summary>
        /// <param name="shardlet">The Shardlet.</param>
        public void RemoveShardlet(Shardlet shardlet)
        {
            DeleteAzureShardlet(shardlet.ShardSetName, shardlet);
        }

        private void DeleteAzureRangeShard(string shardSetName, long distributionKey)
        {
            var repository = new AzureRangeShardRepository();
            var azureRangeShard = GetAzureRangeShard(shardSetName, distributionKey);
            if (azureRangeShard == null)
                return;

            repository.Delete(azureRangeShard);
        }

        private void DeleteAzureShardlet(string shardSetName, Shardlet shardlet)
        {
            var repository = new AzureShardletMapRepository(shardSetName);
            var azureShardlet = repository.Get(shardlet.DistributionKey);

            if (azureShardlet != null)
            {
                repository.Delete(azureShardlet);
            }
        }

        private AzureShardlet GetAzureShardlet(string shardSetName, long distributionKey)
        {
            var repository = new AzureShardletMapRepository(shardSetName);
            return repository.Get(distributionKey);
        }

        private static ConnectionStringSettings GetConnectionString()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["AzureStorage"];

            if (connectionString == null)
            {
                throw new InvalidOperationException("Connection string to azure storage is required in your app.config");
            }

            return connectionString;
        }

        private void SaveAzureRangeShard(string shardSetName, long maxDistributionKey, string serverInstanceName,
            string catalog)
        {
            var repository = new AzureRangeShardRepository();
            var azureRangeShard = GetAzureRangeShard(shardSetName, maxDistributionKey);

            if (azureRangeShard == null)
            {
                azureRangeShard =
                    new AzureRangeShard
                    {
                        Catalog = catalog,
                        MaxRange = maxDistributionKey,
                        ServerInstanceName = serverInstanceName,
                        ShardSetName = shardSetName
                    };

                repository.Insert(azureRangeShard);
            }
            else
            {
                azureRangeShard.Catalog = catalog;
                azureRangeShard.ServerInstanceName = serverInstanceName;

                repository.Merge(azureRangeShard);
            }
        }

        /// <summary>
        /// Saves the azure Shardlet.
        /// </summary>
        /// <param name="shardlet">The shardlet.</param>
        private void SaveAzureShardlet(Shardlet shardlet)
        {
            var shardSetName = shardlet.ShardSetName;
            var distributionKey = shardlet.DistributionKey;
            var shardingKey = shardlet.ShardingKey;
            var status = shardlet.Status;
            var serverInstanceName = shardlet.ServerInstanceName;
            var catalog = shardlet.Catalog;
            var pinned = shardlet.Pinned;

            var repository = new AzureShardletMapRepository(shardSetName);
            var azureShardlet = repository.Get(distributionKey);

            if (azureShardlet == null)
            {
                azureShardlet =
                    new AzureShardlet
                    {
                        ShardSetName = shardSetName,
                        DistributionKey = distributionKey,
                        ShardingKey = shardingKey,
                        ServerInstanceName = serverInstanceName,
                        Catalog = catalog,
                        Status = status.ToString(),
                        Pinned = pinned
                    };

                repository.Insert(azureShardlet);
            }
            else
            {
                azureShardlet.Catalog = catalog;
                azureShardlet.ServerInstanceName = serverInstanceName;
                azureShardlet.Status = status.ToString();
                azureShardlet.Pinned = pinned;

                repository.Merge(azureShardlet);
            }
        }

        private static void TerminateDatabaseConnections(Shardlet shardlet, IEnumerable<short> spids)
        {
            // the kill statement cannot take a parameter, so 
            // dynamic sql seems to be required
            const string sql =
                "If EXISTS(SELECT 1 from sys.dm_exec_sessions WHERE session_id = {0} AND login_time <= '{1}')"
                + " BEGIN"
                + "   KILL {0};"
                + " END";

            var connectionString = shardlet.ConnectionString;
            var connection = new ReliableSqlConnection(connectionString);
            var array = spids.ToArray();
            const int batchSize = 25;

            var batches = Enumerable.Range(0, array.Count()).GroupBy(i => i/batchSize, i => array[i]);

            // todo: what should this be
            var loginTime = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);

            // send multiple kill states in batches
            using (connection)
            {
                connection.Open();

                var command = new SqlCommand("", connection.Current);

                foreach (var batch in batches)
                {
                    var builder = new StringBuilder();

                    foreach (var spid in batch)
                    {
                        builder.AppendLine(string.Format(sql, spid.ToString(CultureInfo.InvariantCulture), loginTime));
                    }

                    command.CommandText = builder.ToString();
                    command.ExecuteNonQuery();
                }
            }
        }

        #endregion
    }
}