#region usings

using System;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Contrib
{
    public class BulkCopier
    {
        #region fields

        private readonly string _sourceConnectionString;
        private readonly string _targetConnectionString;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkCopier"/> class.
        /// </summary>
        /// <param name="sourceConnectionString">The source connection string.</param>
        /// <param name="targetConnectionString">The target connection string.</param>
        public BulkCopier(string sourceConnectionString, string targetConnectionString)
        {
            _sourceConnectionString = sourceConnectionString;
            _targetConnectionString = targetConnectionString;
        }

        #endregion

        #region methods

        /// <summary>
        /// Copies the specified source table.
        /// </summary>
        /// <param name="sourceTable">The source table.</param>
        /// <param name="targetTable">The target table.</param>
        /// <param name="truncateTarget">if set to <c>true</c> truncate the target table.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <param name="timeout">The timeout in seconds.</param>
        public void Copy(string sourceTable, string targetTable, bool truncateTarget = true, int batchSize = 100,
            int timeout = 300)
        {
            using (var sourceSqlConnection = new ReliableSqlConnection(_sourceConnectionString))
            {
                sourceSqlConnection.Open();

                var readCommand = new SqlCommand(string.Format("SELECT * FROM {0}", sourceTable),
                    sourceSqlConnection.Current);
                var reader = readCommand.ExecuteReader();

                Copy(reader, targetTable, truncateTarget, batchSize, timeout);
            }
        }

        /// <summary>
        /// Copies the specified data reader output to the target table.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="targetTable">The target table.</param>
        /// <param name="truncateTarget">if set to <c>true</c> truncate the target table.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <param name="timeout">The timeout.</param>
        public void Copy(SqlDataReader reader, string targetTable, bool truncateTarget = true, int batchSize = 100,
            int timeout = 300)
        {
            using (var targetSqlConnection = new ReliableSqlConnection(_targetConnectionString))
            {
                targetSqlConnection.Open();

                if (truncateTarget)
                {
                    var deleteCommand =
                        new SqlCommand(string.Format("DELETE FROM {0}", targetTable), targetSqlConnection.Current);

                    deleteCommand.ExecuteNonQuery();
                }

                try
                {
                    const SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.KeepNulls;

                    var bulkCopy =
                        new SqlBulkCopy(targetSqlConnection.Current, sqlBulkCopyOptions, null)
                        {
                            DestinationTableName = targetTable,
                            BatchSize = batchSize,
                            BulkCopyTimeout = timeout,
                        };

                    bulkCopy.WriteToServer(reader);
                }
                catch (Exception e)
                {
                    // todo: Log
                    throw;
                }
            }
        }

        #endregion
    }
}