#region usings

using System;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Settings;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.CacheModels
{
    /// <summary>
    /// Class CacheSetting is the serializable object held in the object cache for settings.
    /// </summary>
    [Serializable]
    public class CacheSetting
    {
        #region properties

        /// <summary>
        /// Gets or sets the admin user.
        /// </summary>
        /// <value>The admin user.</value>
        public string AdminUser { get; set; }
        /// <summary>
        /// Gets or sets the encrypted admin password.
        /// </summary>
        /// <value>The encrypted admin password.</value>
        public string EncryptedAdminPassword { get; set; }
        /// <summary>
        /// Gets or sets the encrypted shard password.
        /// </summary>
        /// <value>The encrypted shard password.</value>
        public string EncryptedShardPassword { get; set; }
        /// <summary>
        /// Gets or sets the shard prefix.
        /// </summary>
        /// <value>The shard prefix.</value>
        public string ShardPrefix { get; set; }
        /// <summary>
        /// Gets or sets the shard user.
        /// </summary>
        /// <value>The shard user.</value>
        public string ShardUser { get; set; }

        #endregion

        #region methods

        /// <summary>
        /// Convert to the the Azure Table Storage model for settings.
        /// </summary>
        /// <returns>AzureSetting.</returns>
        public AzureSetting ToAzureSetting()
        {
            return new AzureSetting
            {
                AdminUser = AdminUser,
                EncryptedAdminPassword = EncryptedAdminPassword,
                EncryptedShardPassword = EncryptedShardPassword,
                ShardPrefix = ShardPrefix,
                ShardUser = ShardUser,
            };
        }

        #endregion
    }
}