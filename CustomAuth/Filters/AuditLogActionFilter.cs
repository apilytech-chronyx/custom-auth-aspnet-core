using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace CustomAuth.Filters
{
    public class AuditLogActionFilter : IAsyncActionFilter
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<AuditLogActionFilter> _logger;

        public AuditLogActionFilter(IAuditService auditService, ILogger<AuditLogActionFilter> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Get request headers
            var soapOperation = string.Empty;
            var correlationId = string.Empty;

            if (context.HttpContext.Request.Headers.TryGetValue("x-soap-operation", out var soapHeader))
            {
                soapOperation = soapHeader.ToString();
            }

            if (context.HttpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationHeader))
            {
                correlationId = correlationHeader.ToString();
            }

            // Execute the action
            var resultContext = await next();

            // After action execution, check if there are tracked changes
            if (context.HttpContext.HasChanges())
            {
                var changes = context.HttpContext.GetChangeTracker();

                // Get entity info from route data
                var entityType = context.RouteData.Values["controller"]?.ToString() ?? "Unknown";
                var entityId = context.RouteData.Values["id"]?.ToString() ?? "Unknown";

                // Get user info
                var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
                var username = context.HttpContext.User.Identity?.Name;

                // Create the audit entry
                var auditEntry = new AuditEntry
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Action = context.HttpContext.Request.Method,
                    UserId = userId,
                    Username = username,
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), // Current UTC time formatted
                    Changes = changes,
                    SoapOperation = soapOperation,
                    //CorrelationId = correlationId,
                    //ClientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
                };

                // Save the audit entry
                await _auditService.SaveAuditEntryAsync(auditEntry);

                _logger.LogInformation(
                    "Audit: {EntityType} {EntityId} {Action} by {Username} at {Timestamp}, SOAP: {SoapOp}, Fields: {Fields}",
                    entityType,
                    entityId,
                    context.HttpContext.Request.Method,
                    username,
                    auditEntry.Timestamp,
                    soapOperation,
                    string.Join(", ", changes.Keys)
                );
            }
        }
    }
}
