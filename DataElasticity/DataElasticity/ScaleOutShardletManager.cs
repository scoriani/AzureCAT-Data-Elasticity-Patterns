#region usings

using System;
using Microsoft.AzureCat.Patterns.DataElasticity.Interfaces;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;
using Microsoft.Practices.Unity;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity
{
    /// <summary>
    /// This is a utility class for interfacing with the IShardSetConnectionDriver
    /// </summary>
    internal class ScaleOutShardletManager : UnityBasedManager<ScaleOutShardletManager>
    {
        #region fields

        private Object _instanceLock = new object();

        #endregion

        #region methods

        /// <summary>
        /// Connects the specified shardlet in the connection map.
        /// </summary>
        /// <param name="shardlet">The shardlet.</param>
        /// <param name="spid">The spid.</param>
        public void Connect(Shardlet shardlet, short spid)
        {
            var shardletConnectionDriver = GetShardletConnectionDriver(shardlet.ShardSetName);
            shardletConnectionDriver.PublishShardletConnection(shardlet, spid);
        }

        /// <summary>
        /// Disconnects the specified shardlet from the connection map
        /// </summary>
        /// <param name="shardlet">The shardlet.</param>
        /// <param name="spid">The spid.</param>
        public void Disconnect(Shardlet shardlet, short spid)
        {
            var shardletConnectionDriver = GetShardletConnectionDriver(shardlet.ShardSetName);
            shardletConnectionDriver.RemoveShardletConnection(shardlet, spid);
        }

        //private Dictionary<string, IShardSetConnectionDriver> _cachedDrivers = new Dictionary<string, IShardSetConnectionDriver>();

        public Shardlet GetShardlet(string shardSetName, string shardingKey, short? spid = null)
        {
            var shardletConnectionDriver = GetShardletConnectionDriver(shardSetName);
            var settings = ScaleOutSettingsManager.GetManager().GetSettings();

            var shardSetDriver = _container.Resolve<IShardSetDriver>(shardSetName);
            var distributionKey = shardSetDriver.GetDistributionKeyForShardingKey(shardingKey);

            var shardlet = shardletConnectionDriver.GetShardletByShardingKey(shardSetName, shardingKey, distributionKey);

            if (shardlet == null)
            {
                // would only arrive here if the shardmap is not yet
                // published and the shardlet is not pinned somewhere
                return null;
            }

            shardlet.UserName = settings.ShardUser;
            shardlet.Password = settings.ShardPassword;

            if (spid.HasValue)
            {
                shardletConnectionDriver.PublishShardletConnection(shardlet, spid.Value);
            }

            return shardlet;
        }

        public Shardlet GetShardlet(string shardSetName, long distributionKey, bool isNew = false)
        {
            var driver = GetShardletConnectionDriver(shardSetName);
            var settings = ScaleOutSettingsManager.GetManager().GetSettings();

            var shardletConnection = driver.GetShardletByDistributionKey(shardSetName, distributionKey, isNew);
            shardletConnection.UserName = settings.ShardUser;
            shardletConnection.Password = settings.ShardPassword;

            return shardletConnection;
        }

        public Shardlet GetShardlet(string shardSetName, string dataSetName, string uniqueValue)
        {
            var driver = GetShardletConnectionDriver(shardSetName);
            var settings = ScaleOutSettingsManager.GetManager().GetSettings();
            var shardletConnection = driver.GetShardlet(shardSetName, dataSetName, uniqueValue);
            shardletConnection.UserName = settings.ShardUser;
            shardletConnection.Password = settings.ShardPassword;
            return shardletConnection;
        }

        public IShardSetConnectionDriver GetShardletConnectionDriver(string shardSetName)
        {
            //if (!_cachedDrivers.Keys.Contains(shardSetName))
            //{
            //    lock (_instanceLock)
            //    {
            //        if (!_cachedDrivers.Keys.Contains(shardSetName))
            //        {
            //            _cachedDrivers.Add(shardSetName, _container.Resolve<IShardSetConnectionDriver>());
            //        }
            //    }
            //}
            //return _cachedDrivers[shardSetName];
            return _container.Resolve<IShardSetConnectionDriver>(shardSetName);
        }

        #endregion
    }
}