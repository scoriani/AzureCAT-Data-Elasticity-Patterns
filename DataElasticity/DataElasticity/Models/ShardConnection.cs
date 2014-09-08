#region usings

using System;
using System.Configuration;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Models
{
    /// <summary>
    /// Class ShardConnection defines a connection to a ShardLet based on the ShardSetName and a configured
    /// template in the ConnectionStrings configuration.  
    /// </summary>
    /// <remarks>
    /// <para>
    /// ShardConnection can be used as a standalone
    /// class to generate connection strings.  It also is extended to provide connection
    /// services to a Shardlet.
    /// </para>
    /// <para>
    /// The connection string is built from the a connection string named as follows:
    /// <code>ShardSetName-Template</code>
    /// </para>
    /// <para>
    /// The template can contain any of the following tokens, which will then be substituted with the 
    /// properties on this class:
    /// <list type="bullet">
    /// <item>SHARD_CATALOG: Catalog</item>
    /// <item>SHARD_PASS: Password</item>
    /// <item>SHARD_SERVER: ServerInstanceName</item>
    /// <item>SHARD_USER: UserName</item>
    /// </list>
    /// </para>
    /// </remarks>
    public class ShardConnection
    {
        #region constants

        private const string _catalogToken = @"SHARD_CATALOG";
        private const string _passwordToken = @"SHARD_PASS";
        private const string _serverToken = @"SHARD_SERVER";
        private const string _userToken = @"SHARD_USER";

        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the catalog for the connection.
        /// </summary>
        /// <value>The catalog.</value>
        public string Catalog { get; set; }

        /// <summary>
        /// Gets the connection string built from the template .
        /// </summary>
        /// <value>The connection string.</value>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the Connection string ShardSetName-Template is not found in configuration.
        /// </exception>
        public string ConnectionString
        {
            get
            {
                var connectionString = ConfigurationManager.ConnectionStrings[ShardSetName + "-Template"];
                if (connectionString == null)
                {
                    throw new InvalidOperationException(
                        string.Format("Connection String \"{0}-Template\" was not found in the app.config", ShardSetName));
                }

                var returnString = connectionString.ConnectionString;

                if (!string.IsNullOrWhiteSpace(ServerInstanceName))
                    returnString = returnString.Replace(_serverToken, ServerInstanceName);
                if (!string.IsNullOrWhiteSpace(Catalog)) returnString = returnString.Replace(_catalogToken, Catalog);
                if (!string.IsNullOrWhiteSpace(UserName)) returnString = returnString.Replace(_userToken, UserName);
                if (!string.IsNullOrWhiteSpace(Password)) returnString = returnString.Replace(_passwordToken, Password);

                return returnString;
            }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the name of the server instance.
        /// </summary>
        /// <value>The name of the server instance.</value>
        public string ServerInstanceName { get; set; }

        /// <summary>
        /// Gets or sets the name of the shard set.
        /// </summary>
        /// <value>The name of the shard set.</value>
        public string ShardSetName { get; set; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        public string UserName { get; set; }

        #endregion
    }
}