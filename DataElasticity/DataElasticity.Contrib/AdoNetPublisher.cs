#region usings

using System;
using System.Data;
using Microsoft.AzureCat.Patterns.DataElasticity.Interfaces;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Contrib
{
    public class AdoNetPublisher : PublisherBase
    {
        #region methods

        public void Publish(ShardBase shard, ShardSetConfig shardSetConfig, string shardUserName,
            string shardUserPassword, string sql, CommandType commandType)
        {
            var shardConnection =
                new ShardConnection
                {
                    ServerInstanceName = shard.ServerInstanceName,
                    Catalog = shard.Catalog,
                    UserName = shardUserName,
                    Password = shardUserPassword,
                    ShardSetName = shardSetConfig.ShardSetName
                };

            using (var sqlConnection = GetReliableConnection(shardConnection.ConnectionString))
            {
                try
                {
                    //open the connection
                    sqlConnection.Open();

                    // execute the command
                    var sqlCommand = sqlConnection.CreateCommand();
                    sqlCommand.CommandText = sql;
                    sqlCommand.CommandType = commandType;
                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // todo: Add Logging
                    throw;
                }
            }
        }

        #endregion
    }
}