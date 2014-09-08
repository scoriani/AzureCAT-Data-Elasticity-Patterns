#region usings

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Microsoft.AzureCat.Patterns.CityHash;
using Microsoft.AzureCat.Patterns.DataElasticity.Interfaces;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;
using Microsoft.AzureCat.Patterns.DataElasticity.Contrib;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Demo.AdventureWorks
{
    internal class AwSalesShardSetDriver : IShardSetDriver
    {
        #region constants

        private const string _dacPacBlobNameKey = "DacPacBlobName";
        private const string _dacPacProfileBlobNameKey = "DacPacProfileBlobName";
        private const string _dacPacShouldDeployOnSyncKey = "DacPacShouldDeployOnSync";
        private const string _dacPacSyncProfileBlobNameKey = "DacPacSyncProfileBlobName";

        private const string _deleteShardletProcedure = "[Shardlets].[DeleteShardlet]";

        private const string _referenceDataConnectionStringSetting = "AwMainReferenceData";

        #endregion

        #region fields

        private readonly Settings _settings;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AwSalesShardSetDriver"/> class.
        /// </summary>
        public AwSalesShardSetDriver()
        {
            _settings = Settings.Load();
        }

        #endregion

        #region IShardSetDriver

        /// <summary>
        /// Call to the shard set driver when a shardlet is copied from one shard to another.
        /// </summary>
        /// <param name="sourceShard">The source shard.</param>
        /// <param name="destinationShard">The destination shard.</param>
        /// <param name="shardSetConfig">The shard set configuration.</param>
        /// <param name="shardingKey">The distribution key.</param>
        /// <param name="uniqueProcessID">The unique process identifier.</param>
        public void CopyShardlet(ShardBase sourceShard, ShardBase destinationShard, ShardSetConfig shardSetConfig,
            string shardingKey, Guid uniqueProcessID)
        {
            //Setup the connections for the source and destination shard
            var sourceConnection = GetShardConnection(sourceShard, shardSetConfig);
            var destinationConnection = GetShardConnection(destinationShard, shardSetConfig);
            var uniqueProcessString = uniqueProcessID.ToString().Trim().Replace("-", string.Empty);

            // copy the shardlet
            ResetTempTables(uniqueProcessString, destinationConnection);
            CopyShardletIntoTempTables(shardingKey, sourceConnection, destinationConnection, uniqueProcessString);
            MergeShardletAndDropTempTables(destinationConnection, uniqueProcessString);
        }

        /// <summary>
        /// Call to the shard set driver when a shard is to be first created..
        /// </summary>
        /// <param name="shard">The shard.</param>
        /// <param name="shardSetConfig">The shard set configuration.</param>
        public void CreateShard(ShardBase shard, ShardSetConfig shardSetConfig)
        {
            // Retrieve storage account from connection string.
            var connectionString = ConfigurationManager.ConnectionStrings["AzureStorage"];

            if (connectionString == null)
            {
                throw new InvalidOperationException("Connection string to azure storage is required in your app.config");
            }

            var dacPacBlobName = shardSetConfig.GetShardSetSetting(_dacPacBlobNameKey);
            var dacPacDeployProfileBlobName = shardSetConfig.GetShardSetSetting(_dacPacProfileBlobNameKey);

            var dacPacsPath = DownloadDacpacsFromBlobStore(connectionString, shardSetConfig.ShardSetName);

            var isNewDb = PublishDacPac(shard, shardSetConfig, dacPacsPath + @"\" + dacPacBlobName,
                dacPacsPath + @"\" + dacPacDeployProfileBlobName);

            UpdateReferenceData(shard, isNewDb);
        }

        /// <summary>
        /// Call to the shard set driver when a shard is to be deleted.
        /// </summary>
        /// <param name="shard">The shard.</param>
        /// <param name="shardSetConfig">The shard set configuration.</param>
        public void DeleteShard(ShardBase shard, ShardSetConfig shardSetConfig)
        {
            // todo: make sure this check occurs in the caller
            //if (!ignorePopulation)
            //{
            //    // todo: inefficient to return the entire list to see if any exist
            //    var shardDistributionKeys = GetShardDistributionKeys(shard, shardSetConfig);

            //    if (shardDistributionKeys.Any()) return;
            //}

            // todo: why is the delete code on the publisher?
            var publisher = InstantiateDacPacPublisher(shard, shardSetConfig);

            publisher.DropDatabase();
        }

        /// <summary>
        /// Call to the shard set driver when a specific shardlet is to be deleted from the shardlet.
        /// </summary>
        /// <param name="shard">The shard.</param>
        /// <param name="shardSetConfig">The shard set configuration.</param>
        /// <param name="shardingKey">The distribution key.</param>
        public void DeleteShardlet(ShardBase shard, ShardSetConfig shardSetConfig, string shardingKey)
        {
            var shardConnection = GetShardConnection(shard, shardSetConfig);

            using (var connection = new ReliableSqlConnection(shardConnection.ConnectionString))
            {
                connection.Open();
                var command = new SqlCommand(_deleteShardletProcedure, connection.Current)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add(CreateShardingKeyParameter(shardingKey));

                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Gets the distribution key for sharding key for the DHard Set.
        /// </summary>
        /// <param name="shardingKey">The sharding key.</param>
        /// <returns>System.Int64.</returns>
        public long GetDistributionKeyForShardingKey(string shardingKey)
        {
            return (long)CityHasher.CityHash64String(shardingKey);
        }

        public event TableConfigPublishingHandler ShardSetConfigPublishing;

        /// <summary>
        /// Call to the shard set driver when an existing shard is being synchronized.
        /// </summary>
        /// <param name="shard">The shard.</param>
        /// <param name="shardSetConfig">The shard set configuration.</param>
        public void SyncShard(ShardBase shard, ShardSetConfig shardSetConfig)
        {
            // get the settings for dac pac publishing from the shard set setting
            // use the sync settings            

            // Retrieve storage account from connection string.
            var connectionString = ConfigurationManager.ConnectionStrings["AzureStorage"];

            if (connectionString == null)
            {
                throw new InvalidOperationException("Connection string to azure storage is required in your app.config");
            }

            var dacPacBlobName = shardSetConfig.GetShardSetSetting(_dacPacBlobNameKey);
            var dacPacSyncProfileBlobName = shardSetConfig.GetShardSetSetting(_dacPacSyncProfileBlobNameKey);
            var dacPacShouldDeployOnSync = shardSetConfig.GetShardSetSetting(_dacPacShouldDeployOnSyncKey);

            var dacPacsPath = DownloadDacpacsFromBlobStore(connectionString, shardSetConfig.ShardSetName);

            var isNewDb = dacPacShouldDeployOnSync.ToLower() == "true" &&
                          PublishDacPac(shard, shardSetConfig, dacPacsPath + @"\" + dacPacBlobName,
                              dacPacsPath + @"\" + dacPacSyncProfileBlobName);

            UpdateReferenceData(shard, isNewDb);
        }

        #endregion

        #region methods

        private static SqlCommand CopySalesOrderDetailsIntoTempTable(string shardingKey, string uniqueProcessString,
            ReliableSqlConnection sourceSqlConnection, BulkCopier copier)
        {
            // Copy sales order detail from source to destination
            var detailReaderCommand =
                new SqlCommand("[Shardlets].[GetSalesOrderDetailsByCustomerId]", sourceSqlConnection.Current)
                {
                    CommandType = CommandType.StoredProcedure
                };

            detailReaderCommand.Parameters.Add(CreateShardingKeyParameter(shardingKey));
            var detailReader = detailReaderCommand.ExecuteReader();

            var detailsTargetTableName = GetTempTableName(uniqueProcessString, "[Shardlets].[SalesOrderDetail_");

            copier.Copy(detailReader, detailsTargetTableName);

            detailReader.Close();

            return detailReaderCommand;
        }

        private static void CopySalesOrderHeadersIntoTempTable(string shardingKey, string uniqueProcessString,
            ReliableSqlConnection sourceSqlConnection, BulkCopier copier)
        {
            // Copy sales order header from source to destination
            var headerReaderCommand =
                new SqlCommand("[Shardlets].[GetSalesOrderHeadersByCustomerId]", sourceSqlConnection.Current)
                {
                    CommandType = CommandType.StoredProcedure
                };

            headerReaderCommand.Parameters.Add(CreateShardingKeyParameter(shardingKey));
            var headerReader = headerReaderCommand.ExecuteReader();

            var headerTargetTableName = GetTempTableName(uniqueProcessString, "[Shardlets].[SalesOrderHeader_");

            copier.Copy(headerReader, headerTargetTableName);

            headerReader.Close();
        }

        private static void CopyShardletIntoTempTables(string shardingKey, ShardConnection sourceConnection,
            ShardConnection destinationConnection, string uniqueProcessString)
        {
            // copy the shardlet data to the temporary tables
            var copier = new BulkCopier(sourceConnection.ConnectionString, destinationConnection.ConnectionString);

            using (var sourceSqlConnection = new ReliableSqlConnection(sourceConnection.ConnectionString))
            {
                sourceSqlConnection.Open();

                CopySalesOrderHeadersIntoTempTable(shardingKey, uniqueProcessString, sourceSqlConnection, copier);
                CopySalesOrderDetailsIntoTempTable(shardingKey, uniqueProcessString, sourceSqlConnection, copier);
                CopyShoppingCartItemsToTempTable(shardingKey, uniqueProcessString, sourceSqlConnection, copier);
            }
        }

        private static void CopyShoppingCartItemsToTempTable(string shardingKey, string uniqueProcessString,
            ReliableSqlConnection sourceSqlConnection, BulkCopier copier)
        {
            // Copy shopping cart items from source to destination
            var shoppingCartItemReaderCommand =
                new SqlCommand("[Shardlets].[GetShoppingCartItemsByCustomerId]", sourceSqlConnection.Current)
                {
                    CommandType = CommandType.StoredProcedure
                };

            shoppingCartItemReaderCommand.Parameters.Add(CreateShardingKeyParameter(shardingKey));
            var shoppingCartItemReader = shoppingCartItemReaderCommand.ExecuteReader();

            var shoppingCartItemTableName = GetTempTableName(uniqueProcessString, "[Shardlets].[ShoppingCartItem_");

            copier.Copy(shoppingCartItemReader, shoppingCartItemTableName);

            shoppingCartItemReader.Close();
        }

        private static SqlParameter CreateShardingKeyParameter(string shardingKey,
            string parameterName = "customerID")
        {
            int customerId;
            var isValueInt = int.TryParse(shardingKey, out customerId);

            if (!isValueInt)
            {
                throw new InvalidOperationException(string.Format("The sharding key presented to the AwSalesShardSetDriver is not a valid integer: {0}", shardingKey));
            }

            return new SqlParameter(parameterName, SqlDbType.Int) { Value = shardingKey };
        }

        private static SqlParameter CreateUniqueProcessIDParameter(string uniqueProcessString)
        {
            return new SqlParameter("uniqueProcessID", SqlDbType.Char) { Value = uniqueProcessString };
        }

        private string DownloadDacpacsFromBlobStore(ConnectionStringSettings connectionString, string shardSetName)
        {
            string dacPacsPath;
            if (RoleEnvironment.IsAvailable)
            {
                var dacPacs = RoleEnvironment.GetLocalResource("dacpacs");
                dacPacsPath = dacPacs.RootPath;
            }
            else
            {
                dacPacsPath = Path.GetTempPath();
            }

            dacPacsPath += shardSetName;
            Directory.CreateDirectory(dacPacsPath);

            var container = GetDacPacCloudBlobContainer(connectionString);

            var blobs = container.ListBlobs(null, true);
            foreach (var listBlobItem in blobs)
            {
                var item = (ICloudBlob)listBlobItem;
                var blobName = item.Name;
                blobName = blobName.Substring(blobName.IndexOf('/') + 1);

                var filename = dacPacsPath + @"\" + blobName;
                item.DownloadToFile(filename, FileMode.Create);

                if (!File.Exists(filename))
                {
                    throw new FileNotFoundException("File not found.", filename);
                }
            }

            return dacPacsPath;
        }

        private static CloudBlobContainer GetDacPacCloudBlobContainer(ConnectionStringSettings connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString.ConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("dacpacs");

            return container;
        }

        private static string GetReferenceDataConnectionString()
        {
            var configurationSetting = ConfigurationManager.ConnectionStrings[_referenceDataConnectionStringSetting];

            return configurationSetting.ConnectionString;
        }

        private ReferenceTableUpdater GetReferenceTableUpdater(ShardBase shard)
        {
            return new ReferenceTableUpdater(GetReferenceDataConnectionString(), shard, _settings, "AWSales");
        }

        private ShardConnection GetShardConnection(ShardBase shard, ShardSetConfig shardSetConfig)
        {
            return new ShardConnection
            {
                ServerInstanceName = shard.ServerInstanceName,
                Catalog = shard.Catalog,
                UserName = _settings.AdminUser,
                Password = _settings.AdminPassword,
                ShardSetName = shardSetConfig.ShardSetName
            };
        }

        private static string GetTempTableName(String uniqueProcessString, string tableNamePrefix)
        {
            // this target name style is also implemented in the database
            // in function Shardlets.GetShardletTempTableSuffix

            return string.Format("{0}{1}]", tableNamePrefix, uniqueProcessString);
            //return
            //    shardingKey < 0
            //        ? string.Format("{0}M{1}]", tableNamePrefix, Math.Abs(shardingKey))
            //        : string.Format("{0}{1}]", tableNamePrefix, shardingKey);
        }

        private DacPacPublisher InstantiateDacPacPublisher(ShardBase shard, ShardSetConfig shardSetConfig)
        {
            var parameters =
                new DacPacPublisher.DacPacPublisherParams(
                    shard,
                    shardSetConfig,
                    _settings.ShardUser,
                    _settings.ShardPassword,
                    _settings.AdminUser,
                    _settings.AdminPassword);

            return new DacPacPublisher(parameters);
        }

        private static void MergeSalesOrderDetails(string uniqueProcessString,
            ReliableSqlConnection destinationSqlConnection)
        {
            var detailSyncCommand =
                new SqlCommand("[Shardlets].[SyncShardletSalesOrderDetails]", destinationSqlConnection.Current)
                {
                    CommandType = CommandType.StoredProcedure
                };

            detailSyncCommand.Parameters.Add(CreateUniqueProcessIDParameter(uniqueProcessString));
            detailSyncCommand.ExecuteNonQuery();
        }

        private static void MergeSalesOrderHeaders(string uniqueProcessString,
            ReliableSqlConnection destinationSqlConnection)
        {
            var headerSyncCommand =
                new SqlCommand("[Shardlets].[SyncShardletSalesOrderHeaders]", destinationSqlConnection.Current)
                {
                    CommandType = CommandType.StoredProcedure
                };

            headerSyncCommand.Parameters.Add(CreateUniqueProcessIDParameter(uniqueProcessString));

            headerSyncCommand.ExecuteNonQuery();
        }

        private static void MergeShardletAndDropTempTables(ShardConnection destinationConnection,
            string uniqueProcessString)
        {
            using (var destinationSqlConnection = new ReliableSqlConnection(destinationConnection.ConnectionString))
            {
                // sync the header, detail and map data, drop the temp tables
                destinationSqlConnection.Open();

                // todo: need to check rows affected and return error if none (same on all updates in this method)

                MergeSalesOrderHeaders(uniqueProcessString, destinationSqlConnection);
                MergeSalesOrderDetails(uniqueProcessString, destinationSqlConnection);
                MergeShoppingCartItems(uniqueProcessString, destinationSqlConnection);

                // drop the temporary table data
                var dropTempTablesCommand =
                    new SqlCommand("[Shardlets].[DropShardletSalesOrder]", destinationSqlConnection.Current)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                dropTempTablesCommand.Parameters.Add(CreateUniqueProcessIDParameter(uniqueProcessString));
                dropTempTablesCommand.ExecuteNonQuery();
            }
        }

        private static void MergeShoppingCartItems(string uniqueProcessString,
            ReliableSqlConnection destinationSqlConnection)
        {
            var syncCommand =
                new SqlCommand("[Shardlets].[SyncShardletShoppingCartItems]", destinationSqlConnection.Current)
                {
                    CommandType = CommandType.StoredProcedure
                };

            syncCommand.Parameters.Add(CreateUniqueProcessIDParameter(uniqueProcessString));

            syncCommand.ExecuteNonQuery();
        }

        private bool PublishDacPac(ShardBase shard, ShardSetConfig shardSetConfig, string dacPacPath,
            string dacPacProfilePath)
        {
            var publisher = InstantiateDacPacPublisher(shard, shardSetConfig);

            return publisher.PublishDacPac(dacPacPath, dacPacProfilePath);
        }

        private static void ResetTempTables(string uniqueProcessString, ShardConnection destinationConnection)
        {
            // reset the temporary tables in the target for the current process
            using (var destinationSqlConnection = new ReliableSqlConnection(destinationConnection.ConnectionString))
            {
                destinationSqlConnection.Open();

                var command =
                    new SqlCommand("[Shardlets].[ResetShardletCustomer]", destinationSqlConnection.Current)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                command.Parameters.Add(CreateUniqueProcessIDParameter(uniqueProcessString));

                command.ExecuteNonQuery();
            }
        }

        private void UpdateReferenceData(ShardBase shard, bool isNewDb)
        {
            var updater = GetReferenceTableUpdater(shard);

            if (isNewDb)
            {
                updater.CreateData("[Person].[CountryRegion]");
                updater.CreateData("[Sales].[SalesTerritory]");
                updater.CreateData("[Person].[StateProvince]");
                updater.CreateData("[Sales].[Currency]");
                updater.CreateData("[Sales].[CurrencyRate]");
                updater.CreateData("[Sales].[SalesTaxRate]");

                updater.CreateData("[Production].[UnitMeasure]");
                updater.CreateData("[Production].[ProductCategory]");
                updater.CreateData("[Production].[ProductModel]");
                updater.CreateData("[Production].[ProductSubcategory]");
                updater.CreateData("[Production].[Product]");
            }
            else
            {
                updater.SyncData("[Person].[CountryRegion]", "[Deployment].[CountryRegionTemp]",
                    "[Deployment].[SyncCountryRegion]");
                updater.SyncData("[Sales].[SalesTerritory]", "[Deployment].[SalesTerritoryTemp]",
                    "[Deployment].[SyncSalesTerritory]");
                updater.SyncData("[Person].[StateProvince]", "[Deployment].[StateProvinceTemp]",
                    "[Deployment].[SyncStateProvince]");
                updater.SyncData("[Sales].[Currency]", "[Deployment].[CurrencyTemp]", "[Deployment].[SyncCurrency]");
                updater.SyncData("[Sales].[CurrencyRate]", "[Deployment].[CurrencyRateTemp]",
                    "[Deployment].[SyncCurrencyRate]");
                updater.SyncData("[Sales].[SalesTaxRate]", "[Deployment].[SalesTaxRateTemp]",
                    "[Deployment].[SyncSalesTaxRate]");

                updater.SyncData("[Production].[UnitMeasure]", "[Deployment].[UnitMeasureTemp]",
                    "[Deployment].[SyncUnitMeasure]");
                updater.SyncData("[Production].[ProductCategory]", "[Deployment].[ProductCategoryTemp]",
                    "[Deployment].[SyncProductCategory]");
                updater.SyncData("[Production].[ProductModel]", "[Deployment].[ProductModelTemp]",
                    "[Deployment].[SyncProductModel]");
                updater.SyncData("[Production].[ProductSubcategory]", "[Deployment].[ProductSubcategoryTemp]",
                    "[Deployment].[SyncProductSubcategory]");
                updater.SyncData("[Production].[Product]", "[Deployment].[ProductTemp]",
                    "[Deployment].[SyncProduct]");
            }
        }

        #endregion
    }
}