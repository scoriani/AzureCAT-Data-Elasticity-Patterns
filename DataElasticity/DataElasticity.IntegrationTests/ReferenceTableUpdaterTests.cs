#region usings

using System.Configuration;
using Microsoft.AzureCat.Patterns.DataElasticity.Contrib;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.IntegrationTests
{
    /// <summary>
    /// Class ReferenceTableUpdaterTests tests updating reference data via the ReferenceTableUpdater project.
    /// It requires AWMain and AWSales from the projects of the same name.  These projects are 
    /// in the AdventureWorks demo project in the Database solution.
    /// 
    /// </summary>
    [TestClass]
    public class ReferenceTableUpdaterTests : ScaleOutManagerTestsBase
    {
        #region methods

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            SetUpRetryPolicy();
        }

        [TestMethod]
        public void ShouldCreateAllTest()
        {
            // note this test only runs on an empty target database
            var shard = CreateShard();
            var settings = CreateSettings();

            var connectionString = GetReferenceConnectionString();

            var updater = new ReferenceTableUpdater(connectionString, shard, settings, "AWSales");

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

        [TestMethod]
        public void ShouldSyncAllTest()
        {
            var shard = CreateShard();
            var settings = CreateSettings();

            var connectionString = GetReferenceConnectionString();

            var updater = new ReferenceTableUpdater(connectionString, shard, settings, "AWSales");

            // make sure is syncs twice
            SyncAllData(updater);
            SyncAllData(updater);
        }

        private static void SyncAllData(ReferenceTableUpdater updater)
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

        private static Settings CreateSettings()
        {
            return new Settings
            {
                AdminUser = "Superman",
                AdminPassword = "Blank123",
            };
        }

        private static RangeShard CreateShard()
        {
            return new RangeShard
            {
                Catalog = "AWSales",
                ServerInstanceName = ConfigurationManager.AppSettings["TestSQLServer"] ?? @"(localdb)\v11.0",
            };
        }

        #endregion
    }
}