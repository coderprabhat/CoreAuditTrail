namespace CoreAuditTrail.Models
{
    public class Contact : AuditableEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Unique identifier
        public required string Name { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}
