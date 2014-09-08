#region usings

using System;
using System.Configuration;
using System.Linq;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.GloballyUniqueValues;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.IntegrationTests
{
    [TestClass]
    //todo: review and clean up these tests.
    public class ScaleOutManagerTests : ScaleOutManagerTestsBase
    {
        #region methods

        /// <summary>
        /// Test adding a new server to the configuration repository.  
        /// </summary>
        [TestMethod]
        public void Add_New_Server()
        {
            var r = new Random();

            var newServer =
                new Server
                {
                    ServerInstanceName = "TestServer" + DateTime.Now.Ticks,
                    Location = "Test Environment",
                    MaxShardsAllowed = r.Next(5)
                };

            var server = newServer.Save();

            Assert.IsTrue(server.ServerID != 0);
        }

        /// <summary>
        /// Adds the configured shards to Azure at the configured locations.
        /// </summary>
        [TestMethod]
        public void Add_ShardMap_To_Azure()
        {
            var currentConfig = ShardSetConfig.LoadCurrent(TestShardSetName);
            currentConfig.PublishShardMap();
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            SetUpRetryPolicy();
            SaveTestSettings();
        }

        [TestMethod]
        public void CreateGloballyUniqueValue()
        {
            var value = new GloballyUniqueValue("asset", "utilitybelt") {DistributionKey = 5797866};
            var azureGloballyUniqueValue = new AzureGloballyUniqueValue();

            try
            {
                azureGloballyUniqueValue.DeleteGloballyUniqueValue(value.PartitionKey, value.RowKey);
                azureGloballyUniqueValue.AddGloballyUniqueValue(value.PartitionKey, value.RowKey, value.DistributionKey);
            }
            catch (Exception)
            {
                //TODO: Log to log provider.
                throw;
            }
        }

        [TestMethod]
        public void Deploy_Shards()
        {
            SaveTestShardSetConfig();

            var shardSetConfig = SetupTestShardSetConfig();

            shardSetConfig.DeployShardMap();
        }

        [TestMethod]
        public void Deploy_Shards_Async()
        {
            SaveTestShardSetConfig();

            var shardSetConfig = SetupTestShardSetConfig();

            shardSetConfig.DeployShardMap(true);

            //var queue = ScaleOutQueueManager.GetManager().GetQueue();
            //queue.QueueProcessingEvent += Queue_QueueProcessingEvent;

            //queue.CheckAndProcessQueue();
        }

        [TestMethod]
        public void Elastic_Scale_Down_Test()
        {
            SaveTestSettings();
            SaveTestShardSetConfig();
            DeployCurrentShardSetConfigAsync();

            // process the queues
            var queue = ShardSetActionQueue.GetQueue();
            queue.QueueProcessingEvent += Queue_QueueProcessingEvent;

            queue.CheckAndProcessQueue(Guid.NewGuid());

            // create some test data across the shards
            //CreateTestData();

            // Reread the test shard set configuration to get the Current Shard Map Id value
            // of prior deployment
            var shardSetConfig = ShardSetConfig.LoadCurrent(TestShardSetName);

            // create a new configuration with fewer shards
            shardSetConfig.TargetShardCount = 3;
            shardSetConfig.MaxShardCount = 3;
            shardSetConfig.UpdateShardMap();
            shardSetConfig.Save();

            // redeploy and publish async
            shardSetConfig.DeployShardMap(true);
            shardSetConfig.PublishShardMap(true);

            // execute from queues until clear
            queue.CheckAndProcessQueue(Guid.NewGuid());
        }

        [TestMethod]
        public void Elastic_Scale_Up_Test()
        {
            SaveTestSettings();
            SaveTestShardSetConfig();
            DeployCurrentShardSetConfigAsync();

            // process the queues
            var queue = ShardSetActionQueue.GetQueue();
            queue.QueueProcessingEvent += Queue_QueueProcessingEvent;

            queue.CheckAndProcessQueue(Guid.NewGuid());

            // create some test data across the shards
            //CreateTestData();

            // Reread the test shard set configuration to get the Current Shard Map Id value
            // of prior deployment
            var shardSetConfig = ShardSetConfig.LoadCurrent(TestShardSetName);

            // create a new configuration with more shards
            shardSetConfig.TargetShardCount = 8;
            shardSetConfig.MaxShardCount = 8;
            shardSetConfig.UpdateShardMap();
            shardSetConfig.Save();

            // redeploy and publish async
            shardSetConfig.DeployShardMap(true);
            shardSetConfig.PublishShardMap(true);

            // execute from queues until clear
            queue.CheckAndProcessQueue(Guid.NewGuid());
        }

        [TestMethod]
        public void Get_Server_By_ID()
        {
            var server = MakeServer();
            var loadedServer = Server.Load(server.ServerID);
            Assert.IsTrue(loadedServer.ServerInstanceName == server.ServerInstanceName);
        }

        [TestMethod]
        public void Get_Server_By_Name()
        {
            var server = MakeServer();
            var loadedServer = Server.Load(server.ServerInstanceName);
            Assert.IsTrue(loadedServer.ServerID == server.ServerID);
        }

        [TestMethod]
        public void Get_Servers()
        {
            var serverList = Server.GetServers();
            Assert.IsTrue(serverList.Any());
        }


        [TestMethod]
        public void Load_Edit_Save_Table_Group_Config()
        {
            var r = new Random();
            var dbSize = r.Next(250);
            var currentConfig = ShardSetConfig.LoadCurrent(TestShardSetName);
            currentConfig.MaxShardSizeMb = dbSize;
            currentConfig.Save();
            var newCurrentConfig = ShardSetConfig.LoadCurrent(TestShardSetName);
            Assert.AreEqual(dbSize, newCurrentConfig.MaxShardSizeMb);
        }

        [TestMethod]
        public void Modify_Server()
        {
            var server = MakeServer();

            var loadedServer = Server.Load(server.ServerInstanceName);
            loadedServer.ServerInstanceName = "My New Server Name" + DateTime.Now.Ticks;
            loadedServer.Save();

            var reloadedServer = Server.Load(loadedServer.ServerInstanceName);
            Assert.IsTrue(loadedServer.ServerInstanceName == reloadedServer.ServerInstanceName);
        }

        [TestMethod]
        public void Publish_Shards_Asycn()
        {
            var currentConfig = ShardSetConfig.LoadCurrent(TestShardSetName);
            currentConfig.Servers.Clear();

            var serverLocation = ConfigurationManager.AppSettings["TestSQLServer"] ?? "(localdb)\v11.0";
            var server =
                Server.Load(serverLocation) ??
                new Server
                {
                    ServerInstanceName = serverLocation,
                    Location = "Test Server Location",
                    MaxShardsAllowed = -1,
                };

            server.MaxShardsAllowed = -1;
            server = server.Save();

            currentConfig.Servers.Add(server);
            currentConfig.Save();
            currentConfig.UpdateShardMap();
            currentConfig.Save();
            currentConfig.DeployShardMap(true);
            currentConfig.PublishShardMap(true);

            var queue = ShardSetActionQueue.GetQueue();

            queue.QueueProcessingEvent += Queue_QueueProcessingEvent;
            queue.CheckAndProcessQueue(Guid.NewGuid());
        }

        [TestMethod]
        public void Save_New_Table_Group_Config()
        {
            var config = SaveTestShardSetConfig();

            config.Servers.Add(MakeServer());
            config.Servers.Add(MakeServer());
            config.Servers.Add(MakeServer());
            config.Servers.Add(MakeServer());
            config.Servers.Add(MakeServer());

            config.UpdateShardMap();

            Assert.IsTrue(config.ShardMap.Shards.Count() == config.TargetShardCount);

            config.Save();
        }

        [TestMethod]
        public void Save_Settings()
        {
            var setting = SaveTestSettings();

            var newSetting = Settings.Load();
            Assert.IsTrue(setting.ShardPassword == newSetting.ShardPassword);
        }

        [TestMethod]
        public void Sync_Shards()
        {
            var shardSetConfig = SetupTestShardSetConfig();

            shardSetConfig.SyncShards();
        }

        [TestMethod]
        public void Sync_Shards_Async()
        {
            var shardSetConfig = SetupTestShardSetConfig();

            shardSetConfig.SyncShards(true);

            var queue = ShardSetActionQueue.GetQueue();
            queue.QueueProcessingEvent += Queue_QueueProcessingEvent;

            queue.CheckAndProcessQueue(Guid.NewGuid());
        }

        //private static void CreateTestData()
        //{
        //    var connectionString = GetReferenceConnectionString();
        //    var builder = new TestDataBuilder(connectionString);

        //    const int initialTestCustomerID = 1;
        //    const int numberOfTestCustomers = 100;
        //    const int numberOfTestOrdersPerCustomer = 5;
        //    builder.AddTestDataInShardSet(TestShardSetName, initialTestCustomerID, numberOfTestCustomers, numberOfTestOrdersPerCustomer);
        //}

        private static void DeployCurrentShardSetConfigAsync()
        {
            var shardSetConfig = SetupTestShardSetConfig();

            shardSetConfig.DeployShardMap(true);
            shardSetConfig.PublishShardMap(true);
        }

        private void Queue_QueueProcessingEvent(string message)
        {
            Console.WriteLine(message);
        }

        private static ShardSetConfig SaveTestShardSetConfig()
        {
            var shardSetConfig =
                new ShardSetConfig
                {
                    AllowDeployments = true,
                    CurrentShardCount = 0,
                    MaxShardCount = 5,
                    MaxShardSizeMb = 100,
                    MaxShardletsPerShard = 1000,
                    MinShardSizeMb = 10,
                    TargetShardCount = 5,
                    ShardSetName = TestShardSetName
                };

            // add dacpac settings
            AddDacPacSettings(shardSetConfig);

            shardSetConfig.Save();

            return shardSetConfig;
        }

        #endregion
    }
}