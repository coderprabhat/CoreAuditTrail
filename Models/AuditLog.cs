namespace CoreAuditTrail.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string EntityName { get; set; }
        public required string EntityId { get; set; }
        public required string Changes { get; set; }
        public required string Action { get; set; }
        public required DateTime Timestamp { get; set; }
        public required Guid CreatedBy { get; set; }
        public required string UserEmail { get; set; }
    }
}
