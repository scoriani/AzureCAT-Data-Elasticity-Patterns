using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.AzureCat.Patterns.CityHash;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.AzureCat.Patterns.DataElasticity.IntegrationTests
{
    /// <summary>
    /// Class AzureShardSetConnectionDriverIntegrationTests is used for integration testing the 
    /// AzureShardSetConnectionDriverIntegration driver.
    /// </summary>
    /// <remarks>
    /// Uses localdb or TestSQLServer setting in App.config
    /// Must create AdvWrkAWSales000001 test Azure database.
    /// Needs Unity configuration
    /// </remarks>
    [TestClass]
    public class AzureShardSetConnectionDriverIntegrationTests : ScaleOutManagerTestsBase
    {
        #region constants

        private const string _testCatalog = "AdvWrkAWSales000001";
        private const string _testPassword = "Blank123";
        private const string _testShardSetName = "AWSales";
        private const string _testUserName = "Batman";

        #endregion

        #region fields

        private static readonly string _testServerInstanceName =
            ConfigurationManager.AppSettings["TestSQLServer"] ?? @"(localdb)\v11.0";

        #endregion

        #region methods

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            SetUpRetryPolicy();
        }

        [TestMethod]
        public void ShouldGetIndividualShardlet()
        {
            //assemble
            var driver = new AzureShardSetConnectionDriver();

            var shardSetConfig = GetShardSetConfig();

            InsertTestShardlets(driver, 10);

            // act (should be in cache)
            const string shardingKey = "1";
            var distributionKey = GetDistributionKey(shardingKey);
            var shardlet = driver.GetShardletByShardingKey(shardSetConfig.ShardSetName, shardingKey, distributionKey);

            //assert
            Assert.IsNotNull(shardlet);
            Assert.AreEqual(shardingKey, shardlet.ShardingKey);
            Assert.AreEqual(distributionKey, shardlet.DistributionKey);

            // act (no caching)
            AzureCache.Clear();
            shardlet = driver.GetShardletByShardingKey(shardSetConfig.ShardSetName, shardingKey, distributionKey);

            //assert
            Assert.IsNotNull(shardlet);
            Assert.AreEqual(shardingKey, shardlet.ShardingKey);
            Assert.AreEqual(distributionKey, shardlet.DistributionKey);
        }

        [TestMethod]
        public void ShouldPublishAndGetOver1000Shardlets()
        {
            // Windows Azure Tables returns up to a maximum of 1000 entities in a single request and returns a continuation token 
            // when more results are available.  Make sure our implementation returns more than 1000 rows.

            //assemble
            var driver = new AzureShardSetConnectionDriver();

            var shardSetConfig = GetShardSetConfig();

            InsertTestShardlets(driver, 1200);

            // act
            var keys =
                driver
                    .GetShardlets(shardSetConfig.ShardSetName)
                    .Select(s => s.DistributionKey)
                    .OrderBy(k => k)
                    .ToArray();

            //assert
            Assert.AreEqual(1200, keys.Count());
            Assert.AreEqual(-9223304498103175194, keys.First());
            Assert.AreEqual(9218225541702210765, keys.Last());
        }


        [TestMethod]
        public void ShouldPublishAndRemoveOver100ShardletConnectionsByUtcDateTime()
        {
            //assemble
            var driver = new AzureShardSetConnectionDriver();

            var shardingKey = 1.ToString(CultureInfo.InvariantCulture);
            var shardlet = GetShardlet(shardingKey);

            // act
            const int numberOfConnections = 210;
            const short startSpid = 500;
            const short endSpid = 500 + numberOfConnections - 1;

            for (var i = startSpid; i <= endSpid; i++)
            {
                driver.PublishShardletConnection(shardlet, i);
            }

            var spids = driver.GetConnectedSpids(shardlet).OrderBy(spid => spid).ToArray();

            Assert.AreEqual(numberOfConnections, spids.Count());
            Assert.AreEqual(startSpid, spids.First());
            Assert.AreEqual(endSpid, spids.Last());

            Thread.Sleep(10000);

            driver.RemoveConnections(shardlet.ShardSetName, DateTime.UtcNow);

            spids = driver.GetConnectedSpids(shardlet).ToArray();

            Assert.AreEqual(0, spids.Count());
        }

        [TestMethod]
        public void ShouldPublishAndRemoveRangeShardDistributionKeys()
        {
            //assemble
            var driver = new AzureShardSetConnectionDriver();

            var shardSetConfig = GetShardSetConfig();

            const int numberOfRows = 20;
            InsertTestRangeShards(driver, shardSetConfig.ShardSetName, numberOfRows);

            // note - we are not putting in a valid shardmap here ... just testing the IO

            // act
            var rangeShard = driver.GetAzureRangeShard(shardSetConfig.ShardSetName, 0);

            //assert
            Assert.IsNotNull(rangeShard);

            // remove
            RemoveTestRangeShards(driver, shardSetConfig.ShardSetName, numberOfRows);

            //assert
            rangeShard = driver.GetAzureRangeShard(shardSetConfig.ShardSetName, 0);
            Assert.IsNull(rangeShard);
        }

        [TestMethod]
        public void ShouldPublishAndRemoveShardletConnection()
        {
            //assemble
            var driver = new AzureShardSetConnectionDriver();

            var shardingKey = 1.ToString(CultureInfo.InvariantCulture);
            var shardlet = GetShardlet(shardingKey);

            // act
            driver.PublishShardletConnection(shardlet, 1);
            driver.PublishShardletConnection(shardlet, 2);

            var spids = driver.GetConnectedSpids(shardlet).ToArray();

            Assert.AreEqual(2, spids.Count());
            Assert.AreEqual(1, spids.First());
            Assert.AreEqual(2, spids.Last());

            driver.RemoveShardletConnection(shardlet, 1);

            spids = driver.GetConnectedSpids(shardlet).ToArray();

            Assert.AreEqual(1, spids.Count());
            Assert.AreEqual(2, spids.First());
        }

        [TestMethod]
        public void ShouldPublishAndRemoveShardlets()
        {
            //assemble
            var driver = new AzureShardSetConnectionDriver();

            var shardSetConfig = GetShardSetConfig();

            const int numberOfRows = 20;
            InsertTestShardlets(driver, numberOfRows);

            // act
            var keys =
                driver
                    .GetShardlets(shardSetConfig.ShardSetName)
                    .Select(s => s.DistributionKey)
                    .OrderBy(k => k)
                    .ToArray();

            //assert
            Assert.AreEqual(numberOfRows, keys.Count());
            Assert.AreEqual(-9142586270102516767, keys.First());
            Assert.AreEqual(8943927636039079085, keys.Last());

            // remove
            RemoveTestShardlets(driver, numberOfRows);

            keys =
                driver
                    .GetShardlets(shardSetConfig.ShardSetName)
                    .Select(s => s.DistributionKey)
                    .OrderBy(k => k)
                    .ToArray();

            //assert
            Assert.AreEqual(0, keys.Count());
        }

        [TestMethod]
        public void ShouldPublishAndTerminate100ShardletConnectionsBySpid()
        {
            //assemble
            var driver = new AzureShardSetConnectionDriver();

            var shardingKey = 1.ToString(CultureInfo.InvariantCulture);
            var shardlet = GetShardlet(shardingKey);

            // act

            // publish connections - avoid connections under 500 for testing
            // don't want to KILL core processes in SQL Server
            const int numberOfConnections = 210;
            const short startSpid = 500;
            const short endSpid = 500 + numberOfConnections - 1;

            for (var i = startSpid; i <= endSpid; i++)
            {
                driver.PublishShardletConnection(shardlet, i);
            }

            var spids = driver.GetConnectedSpids(shardlet).OrderBy(spid => spid).ToArray();

            Assert.AreEqual(numberOfConnections, spids.Count());
            Assert.AreEqual(startSpid, spids.First());
            Assert.AreEqual(endSpid, spids.Last());

            driver.TerminateConnections(shardlet);

            spids = driver.GetConnectedSpids(shardlet).ToArray();

            Assert.AreEqual(0, spids.Count());
        }

        [TestInitialize]
        public void TestInitialize()
        {
            AzureShardSetConnectionDriver.InitializeAzureTables(_testShardSetName, true);
            AzureShardSetConnectionDriver.InitializeAzureTables();
            AzureCache.Clear();
        }

        private static long GetDistributionKey(string shardingKey)
        {
            return CityHasher.CityHash64StringGetLong(shardingKey);
        }

        private static long GetDistributionKey(int shardingKey)
        {
            return CityHasher.CityHash64StringGetLong(shardingKey.ToString(CultureInfo.InvariantCulture));
        }

        private static RangeShard GetRangeShard(int seed)
        {
            return new RangeShard
            {
                Catalog = _testCatalog,
                ServerInstanceName = _testServerInstanceName,
                HighDistributionKey = seed,
                LowDistributionKey = seed,
                ShardID = seed
            };
        }

        private static ShardSetConfig GetShardSetConfig()
        {
            return new ShardSetConfig
            {
                AllowDeployments = true,
                CurrentShardCount = 0,
                MaxShardCount = 5,
                MaxShardSizeMb = 100,
                MaxShardletsPerShard = 1000,
                MinShardSizeMb = 10,
                TargetShardCount = 5,
                ShardSetName = _testShardSetName
            };
        }

        private static Shardlet GetShardlet(string shardingKey)
        {
            return new Shardlet
            {
                Catalog = _testCatalog,
                ServerInstanceName = ConfigurationManager.AppSettings["TestSQLServer"] ?? @"(localdb)\v11.0",
                UserName = _testUserName,
                Password = _testPassword,
                ShardSetName = _testShardSetName,
                ShardingKey = shardingKey,
                Pinned = false,
                DistributionKey = GetDistributionKey(shardingKey),
                Status = ShardletStatus.Active
            };
        }

        private static void InsertTestRangeShards(AzureShardSetConnectionDriver driver, string shardSetName,
            int numberToInsert)
        {
            for (var i = 1; i <= numberToInsert; i++)
            {
                var rangeShard = GetRangeShard(i);
                driver.PublishShard(shardSetName, rangeShard);
            }
        }


        private static void InsertTestShardlets(AzureShardSetConnectionDriver driver, int numberToInsert)
        {
            for (var i = 1; i <= numberToInsert; i++)
            {
                var shardlet = GetShardlet(i.ToString(CultureInfo.InvariantCulture));
                driver.PublishShardlet(shardlet);
            }
        }

        private static void RemoveTestRangeShards(AzureShardSetConnectionDriver driver, string shardSetName,
            int numberToInsert)
        {
            for (var i = 1; i <= numberToInsert; i++)
            {
                var rangeShard = GetRangeShard(i);
                driver.RemoveShard(shardSetName, rangeShard);
            }
        }

        private static void RemoveTestShardlets(AzureShardSetConnectionDriver driver, int numberToInsert)
        {
            for (var i = 1; i <= numberToInsert; i++)
            {
                var shardlet = GetShardlet(i.ToString(CultureInfo.InvariantCulture));
                driver.RemoveShardlet(shardlet);
            }
        }

        #endregion
    }
}