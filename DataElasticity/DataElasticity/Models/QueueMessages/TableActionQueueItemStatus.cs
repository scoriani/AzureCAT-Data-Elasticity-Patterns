namespace Microsoft.AzureCat.Patterns.DataElasticity.Models.QueueMessages
{
    public enum TableActionQueueItemStatus
    {
        Queued,
        InProcess,
        Completed,
        Errored,
        NotFound,
    }
}