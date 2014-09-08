#region usings

using System;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Queues
{
    public abstract class BaseQueuedActionEntity : LongBasedRowKeyEntity
    {
        #region fields

        private long _longRowKey;

        #endregion

        #region properties

        public DateTime LastTouched { get; set; }

        public long LongRowKey
        {
            get { return _longRowKey; }
            set
            {
                _longRowKey = value;
                RowKey = MakeRowKeyFromLong(_longRowKey);
                PartitionKey = RowKey.Substring(0, 3);
            }
        }

        public string Message { get; set; }
        public string Status { get; set; }

        #endregion

        #region constructors

        public BaseQueuedActionEntity(string partitionKey, string rowKey) :
            base(partitionKey, rowKey)
        {
        }

        public BaseQueuedActionEntity()
        {
        }

        #endregion
    }
}