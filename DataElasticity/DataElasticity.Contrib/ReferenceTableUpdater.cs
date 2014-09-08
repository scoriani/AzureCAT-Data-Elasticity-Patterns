#region usings

using System.Data.SqlClient;
using Microsoft.AzureCat.Patterns.DataElasticity.Interfaces;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Contrib
{
    /// <summary>
    /// Class ReferenceTableUpdater syncs a specified reference table from the main reference database.
    /// </summary>
    public class ReferenceTableUpdater
    {
        #region fields

        private readonly string _referenceDataConnectionString;
        private readonly ShardConnection _shardConnection;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceTableUpdater" /> class.
        /// </summary>
        /// <param name="referenceDataConnectionString">The reference data connection string.</param>
        /// <param name="shard">The shard to update.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="tableGroupName">Name of the shard set.</param>
        public ReferenceTableUpdater(string referenceDataConnectionString, ShardBase shard, Settings settings, string tableGroupName)
        {
            _referenceDataConnectionString = referenceDataConnectionString;
            _shardConnection =
                new ShardConnection
                {
                    ServerInstanceName = shard.ServerInstanceName,
                    Catalog = shard.Catalog,
                    UserName = settings.AdminUser,
                    Password = settings.AdminPassword,
                    ShardSetName = tableGroupName
                };
        }

        #endregion

        #region methods

        /// <summary>
        /// Creates the reference data within a new shard.
        /// </summary>
        public void CreateData(string tableName)
        {
            BulkCopyTable(tableName, tableName);
        }

        /// <summary>
        /// Synchronizes the reference data within an existing shard.
        /// </summary>
        public void SyncData(string tableName, string tempTableName, string syncProcedure)
        {
            BulkCopyTable(tableName, tempTableName);
            SyncTempTableToReferenceTable(syncProcedure);
        }

        private void BulkCopyTable(string sourceTable, string targetTable)
        {
            var copier = new BulkCopier(_referenceDataConnectionString, _shardConnection.ConnectionString);

            copier.Copy(sourceTable, targetTable);
        }

        private void SyncTempTableToReferenceTable(string syncProcedure)
        {
            using (var sqlConnection = new ReliableSqlConnection(_shardConnection.ConnectionString))
            {
                sqlConnection.Open();
                var command = new SqlCommand(syncProcedure, sqlConnection.Current);

                command.ExecuteNonQuery();
            }
        }

        #endregion
    }
}