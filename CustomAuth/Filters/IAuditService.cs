namespace CustomAuth.Filters
{
    public interface IAuditService
    {
        Task SaveAuditEntryAsync(AuditEntry entry);
    }
}
