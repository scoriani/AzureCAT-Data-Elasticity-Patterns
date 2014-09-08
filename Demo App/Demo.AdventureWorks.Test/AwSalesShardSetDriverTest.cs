#region usings

using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Microsoft.AzureCat.Patterns.DataElasticity.Interfaces;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Demo.AdventureWorks.Test
{
    [TestClass]
    public class AwSalesShardSetDriverTest : ScaleOutManagerTestsBase
    {
        #region Tests

        [TestMethod]
        public void Copy_Shardlet()
        {
            // Assemble
            var shard1 = CreateShard("CopyShardletTest1");
            var shard2 = CreateShard("CopyShardletTest2");

            var shardSetConfig = CreateTestShardSetConfig();

            var driver = new AwSalesShardSetDriver();

            driver.CreateShard(shard1, shardSetConfig);
            driver.CreateShard(shard2, shardSetConfig);

            const int numberOfTestCustomers = 5;
            const int numberOfTestOrders = 5;
            const int initialTestCustomerID1 = 1;
            const int initialTestCustomerID2 = 1 + numberOfTestOrders;
            const int numberOfTestSalesLineNums = 4;
            AddTestShardlets(shard1, shardSetConfig, initialTestCustomerID1, numberOfTestCustomers, numberOfTestOrders, numberOfTestSalesLineNums);
            AddTestShardlets(shard2, shardSetConfig, initialTestCustomerID2, numberOfTestCustomers, numberOfTestOrders, numberOfTestSalesLineNums);

            // These are the distribution key for Customer IDs 1 and 3 in test data:
            const string shardingKey1 = "1";
            const string shardingKey3 = "3";

            // Act
            driver.CopyShardlet(shard1, shard2, shardSetConfig, shardingKey1, Guid.NewGuid());
            driver.CopyShardlet(shard1, shard2, shardSetConfig, shardingKey3, Guid.NewGuid());

            // Assert
        }

        [TestMethod]
        public void Create_And_Delete_Shard()
        {
            var shard = CreateShard("CreateAndDeleteShardTest");
            var shardSetConfig = CreateTestShardSetConfig();

            var driver = new AwSalesShardSetDriver();

            driver.CreateShard(shard, shardSetConfig);
            driver.DeleteShard(shard, shardSetConfig);
        }

        [TestMethod]
        public void Create_And_Sync_Shard()
        {
            var shard = CreateShard("CreateAndSyncShardTest");
            var shardSetConfig = CreateTestShardSetConfig();

            var driver = new AwSalesShardSetDriver();

            driver.CreateShard(shard, shardSetConfig);
            driver.SyncShard(shard, shardSetConfig);
        }

        [TestMethod]
        public void Delete_Shardlet()
        {
            // Assemble
            var shard = CreateShard("DeleteShardletTest");

            var shardSetConfig = CreateTestShardSetConfig();

            var driver = new AwSalesShardSetDriver();

            driver.CreateShard(shard, shardSetConfig);

            const int numberOfTestCustomers = 5;
            const int numberOfTestOrders = 5;
            const int initialTestCustomerID = 1;
            const int numberOfTestSalesLineNums = 4;
            AddTestShardlets(shard, shardSetConfig, initialTestCustomerID, numberOfTestCustomers, numberOfTestOrders, numberOfTestSalesLineNums);

            // Act
            var shardingKey = initialTestCustomerID.ToString(CultureInfo.InvariantCulture);
            driver.DeleteShardlet(shard, shardSetConfig, shardingKey);

            // Assert
        }

        //[TestMethod]
        //public void Get_Shard_Distribution_Keys_In_Range_Test()
        //{
        //    // Assemble
        //    var shard = CreateShard("GetShardDistributionKeysInRangeTest");
        //    var shardSetConfig = CreateTestShardSetConfig();

        //    var driver = new AwSalesShardSetDriver();

        //    driver.CreateShard(shard, shardSetConfig);

        //    const int numberOfTestCustomers = 50;
        //    const int numberOfTestOrders = 5;
        //    const int initialTestCustomerID = 1;
        //    const int numberOfTestSalesLineNums = 4;
        //    AddTestShardlets(shard, shardSetConfig, initialTestCustomerID, numberOfTestCustomers, numberOfTestOrders, numberOfTestSalesLineNums);

        //    // Act
        //    var ids =
        //        driver.GetShardDistributionKeysInRange(shard, shardSetConfig, -3348943546135474490, 663500469622951168)
        //            .OrderBy(i => i);

        //    // Assert
        //    Assert.IsTrue(ids.Any());
        //    Assert.AreEqual(11, ids.Count());
        //}

        //[TestMethod]
        //public void Get_Shard_Distribution_Keys_Test()
        //{
        //    // Assemble
        //    var shard = CreateShard("GetShardDistributionKeysTest");
        //    var shardSetConfig = CreateTestShardSetConfig();

        //    var driver = new AwSalesShardSetDriver();

        //    driver.CreateShard(shard, shardSetConfig);

        //    const int numberOfTestCustomers = 100;
        //    const int numberOfTestOrders = 1;
        //    const int initialTestCustomerID = 1;
        //    const int numberOfTestSalesLineNums = 4;
        //    AddTestShardlets(shard, shardSetConfig, initialTestCustomerID, numberOfTestCustomers, numberOfTestOrders, numberOfTestSalesLineNums);

        //    // Act
        //    var ids = driver.GetShardDistributionKeys(shard, shardSetConfig).ToArray();

        //    // Assert
        //    Assert.IsTrue(ids.Any());
        //    Assert.AreEqual(100, ids.Count());
        //}

        #endregion

        #region methods

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            SetUpRetryPolicy();
            SaveTestSettings();
        }

        private static void AddTestShardlets(ShardBase shard, ShardSetConfig shardSetConfig,
            int initialTestCustomerID, int numberOfTestCustomers, int numberOfTestOrders, int numberOfTestSalesLineNums)
        {
            var shardCatalogConnection =
                new ShardConnection
                {
                    ServerInstanceName = shard.ServerInstanceName,
                    Catalog = shard.Catalog,
                    UserName = "Superman",
                    Password = "Blank123",
                    ShardSetName = shardSetConfig.ShardSetName
                };

            var builder = new TestDataBuilder(GetReferenceConnectionString());
            builder.AddTestSalesOrdersInDatabase(shardCatalogConnection.ConnectionString, initialTestCustomerID, numberOfTestCustomers, numberOfTestOrders);
            builder.AddTestShoppingCartItemsInDatabase(shardCatalogConnection.ConnectionString, initialTestCustomerID, numberOfTestCustomers, numberOfTestSalesLineNums);
        }

        private static RangeShard CreateShard(string catalog)
        {
            var server = ConfigurationManager.AppSettings["TestSQLServer"] ?? @"(localdb)\v11.0";

            var shard = new RangeShard
            {
                Catalog = catalog,
                HighDistributionKey = 100,
                LowDistributionKey = 0,
                ServerInstanceName = server,
                ShardID = 1
            };

            return shard;
        }

        private static ShardSetConfig CreateTestShardSetConfig()
        {
            var shardSetConfig = new ShardSetConfig { ShardSetName = "AWSales" };

            AddDacPacSettings(shardSetConfig);

            return shardSetConfig;
        }

        #endregion
    }
}