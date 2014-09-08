#region usings

using System;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.CacheModels;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Settings
{
    public class AzureSetting : LongBasedRowKeyEntity
    {
        #region properties

        public string AdminUser { get; set; }
        public string EncryptedAdminPassword { get; set; }
        public string EncryptedShardPassword { get; set; }
        public string ShardPrefix { get; set; }
        public string ShardUser { get; set; }

        #endregion

        #region constructors

        public AzureSetting()
        {
            PartitionKey = "Settings";
            RowKey = DateTime.UtcNow.Ticks.ToString();
        }

        public AzureSetting(string secret, DataElasticity.Models.Settings settings)
        {
            PartitionKey = "Settings";
            RowKey = DateTime.UtcNow.Ticks.ToString();
            ShardPrefix = settings.ShardPrefix;
            ShardUser = settings.ShardUser;
            AdminUser = settings.AdminUser;
            SetAdminPassword(secret, settings.AdminPassword);
            SetShardPassword(secret, settings.ShardPassword);
        }

        #endregion

        #region methods

        public string GetAdminPassword(string secret)
        {
            return RijndaelManagedCrypto.DecryptStringAES(EncryptedAdminPassword, secret);
        }

        public string GetShardPassword(string secret)
        {
            return RijndaelManagedCrypto.DecryptStringAES(EncryptedShardPassword, secret);
        }

        public void SetAdminPassword(string secret, string password)
        {
            EncryptedAdminPassword = RijndaelManagedCrypto.EncryptStringAES(password, secret);
        }

        public void SetShardPassword(string secret, string password)
        {
            EncryptedShardPassword = RijndaelManagedCrypto.EncryptStringAES(password, secret);
        }

        public CacheSetting ToCacheSetting()
        {
            return new CacheSetting
            {
                AdminUser = AdminUser,
                EncryptedAdminPassword = EncryptedAdminPassword,
                EncryptedShardPassword = EncryptedShardPassword,
                ShardPrefix = ShardPrefix,
                ShardUser = ShardUser,
            };
        }

        public DataElasticity.Models.Settings ToFrameworkSetting(string secret)
        {
            return new DataElasticity.Models.Settings
            {
                AdminPassword = GetAdminPassword(secret),
                AdminUser = AdminUser,
                ShardPassword = GetShardPassword(secret),
                ShardPrefix = ShardPrefix,
                ShardUser = ShardUser,
            };
        }

        #endregion
    }
}