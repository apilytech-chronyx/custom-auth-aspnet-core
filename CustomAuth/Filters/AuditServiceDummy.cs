
namespace CustomAuth.Filters
{
    public class AuditServiceDummy : IAuditService
    {
        public Task SaveAuditEntryAsync(AuditEntry entry)
        {
            return Task.CompletedTask; // No operation, just a dummy implementation
        }
    }
}
