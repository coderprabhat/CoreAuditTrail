using CoreAuditTrail.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Numerics;
using System.Security.Claims;
using System.Text;

namespace CoreAuditTrail.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor contextAccessor)
            : base(options)
        {
            _contextAccessor = contextAccessor;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var modifiedEntries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                .ToList();

            foreach (var modifiedEntity in modifiedEntries)
            {
                var primaryKey = GetPrimaryKeyValue(modifiedEntity);
                var auditLog = new AuditLog
                {
                    EntityName = modifiedEntity.Entity.GetType().Name,
                    EntityId = primaryKey?.ToString() ?? "Unknown",
                    UserEmail = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email) ?? "Unknown",
                    Action = modifiedEntity.State.ToString(),
                    Timestamp = DateTime.UtcNow,
                    CreatedBy = Guid.TryParse(_contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
                                ? userId
                                : Guid.Empty,
                    Changes = GetChanges(modifiedEntity)
                };

                // Add the AuditLog to the DbContext
                AuditLogs.Add(auditLog);
            }

            // Call the base SaveChangesAsync to persist changes
            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
             .Property(u => u.Role)
             .HasConversion(
                 r => r.ToString(), // Convert enum to string
                 r => (User.UserRole)Enum.Parse(typeof(User.UserRole), r) // Convert string back to enum
             );
        }

        private string GetChanges(EntityEntry modifiedEntity)
        {
            var changes = new StringBuilder();
            foreach (var property in modifiedEntity.OriginalValues.Properties)
            {
                var originalValue = modifiedEntity.OriginalValues[property];
                var currentValue = modifiedEntity.CurrentValues[property];
                if (!Equals(originalValue, currentValue))
                {
                    changes.AppendLine($"{property.Name} : From '{originalValue}' to '{currentValue}'");
                }
            }
            return changes.ToString();
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);
        }

        private object? GetPrimaryKeyValue(EntityEntry entry)
        {
            var keyName = entry.Metadata.FindPrimaryKey()?.Properties.FirstOrDefault()?.Name;
            if (keyName != null)
            {
                return entry.CurrentValues[keyName];
            }
            return null;
        }
    }
}
