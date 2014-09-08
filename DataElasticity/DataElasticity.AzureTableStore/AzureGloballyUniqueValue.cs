#region usings

using System;
using System.Configuration;
using System.Linq;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.CacheModels;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.GloballyUniqueValues;
using Microsoft.WindowsAzure.Storage;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore
{
    /// <summary>
    /// Class AzureGloballyUniqueValue.
    /// Use this class to manage the tokens that must be globally unique and need to be
    /// associated to a Shardlet.  These values rely on a lookup mechanism to return
    /// the DistributionKey.
    /// </summary>
    public class AzureGloballyUniqueValue
    {
        //Constructor retrieves the azure storage account details, and creates a context

        #region fields

        /// <summary>
        /// The _ service context
        /// </summary>
        private readonly GloballyUniqueValueContext _serviceContext;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureGloballyUniqueValue"/> class.
        /// </summary>
        /// <exception cref="System.Exception">Connection string to azure storage is required in your app.config</exception>
        public AzureGloballyUniqueValue()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["AzureStorage"];
            if (connectionString == null)
            {
                throw new Exception("Connection string to azure storage is required in your app.config");
            }
            var storageAccount = CloudStorageAccount.Parse(connectionString.ConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var globallyUniqueTable = tableClient.GetTableReference(GloballyUniqueValueContext.GloballyUniqueValueTable);
            globallyUniqueTable.CreateIfNotExists();
            _serviceContext = new GloballyUniqueValueContext(tableClient);

        }

        #endregion

        #region methods

        /// <summary>
        /// Adds the globally unique value.
        /// </summary>
        /// <param name="dataSetName">Name of the data set.</param>
        /// <param name="uniqueValue">The unique value.</param>
        /// <param name="distributionKey">The distribution key.</param>
        /// <exception cref="System.InvalidOperationException">The provided unique value already exist</exception>
        public void AddGloballyUniqueValue(string dataSetName, string uniqueValue, long distributionKey)
        {
            var globallyUniqueValue = GetGloballyUniqueValue(dataSetName, uniqueValue, true);
            if (globallyUniqueValue == null)
            {
                globallyUniqueValue = new GloballyUniqueValue
                {
                    DataSetName = dataSetName,
                    UniqueValue = uniqueValue,
                    DistributionKey = distributionKey
                };
                _serviceContext.AddObject(GloballyUniqueValueContext.GloballyUniqueValueTable, globallyUniqueValue);
            }
            else
            {
                throw new InvalidOperationException("The provided unique value already exist");
            }

            _serviceContext.SaveChanges();

            AzureCache.Put(dataSetName + "-" + uniqueValue, globallyUniqueValue);
        }

        /// <summary>
        /// Deletes the globally unique value.
        /// </summary>
        /// <param name="dataSetName">Name of the data set.</param>
        /// <param name="uniqueValue">The unique value.</param>
        public void DeleteGloballyUniqueValue(string dataSetName, string uniqueValue)
        {
            var itemToDelete = GetGloballyUniqueValue(dataSetName, uniqueValue);
            if (itemToDelete != null)
            {
                _serviceContext.DeleteObject(itemToDelete);
                _serviceContext.SaveChanges();
            }
            AzureCache.Remove(dataSetName + "-" + uniqueValue);
        }

        /// <summary>
        /// Gets the globally unique value.
        /// </summary>
        /// <param name="dataSetName">Name of the data set.</param>
        /// <param name="uniqueValue">The unique value.</param>
        /// <param name="skipCache">if set to <c>true</c> [skip cache].</param>
        /// <returns>GloballyUniqueValue.</returns>
        public GloballyUniqueValue GetGloballyUniqueValue(string dataSetName, string uniqueValue, bool skipCache = false)
        {
            if (!skipCache)
            {
                var cachedUniqueValue = AzureCache.Get(dataSetName + "-" + uniqueValue) as CacheGloballyUniqueValue;
                if (cachedUniqueValue == null)
                {
                    var returnUniqueValue =
                        _serviceContext.uniqueValues
                            .Where(c => c.PartitionKey == dataSetName &&
                                        c.RowKey == uniqueValue)
                            .Select(c => c)
                            .FirstOrDefault();

                    if (returnUniqueValue != null)
                    {
                        cachedUniqueValue = returnUniqueValue.ToCacheGloballyUniqueValue();
                        AzureCache.Put(dataSetName + "-" + uniqueValue, cachedUniqueValue);
                        return returnUniqueValue;
                    }
                    AzureCache.Put(dataSetName + "-" + uniqueValue, new CacheGloballyUniqueValue { IsEmpty = true });
                    return null;
                }
                if (cachedUniqueValue.IsEmpty)
                {
                    return null;
                }
                return cachedUniqueValue.ToGloballyUniqueValue();
            }

            return
                _serviceContext.uniqueValues
                    .Where(c => c.PartitionKey == dataSetName &&
                                c.RowKey == uniqueValue)
                    .Select(c => c)
                    .FirstOrDefault();
        }

        //Update
        /// <summary>
        /// Updates the globally unique value.
        /// </summary>
        /// <param name="dataSetName">Name of the data set.</param>
        /// <param name="uniqueValue">The unique value.</param>
        /// <param name="distributionKey">The distribution key.</param>
        /// <exception cref="System.InvalidOperationException">The provided unique value doesn't exist</exception>
        public void UpdateGloballyUniqueValue(string dataSetName, string uniqueValue, long distributionKey)
        {
            var globallyUniqueValue = GetGloballyUniqueValue(dataSetName, uniqueValue, true);
            if (globallyUniqueValue != null)
            {
                globallyUniqueValue.DataSetName = dataSetName;
                globallyUniqueValue.UniqueValue = uniqueValue;
                globallyUniqueValue.DistributionKey = distributionKey;
                _serviceContext.UpdateObject(globallyUniqueValue);

                _serviceContext.SaveChanges();
            }
            else
            {
                throw new InvalidOperationException("The provided unique value doesn't exist");
            }

            AzureCache.Remove(dataSetName + "-" + uniqueValue);
            AzureCache.Put(dataSetName + "-" + uniqueValue, globallyUniqueValue);
        }

        #endregion
    }
}