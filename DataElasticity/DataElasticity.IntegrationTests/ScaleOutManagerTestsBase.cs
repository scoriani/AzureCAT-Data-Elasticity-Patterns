#region usings

using System;
using System.Configuration;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Configuration;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.IntegrationTests
{
    public class ScaleOutManagerTestsBase
    {
        #region constants

        protected const string TestShardSetName = "AWSales";
        private const string _referenceDataConnectionStringSetting = "AwMainReferenceData";

        #endregion

        #region methods

        protected static void AddDacPacSettings(ShardSetConfig shardSetConfig)
        {
            shardSetConfig.SetShardSetSetting("DacPacBlobName", @"AWSales.dacpac");
            shardSetConfig.SetShardSetSetting("DacPacProfileBlobName", @"AWSales.Deploy.azuredb.publish.xml");
            shardSetConfig.SetShardSetSetting("DacPacSyncProfileBlobName", @"AWSales.Sync.azuredb.publish.xml");
            shardSetConfig.SetShardSetSetting("DacPacShouldDeployOnSync", false.ToString());
        }

        protected static string GetReferenceConnectionString()
        {
            var configurationSetting = ConfigurationManager.ConnectionStrings[_referenceDataConnectionStringSetting];
            return configurationSetting.ConnectionString;
        }

        protected Server MakeServer()
        {
            var r = new Random();
            return (new Server
            {
                ServerInstanceName = "TestServer" + DateTime.Now.Ticks,
                Location = "Test Environment",
                MaxShardsAllowed = r.Next(4) + 2
            }).Save();
        }

        protected static Settings SaveTestSettings()
        {
            var setting = new Settings
            {
                ShardPrefix = "AdvWrk",
                ShardUser = "Batman",
                ShardPassword = "Blank123",
                AdminUser = "Superman",
                AdminPassword = "Blank123"
            };

            setting.Save();

            return setting;
        }

        protected static void SetUpRetryPolicy()
        {
            using (var config = new SystemConfigurationSource())
            {
                var settings = RetryPolicyConfigurationSettings.GetRetryPolicySettings(config);

                // Initialize the RetryPolicyFactory with a RetryManager built from the 
                // settings in the configuration file.
                var buildRetryManager = settings.BuildRetryManager();
                RetryPolicyFactory.SetRetryManager(buildRetryManager);
            }
        }

        protected static ShardSetConfig SetupTestShardSetConfig()
        {
            // Get the test shard set configuration
            var shardSetConfig = ShardSetConfig.LoadCurrent(TestShardSetName);

            // Get the test server
            var serverLocation = ConfigurationManager.AppSettings["TestSQLServer"] ?? @"(localdb)\v11.0";
            var server = GetTestServer(serverLocation);

            // For testing don;t worry about how many shards are in the server
            server.MaxShardsAllowed = -1;
            server = server.Save();

            // Clear out the servers and just add the test server
            shardSetConfig.Servers.Clear();
            shardSetConfig.Servers.Add(server);

            // Update the shard map to point to the databases to the new server
            shardSetConfig.UpdateShardMap();

            shardSetConfig.Save();

            return shardSetConfig;
        }

        private static Server GetTestServer(string serverLocation)
        {
            var server = Server.Load(serverLocation);
            if (server == null)
            {
                server = new Server
                {
                    ServerInstanceName = serverLocation,
                    Location = "Test Server Location",
                    MaxShardsAllowed = -1,
                };
            }
            return server;
        }

        #endregion
    }
}