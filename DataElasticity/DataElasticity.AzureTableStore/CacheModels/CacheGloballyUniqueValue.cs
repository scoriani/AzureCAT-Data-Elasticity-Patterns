// ***********************************************************************

#region usings

using System;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.GloballyUniqueValues;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.CacheModels
{
    /// <summary>
    /// Class CacheGloballyUniqueValue is the serializable object held in the object cache for global unique values.
    /// </summary>
    [Serializable]
    public class CacheGloballyUniqueValue
    {
        #region properties

        /// <summary>
        /// Gets or sets the name of the data set.
        /// </summary>
        /// <value>The name of the data set.</value>
        public string DataSetName { get; set; }

        /// <summary>
        /// Gets or sets the distribution key.
        /// </summary>
        /// <value>The distribution key.</value>
        public long DistributionKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty { get; set; }

        /// <summary>
        /// Gets or sets the unique value.
        /// </summary>
        /// <value>The unique value.</value>
        public string UniqueValue { get; set; }

        #endregion

        #region methods

        /// <summary>
        /// Converts to the Azure Table Storage model for the globally unique value.
        /// </summary>
        /// <returns>GloballyUniqueValue.</returns>
        public GloballyUniqueValue ToGloballyUniqueValue()
        {
            if (IsEmpty)
            {
                return new GloballyUniqueValue();
            }
            return new GloballyUniqueValue
            {
                DataSetName = DataSetName,
                UniqueValue = UniqueValue,
                DistributionKey = DistributionKey
            };
        }

        #endregion
    }
}