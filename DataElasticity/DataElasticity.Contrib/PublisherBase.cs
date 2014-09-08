#region usings

using System;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Contrib
{
    /// <summary>
    /// Class PublisherBase is a common base class for publishers 
    /// working with scale out database shards.
    /// </summary>
    public class PublisherBase
    {
        #region methods

        /// <summary>
        /// Gets a default reliable connection with retry count of 3.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>ReliableSqlConnection.</returns>
        protected ReliableSqlConnection GetReliableConnection(String connectionString)
        {
            RetryPolicy myRetryPolicy = new RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>(3);

            var reliableConn = new ReliableSqlConnection(connectionString,
                myRetryPolicy);

            return reliableConn;
        }

        #endregion
    }
}