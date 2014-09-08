using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models;
using Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Models.Queues;
using Microsoft.AzureCat.Patterns.DataElasticity.Models.QueueMessages;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.AzureCat.Patterns.DataElasticity.AzureTableStore.Requests
{
    /// <summary>
    /// Class RequestManagerBase implements the access to the 
    /// action queues and supporting tables for Data Elasticity
    /// </summary>
    /// <typeparam name="TRequest">A type of Queue Request for a Data Elasticity action.</typeparam>
    /// <typeparam name="TAction">The Action associated with the request</typeparam>
    internal abstract class RequestManagerBase<TAction, TRequest>
        where TAction : BaseQueuedActionEntity, new()
        where TRequest : BaseQueueRequest<TRequest>
    {
        #region fields

        protected readonly CloudQueue Queue;
        protected readonly CloudTableClient TableClient;

        #endregion

        #region properties

        /// <summary>
        /// Gets the name of the Azure table storing the queue data.
        /// </summary>
        /// <value>The name of the table.</value>
        protected abstract string TableName { get; }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestManagerBase{TAction,TRequest}" /> class.
        /// </summary>
        /// <param name="queue">The action queue holding the Data Elasticity actions.</param>
        /// <param name="tableClient">The table client.</param>
        protected RequestManagerBase(CloudQueue queue, CloudTableClient tableClient)
        {
            Queue = queue;
            TableClient = tableClient;
        }

        #endregion

        #region methods

        /// <summary>
        /// Gets the actions based on the status.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>IList{TRequest}.</returns>
        public IList<TAction> GetActions(TableActionQueueItemStatus status)
        {
            var condition = TableQuery.GenerateFilterCondition("Status", QueryComparisons.Equal, status.ToString());

            var query =
                new TableQuery<TAction>()
                    .Where(condition);

            var table = TableClient.GetTableReference(TableName);

            IList<TAction> result = null;
            RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy()
                .ExecuteAction(() => { result = table.ExecuteQuery(query).ToList(); });

            return result;
        }

        /// <summary>
        /// Gets the next action.
        /// </summary>
        /// <returns>TRequest</returns>
        public TRequest GetNextAction()
        {
            //Get the next message
            var message = Queue.GetMessage();
            if (message == null)
                return null;

            // Look up the table row for the message
            var request = GetAction(Int64.Parse(message.AsString));

            //Delete the message... If it errors during processing the consumer should make a new queue entry
            RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy()
                .ExecuteAction(() => Queue.DeleteMessage(message));

            return request != null ? CreateRequest(request) : null;
        }

        /// <summary>
        /// Determines whether there is a queued request in process.
        /// </summary>
        /// <returns><c>true</c> if there is a queued request in process; otherwise, <c>false</c>.</returns>
        public bool IsInProcess()
        {
            var condition1 = TableQuery.GenerateFilterCondition("Status", QueryComparisons.Equal,
                TableActionQueueItemStatus.InProcess.ToString());
            var condition2 = TableQuery.GenerateFilterCondition("Status", QueryComparisons.Equal,
                TableActionQueueItemStatus.Queued.ToString());
            var condition = TableQuery.CombineFilters(condition1, TableOperators.Or, condition2);

            var query =
                new TableQuery<TAction>()
                    .Where(condition);

            var table = TableClient.GetTableReference(TableName);

            var result = false;
            RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy()
                .ExecuteAction(() => result = table.ExecuteQuery(query).Any());

            return result;
        }

        protected void AddQueueMessage(long rowKey)
        {
            RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy()
                .ExecuteAction(
                    () => Queue.AddMessage(new CloudQueueMessage(rowKey.ToString(CultureInfo.InvariantCulture))));
        }

        /// <summary>
        /// Creates a new Azure request from the shard action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>TRequest</returns>
        protected abstract TRequest CreateRequest(TAction action);

        /// <summary>
        /// Gets the action.
        /// </summary>
        /// <param name="queueId">The queue identifier.</param>
        /// <returns>TAction.</returns>
        protected TAction GetAction(long queueId)
        {
            var rowKey = LongBasedRowKeyEntity.MakeRowKeyFromLong(queueId);

            var condition = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey);

            var query =
                new TableQuery<TAction>()
                    .Where(condition);

            var table = TableClient.GetTableReference(TableName);

            TAction result = null;
            RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy()
                .ExecuteAction(() => { result = table.ExecuteQuery(query).FirstOrDefault(); });

            return result;
        }


        protected static void InsertTableAction(CloudTable shardCreationsTable, BaseQueuedActionEntity action)
        {
            RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy()
                .ExecuteAction(() => shardCreationsTable.Execute(TableOperation.Insert(action)));
        }

        protected static void MergeTableAction(CloudTable shardCreationsTable, BaseQueuedActionEntity action)
        {
            RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy()
                .ExecuteAction(() => shardCreationsTable.Execute(TableOperation.Merge(action)));
        }

        #endregion
    }
}