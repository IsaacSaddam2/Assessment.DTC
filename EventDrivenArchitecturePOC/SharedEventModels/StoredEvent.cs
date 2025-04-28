namespace SharedEventModels
{
    public class StoredEvent
    {
        public long Id { get; set; }
        public Guid AggregateId { get; set; }
        public string Type { get; set; } = null!;
        public string Payload { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
}
