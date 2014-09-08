using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Microsoft.AzureCat.Patterns.DataElasticity.Client
{
    /// <summary>
    /// Class ElasticSqlConnection creates a SQL connection based on sharding key. 
    /// 
    /// This class cannot be inherited.
    /// </summary>
    public sealed class ElasticSqlConnection : IDbConnection
    {
        #region fields

        private readonly Shardlet _shardlet;
        private readonly SqlConnection _sqlConnection;
        private short _spid;

        #endregion

        #region properties

        /// <summary>
        /// Gets the current sql connection for use in commands, etc.
        /// </summary>
        /// <value>The current sql connection.</value>
        public SqlConnection Current
        {
            get { return _sqlConnection; }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ElasticSqlConnection"/> class.
        /// </summary>
        /// <param name="shardSetName">Name of the shard set.</param>
        /// <param name="shardingKey">The sharding key.</param>
        public ElasticSqlConnection(string shardSetName, string shardingKey)
        {
            _shardlet = Shardlet.Load(shardSetName, shardingKey);

            if (_shardlet == null)
            {
                throw new ElasticDataException(string.Format("The Data Elasticity cannot locate a pinned shardlet or shardlet range for this sharding key: {0}", shardingKey));
            }

            if (_shardlet.Status == ShardletStatus.Moving)
            {
                throw new ElasticDataException(string.Format("The Data Elasticity is currently moving the shardlet with sharding key: {0}", shardingKey));
            }

            _sqlConnection = new ReliableSqlConnection(_shardlet.ConnectionString).Current;
        }

        #endregion

        #region IDbConnection

        public IDbTransaction BeginTransaction()
        {
            return _sqlConnection.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return _sqlConnection.BeginTransaction(il);
        }

        public void ChangeDatabase(string databaseName)
        {
            throw new InvalidOperationException(
                "The database for an Elastic Connection cannot be changed.  It is calculated based on the Shard Set and Sharding Key");
        }

        public void Close()
        {
            if (State != ConnectionState.Closed && State != ConnectionState.Broken)
            {
                Shardlet.Disconnect(_shardlet, _spid);
                _sqlConnection.Close();
            }

            _spid = 0;
        }

        public string ConnectionString
        {
            get { return _sqlConnection.ConnectionString; }
            set
            {
                throw new InvalidOperationException(
                    "The connection string for an Elastic connection cannot be set.  It is calculated based on the Shard Set and Sharding Key");
            }
        }

        public int ConnectionTimeout
        {
            get { return _sqlConnection.ConnectionTimeout; }
        }

        public IDbCommand CreateCommand()
        {
            return _sqlConnection.CreateCommand();
        }

        public string Database
        {
            get { return _sqlConnection.Database; }
        }

        public void Dispose()
        {
            Close();
        }

        public void Open()
        {
            if (State == ConnectionState.Closed)
            {
                _sqlConnection.Open();

                _spid = GetSpid();

                Shardlet.Connect(_shardlet, _spid);
            }
        }

        public ConnectionState State
        {
            get { return _sqlConnection.State; }
        }

        #endregion

        #region methods

        private short GetSpid()
        {
            using (var sqlCommand = new SqlCommand("SELECT @@SPID", _sqlConnection))
            {
                return (short) sqlCommand.ExecuteScalar();
            }
        }

        #endregion
    }
}