namespace FleetFlow.Api.Models;

public class WorkOrder
{
    public int WorkOrderId { get; set; }
    public int VehicleId { get; set; }
    public int? AssignedUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";
    public string Status { get; set; } = "Open";
    public decimal LaborCost { get; set; }
    public decimal PartsCost { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedDate { get; set; }

    public Vehicle? Vehicle { get; set; }
    public User? AssignedUser { get; set; }
    public ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();
}

