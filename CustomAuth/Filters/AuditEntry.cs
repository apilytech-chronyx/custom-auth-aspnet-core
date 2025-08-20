namespace CustomAuth.Filters
{
    public class AuditEntry
    {
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public string Action { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Timestamp { get; set; }
        public Dictionary<string, (object OldValue, object NewValue)> Changes { get; set; }
        public string SoapOperation { get; set; }
    }
}
