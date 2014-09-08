#region usings

using System;
using System.Configuration;
using System.Linq;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Settings;
using Microsoft.AzureCat.Patterns.DataElasticity.Interfaces;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;
using Microsoft.WindowsAzure.Storage;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore
{
    public class AzureSettingsRepository : ISettingsRepository
    {
        #region fields

        private readonly AzureSettingContext _ServiceContext;
        private readonly string _encryptionKey;
        //private readonly string AzureCacheKey = "ShardSettingsRepositoryCache";
        private readonly object _lock = new object();
        //private DataCache AzureCache = null;
        private DateTime _lastCacheCheck;
        private AzureSetting _settingCache;

        #endregion

        #region constructors

        public AzureSettingsRepository(string encryptionKey)
        {
            _encryptionKey = encryptionKey;
            var connectionString = ConfigurationManager.ConnectionStrings["AzureStorage"];
            if (connectionString == null)
            {
                throw new Exception("Connection string to azure storage is required in your app.config");
            }
            var storageAccount = CloudStorageAccount.Parse(connectionString.ConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var settingsTable = tableClient.GetTableReference(AzureSettingContext.SettingsTable);

            settingsTable.CreateIfNotExists();
            
            _ServiceContext = new AzureSettingContext(tableClient);

            //AzureCache = new DataCache("default");
        }

        #endregion

        #region ISettingsRepository

        public Settings GetSettings()
        {
            if (_settingCache != null && _lastCacheCheck > DateTime.Now.AddMinutes(-5))
            {
                return _settingCache.ToFrameworkSetting(_encryptionKey);
            }
            lock (_lock)
            {
                var azureSetting = _ServiceContext.Settings.FirstOrDefault();
                if (azureSetting == null)
                {
                    return new Settings();
                }
                //AzureCache.Put(AzureCacheKey, azureSetting.ToCacheSetting());
                _settingCache = azureSetting;
                _lastCacheCheck = DateTime.Now;
                return azureSetting.ToFrameworkSetting(_encryptionKey);
            }
        }

        public Settings SaveSettings(Settings settings)
        {
            var result = _ServiceContext.Settings.ToList();
            result.ForEach(x => _ServiceContext.DeleteObject(x));
            var newSetting = new AzureSetting(_encryptionKey, settings);
            _ServiceContext.AddObject(AzureSettingContext.SettingsTable, newSetting);
            _ServiceContext.SaveChanges();
            //AzureCache.Remove(AzureCacheKey);
            //AzureCache.Put(AzureCacheKey, newSetting.ToCacheSetting());
            lock (_lock)
            {
                _settingCache = newSetting;
                _lastCacheCheck = DateTime.Now;
            }
            return settings;
        }

        #endregion
    }
}