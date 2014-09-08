#region usings

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using StackExchange.Redis;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore
{
    /// <summary>
    /// Class AzureCache implements a static, singleton cache used by the
    /// Azure Store Implementation.  It is implemented on top of the Redis service.
    /// </summary>
    public static class AzureCache
    {
        #region constants

        private const string _cacheConfigurationStringKey = "RedisConnectionString";
        private const string _shouldUseAzureCacheConfigurationKey = "ShouldUseAzureCache";

        #endregion

        #region fields

        private static readonly Lazy<string> _connectionString =
            new Lazy<string>(() =>
            {
                InitializeCacheConnectionString();

                return _connectionStringValue;
            });

        private static readonly Lazy<bool> _shouldUseAzureCache =
            new Lazy<bool>(() =>
            {
                InitializeShouldUseAzureCache();

                return _shouldUseAzureCacheValue;
            });

        private static ConnectionMultiplexer _connection;
        private static string _connectionStringValue;
        private static bool _shouldUseAzureCacheValue;

        #endregion

        #region properties

        private static ConnectionMultiplexer Connection
        {
            get
            {
                if (!ShouldUseAzureCache) return null;

                if (_connection == null || !_connection.IsConnected)
                {
                    _connection = ConnectionMultiplexer.Connect(ConnectionString);
                }

                return _connection;
            }
        }

        private static string ConnectionString
        {
            get { return _connectionString.Value; }
        }

        private static bool ShouldUseAzureCache
        {
            get { return _shouldUseAzureCache.Value; }
        }

        #endregion

        #region methods

        /// <summary>
        /// Clears the Azure Redis Cache.
        /// </summary>
        public static void Clear()
        {
            if (!ShouldUseAzureCache) return;

            var servers = 
                Connection
                    .GetEndPoints()
                    .Select(ep => Connection.GetServer(ep));

            foreach (var server in servers)
            {
                server.FlushDatabase();
            }
        }

        /// <summary>
        /// Gets an object of type T from the cache using the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the object in cache.</typeparam>
        /// <param name="key">The unique value that is used to identify the object in the cache.</param>
        /// <returns>T or null if the item does not exist or cache is not configured</returns>
        public static T Get<T>(string key)
        {
            return !ShouldUseAzureCache ? default(T) : Connection.GetDatabase().Get<T>(key);
        }

        /// <summary>
        /// Gets an object from the cache using the specified key.
        /// </summary>
        /// <param name="key">The unique value that is used to identify the object in the cache.</param>
        /// <returns>System.Object or null if the item does not exist or cache is  not configured</returns>
        public static object Get(string key)
        {
            return !ShouldUseAzureCache ? null : Connection.GetDatabase().Get(key);
        }

        /// <summary>
        /// Adds or replaces and object in the cache using the specified key.
        /// </summary>
        /// <param name="key">The unique value that is used to identify the object in the cache.</param>
        /// <param name="obj">The object to add or replace in the cache.</param>
        public static void Put(string key, object obj)
        {
            if (!ShouldUseAzureCache) return;

            Connection.GetDatabase().Set(key, obj, flags: CommandFlags.FireAndForget);
        }

        /// <summary>
        /// Removes and object from the cache using the the specified key.
        /// </summary>
        /// <param name="key">The unique value that is used to identify the object in the cache.</param>
        public static void Remove(string key)
        {
            if (!ShouldUseAzureCache) return;

            Connection.GetDatabase().KeyDelete(key);
        }

        private static void InitializeShouldUseAzureCache()
        {
            var setting = CloudConfigurationManager.GetSetting(_shouldUseAzureCacheConfigurationKey);

            if (setting == null)
            {
                _shouldUseAzureCacheValue = false;
            }

            Boolean.TryParse(setting, out _shouldUseAzureCacheValue);
        }

        private static void InitializeCacheConnectionString()
        {
            _connectionStringValue = CloudConfigurationManager.GetSetting(_cacheConfigurationStringKey);

            if (_connectionStringValue == null)
            {
                throw new InvalidOperationException(
                    string.Format("Connection string to Redis cache {0} is required in your app.config",
                        _cacheConfigurationStringKey));
            }
        }

        #endregion
    }
}