namespace FleetFlow.Api.Models;

public class AuditLog
{
    public int AuditLogId { get; set; }
    public int? CompanyId { get; set; }
    public int? UserId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

