namespace CoreAuditTrail.Models
{
    public class User : AuditableEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public string Salt { get; set; } = string.Empty;


        public UserRole Role { get; set; } = UserRole.User;

        public enum UserRole
        {
            Admin,
            User,
            Manager,
            Finance
        }
    }
}
