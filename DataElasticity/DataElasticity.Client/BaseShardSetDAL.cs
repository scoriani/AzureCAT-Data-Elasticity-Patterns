#region usings

using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Client
{
    /// <summary>
    /// Class BaseShardSetDal .
    /// </summary>
    public class BaseShardSetDal
    {
        #region fields

        private readonly string _shardSetName;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseShardSetDal"/> class.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        public BaseShardSetDal(string shardSetName)
        {
            _shardSetName = shardSetName;
        }

        #endregion

        #region methods

        /// <summary>
        /// Executes the command text against the shard connection in a non query fashion.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="distributionKey">The distribution key.</param>
        /// <returns>System.Int32 return value of the execution.</returns>
        public int ExecuteNonQuery(string commandText, long distributionKey)
        {
            var shardlet = Shardlet.Load(_shardSetName, distributionKey);
            return ExecuteNonQuery(commandText, GetReliableConnection(shardlet));
        }

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// Executes the command text against the shard connection in a non query fashion.
        /// <param name="shardingKey">The sharding key.</param>
        /// <returns>System.Int32 return value of the execution.</returns>
        public int ExecuteNonQuery(string commandText, string shardingKey)
        {
            var shardlet = Shardlet.Load(_shardSetName, shardingKey);
            return ExecuteNonQuery(commandText, new ReliableSqlConnection(shardlet.ConnectionString).Current);
        }

        /// <summary>
        /// Executes the command text against the shard connection in a non query fashion.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="dataSet">The data set.</param>
        /// <returns>System.Int32 return value of the execution.</returns>
        public void ExecuteNonQuery(string commandText, string dataSet, string guid)
        {
            var shardlet = Shardlet.Load(_shardSetName, dataSet, guid);
            ExecuteNonQuery(commandText, new ReliableSqlConnection(shardlet.ConnectionString).Current);
        }

        /// <summary>
        /// Executes the non query on all shards in the shard map.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <remarks>
        /// NOTE: This only works properly if the current configuration is in sync with the 
        /// actual shards.
        /// </remarks>
        public void ExecuteNonQuery(string commandText)
        {
            var shardSetConfig = ShardSetConfig.LoadCurrent(_shardSetName);
            var shardMap = shardSetConfig.ShardMap;
            var settings = Settings.Load();

            var totalShards = shardMap.Shards.Count;
            var tasks = new Task[totalShards];
            var i = 0;
            foreach (var shard in shardMap.Shards.ToList())
            {
                var connString = "Server=" + shard.ServerInstanceName + ";Database=" + shard.Catalog + ";User Id=" +
                                 settings.ShardUser + ";Password=" + settings.ShardPassword + ";";
                tasks[i] = Task.Run(() => ExecuteNonQuery(commandText, new ReliableSqlConnection(connString).Current));
                i++;
            }

            var allComplete = false;
            while (!allComplete)
            {
                foreach (var task in tasks)
                {
                    if (!task.Status.Equals(TaskStatus.RanToCompletion))
                    {
                        if (task.Status.Equals(TaskStatus.Faulted))
                        {
                            Debug.WriteLine("Faulted: " + task.Exception.InnerException.Message);
                        }
                        Thread.Sleep(25);
                        break;
                    }

                    allComplete = true;
                }
            }
        }

        //Return a result set

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="distributionKey">The distribution key.</param>
        /// <returns>DataTable.</returns>
        public DataTable ExecuteQuery(string shardSetName, string commandText, long distributionKey)
        {
            var shardlet = Shardlet.Load(shardSetName, distributionKey);
            return ExecuteQuery(commandText, new ReliableSqlConnection(shardlet.ConnectionString).Current);
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="shardingKey">The sharding key.</param>
        /// <returns>DataTable.</returns>
        public DataTable ExecuteQuery(string commandText, string shardingKey)
        {
            var shardlet = Shardlet.Load(_shardSetName, shardingKey);
            return ExecuteQuery(commandText, new ReliableSqlConnection(shardlet.ConnectionString).Current);
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="dataSet">The data set.</param>
        /// <returns>DataTable.</returns>
        public DataTable ExecuteQuery(string shardSetName, string commandText, string guid, string dataSet)
        {
            var shardlet = Shardlet.Load("tpch", dataSet, guid);
            return ExecuteQuery(commandText, new ReliableSqlConnection(shardlet.ConnectionString).Current);
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns>DataTable.</returns>
        public DataTable ExecuteQuery(string commandText)
        {
            //todo: needs another look on how the tasks are being run
            var shardSetConfig = ShardSetConfig.LoadCurrent(_shardSetName);
            var shardMap = shardSetConfig.ShardMap;
            var settings = Settings.Load();
            var finalResult = new DataTable();

            var totalShards = shardMap.Shards.Count;
            var tasks = new Task<DataTable>[totalShards];
            var i = 0;
            foreach (var shard in shardMap.Shards.ToList())
            {
                var connectionString =
                    "Server=" + shard.ServerInstanceName
                    + ";Database=" + shard.Catalog
                    + ";User Id=" + settings.ShardUser
                    + ";Password=" + settings.ShardPassword
                    + ";";

                tasks[i] = Task.Run(() => ExecuteQuery(commandText, new ReliableSqlConnection(connectionString).Current));
                i++;
            }

            var allComplete = false;
            while (!allComplete)
            {
                foreach (Task task in tasks)
                {
                    if (!task.Status.Equals(TaskStatus.RanToCompletion))
                    {
                        if (task.Status.Equals(TaskStatus.Faulted))
                        {
                            Debug.WriteLine("Faulted: " + task.Exception.InnerException.Message);
                            return null;
                        }
                        Thread.Sleep(25);
                        break;
                    }

                    allComplete = true;
                }
            }

            foreach (var dt in tasks)
            {
                finalResult.Merge(dt.Result);
            }

            return finalResult;
        }

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="connection">The SQL connection.</param>
        /// <returns>System.Int32 return value of the execution.</returns>
        private int ExecuteNonQuery(string commandText, SqlConnection connection)
        {
            int result;
            using (connection)
            {
                var command = new SqlCommand(commandText, connection);
                connection.Open();

                result = command.ExecuteNonQuery();
            }

            return result;
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="connection">The SQL connection.</param>
        /// <returns>DataTable.</returns>
        private DataTable ExecuteQuery(string commandText, SqlConnection connection)
        {
            var dataTable = new DataTable();
            using (connection)
            {
                var command = new SqlCommand(commandText, connection);
                connection.Open();

                var reader = command.ExecuteReader();
                dataTable.Load(reader);
            }

            return dataTable;
        }

        private static SqlConnection GetReliableConnection(Shardlet shardlet)
        {
            return new ReliableSqlConnection(shardlet.ConnectionString).Current;
        }

        #endregion
    }
}