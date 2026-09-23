namespace CustomHome.Models
{
    public enum QueueOperationResult
    {
        Success,
        QueueFull,
        ServingCapacityFull,
        NoWaitingCustomer,
        TokenNotFound
    }
}