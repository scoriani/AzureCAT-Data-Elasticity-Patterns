#region usings

using System;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Demo.AdventureWorks.Test
{
    /// <summary>
    /// A set of Data Elasticity demos that assume web role(s) are running and processing queues.
    /// </summary>
    [TestClass]
    public class DataElasticityDemo : ScaleOutManagerTestsBase
    {
        #region Demo

        /// <summary>
        /// Clear the cache.
        /// </summary>
        [TestMethod]
        public void T0_ClearCache()
        {
            AzureCache.Clear();
        }

        [TestMethod]
        public void T1_Create_Initial_Sharded_Environment()
        {
            // using the project, set up the AwMain
            // AwMain
            //      Golden Reference Data
            //      SalesOrderHeader ID Generation
            //      Data Elasticity Configuration

            // set up the global settings using the ISettingsRepository interface
            //  ... implementation uses AwMain database in  SQL Azure 
            SaveSettings();

            // set up the test server in configuration
            var server = ConfigureServer();

            // set up the shard set configuration for AWSales shard set using IConfigRepository interface
            //  ... implementation uses AwMain database SQL Azure 
            var shardSetConfig = ConfigureShardSet(server);

            // range shard deployment
            shardSetConfig.DeployShardMap(_queueAndUseWorkerRoles);

            // pointer shard deployment
            shardSetConfig.DeployPointerShards(_queueAndUseWorkerRoles);

            // update online shard map
            shardSetConfig.PublishShardMap(_queueAndUseWorkerRoles);
        }


        [TestMethod]
        public void T2_Update_Reference_Data_And_Propagate()
        {
            // Scenario - nightly job updates the CurrencyRate table in AwMain
            //      Run data update
            //      Queue a Shard Sync for Range Shards
            //      Queue a Shard Sync for Pointer Shards

            // update the CurrencyRate table in AwMain
            LoadCurrencyRatesInAwMain(500);

            // Get the shard set configuration
            var shardSetConfig = ShardSetConfig.LoadCurrent(TestShardSetName);

            // range shard synchronization
            shardSetConfig.SyncShards(_queueAndUseWorkerRoles);

            // pointer shard synchronization
            shardSetConfig.SyncPointerShards(_queueAndUseWorkerRoles);
        }

        [TestMethod]
        public void T3_Add_Data_To_Shards()
        {
            // create some test data in the sharded database
            var connectionString = GetReferenceConnectionString();
            var builder = new TestDataBuilder(connectionString);

            const int initialTestCustomerID = 1;
            const int numberOfTestCustomers = 100;
            const int numberOfTestOrdersPerCustomer = 5;

            builder.AddTestDataInShardSet(TestShardSetName, initialTestCustomerID, numberOfTestCustomers, numberOfTestOrdersPerCustomer);
        }

        [TestMethod]
        public void T4_Pin_Data_To_Shard()
        {
            var shardSetConfig = ShardSetConfig.LoadCurrent(TestShardSetName);
            var pointerShard = shardSetConfig.Shards.First();

            var shardlet = Shardlet.Load(TestShardSetName, "1");

            shardlet.MoveToShard(pointerShard, true, true, new Guid(), _queueAndUseWorkerRoles);
        }

        [TestMethod]
        public void T5_Scale_Out()
        {
            // read the test shard set configuration 
            var shardSetConfig = ShardSetConfig.LoadCurrent(TestShardSetName);

            // create a new configuration with more shards
            shardSetConfig.TargetShardCount = 8;
            shardSetConfig.MaxShardCount = 8;

            // recalculate the shard map and save a new configuration
            shardSetConfig.UpdateShardMap();
            shardSetConfig.Save();

            // shard deployment (only works in queues)
            shardSetConfig.DeployShardMap(true);

            // update of online shard map (only works in queues)
            shardSetConfig.PublishShardMap(true);
        }

        [TestMethod]
        public void T6_Scale_In()
        {
            // read the test shard set configuration 
            var shardSetConfig = ShardSetConfig.LoadCurrent(TestShardSetName);

            // create a new configuration with more shards
            shardSetConfig.TargetShardCount = 3;
            shardSetConfig.MaxShardCount = 3;

            // recalculate the shard map and save a new configuration
            shardSetConfig.UpdateShardMap();
            shardSetConfig.Save();

            // shard deployment
            shardSetConfig.DeployShardMap(true);

            // update of online shard map
            shardSetConfig.PublishShardMap(true);
        }

        #endregion

        #region constants

        private const string _inputFilePath = @"C:\CurrencyRates.txt";
        private const bool _queueAndUseWorkerRoles = true;

        #endregion

        #region methods

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            SetUpRetryPolicy();
        }

        protected static Settings SaveSettings()
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

        private static Server ConfigureServer()
        {
            var serverLocation = ConfigurationManager.AppSettings["TestSQLServer"] ?? @"localhost";

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

        private static ShardSetConfig ConfigureShardSet(Server server)
        {
            var shardSetConfig =
                new ShardSetConfig
                {
                    AllowDeployments = true,
                    CurrentShardCount = 0,
                    MaxShardCount = -1,
                    MaxShardSizeMb = 100,
                    MaxShardletsPerShard = 1000,
                    MinShardSizeMb = 10,
                    TargetShardCount = 5,
                    ShardSetName = "AWSales"
                };

            shardSetConfig.Servers.Add(server);
            shardSetConfig.UpdateShardMap();

            // add pointer shard
            var serverLocation = ConfigurationManager.AppSettings["TestSQLServer"] ?? @"(localdb)\v11.0";

            var pointerShard =
                new Shard
                {
                    Catalog = "AdvWrkAWSales_HighVolume",
                    Description = "Database for premium speed orders",
                    ServerInstanceName = serverLocation
                };

            shardSetConfig.Shards.Add(pointerShard);

            // add dacpac settings
            shardSetConfig.SetShardSetSetting("DacPacBlobName", @"AWSales.dacpac");
            shardSetConfig.SetShardSetSetting("DacPacProfileBlobName", @"AWSales.Deploy.azuredb.publish.xml");
            shardSetConfig.SetShardSetSetting("DacPacSyncProfileBlobName", @"AWSales.Sync.azuredb.publish.xml");
            shardSetConfig.SetShardSetSetting("DacPacShouldDeployOnSync", _queueAndUseWorkerRoles.ToString());

            try
            {
                shardSetConfig.Save();
            }
            catch (DbEntityValidationException e)
            {
                // todo: log
                var errors = e.EntityValidationErrors;
                throw;
            }


            return shardSetConfig;
        }

        private void LoadCurrencyRatesInAwMain(int maxNumberOfRows)
        {
            var connectionString = GetReferenceConnectionString();

            using (var connection = new ReliableSqlConnection(connectionString))
            {
                connection.Open();

                var command = new SqlCommand("[Sales].[InsertCurrencyRate]", connection.Current) { CommandType = CommandType.StoredProcedure };
                var currencyRateDateParam = new SqlParameter("CurrencyRateDate", SqlDbType.DateTime);
                var fromCurrencyCodeParam = new SqlParameter("FromCurrencyCode", SqlDbType.Char, 3);
                var toCurrencyCodeParam = new SqlParameter("ToCurrencyCode", SqlDbType.Char, 3);
                var averageRateParam = new SqlParameter("AverageRate", SqlDbType.Money);
                var endOfDayRateParam = new SqlParameter("EndOfDayRate", SqlDbType.Money);

                command.Parameters.Add(currencyRateDateParam);
                command.Parameters.Add(fromCurrencyCodeParam);
                command.Parameters.Add(toCurrencyCodeParam);
                command.Parameters.Add(averageRateParam);
                command.Parameters.Add(endOfDayRateParam);

                var headerSkipped = false;
                var lineNumber = 0;
                using (var streamReader = new StreamReader(_inputFilePath))
                {
                    var line = streamReader.ReadLine();

                    while (line != null && lineNumber++ <= maxNumberOfRows)
                    {
                        if (!headerSkipped)
                        {
                            headerSkipped = true;
                            line = streamReader.ReadLine();
                            continue;
                        }

                        var dataArray = line.Split('|');

                        currencyRateDateParam.Value = DateTime.Parse(dataArray[1]);
                        fromCurrencyCodeParam.Value = dataArray[2];
                        toCurrencyCodeParam.Value = dataArray[3];
                        averageRateParam.Value = Decimal.Parse(dataArray[4]);
                        endOfDayRateParam.Value = Decimal.Parse(dataArray[5]);

                        command.ExecuteNonQuery();

                        line = streamReader.ReadLine();
                    }
                }
            }
        }

        #endregion
    }
}