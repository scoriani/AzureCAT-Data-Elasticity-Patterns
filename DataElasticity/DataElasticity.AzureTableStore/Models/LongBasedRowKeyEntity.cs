#region usings

using Microsoft.WindowsAzure.Storage.Table;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models
{
    /// <summary>
    /// Class LongBasedRowKeyEntity is the common base class for Talbe Entities using the
    /// long value as a row key.
    /// </summary>
    public abstract class LongBasedRowKeyEntity : TableEntity
    {
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.WindowsAzure.Storage.Table.TableEntity" /> class with the specified partition key and row key.
        /// </summary>
        /// <param name="partitionKey">A string containing the partition key of the <see cref="T:Microsoft.WindowsAzure.Storage.Table.TableEntity" /> to be initialized.</param>
        /// <param name="rowKey">A string containing the row key of the <see cref="T:Microsoft.WindowsAzure.Storage.Table.TableEntity" /> to be initialized.</param>
        protected LongBasedRowKeyEntity(string partitionKey, string rowKey) :
            base(partitionKey, rowKey)
        {
            ETag = "*";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LongBasedRowKeyEntity"/> class.
        /// </summary>
        protected LongBasedRowKeyEntity()
        {
            ETag = "*";
        }

        #endregion

        #region methods

        /// <summary>
        /// Makes the row key from a long value.
        /// </summary>
        /// <param name="rowKey">The row key.</param>
        /// <returns>System.String.</returns>
        public static string MakeRowKeyFromLong(long rowKey)
        {
            return
                rowKey < 0
                    ? ((rowKey*-1) - long.MaxValue).ToString("00000000000000000000")
                    : rowKey.ToString("00000000000000000000");
        }

        #endregion
    }
}