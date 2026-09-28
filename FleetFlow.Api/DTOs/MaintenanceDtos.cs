namespace FleetFlow.Api.DTOs;

public class WorkOrderDto
{
    public int WorkOrderId { get; set; }
    public int VehicleId { get; set; }
    public int? AssignedUserId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal LaborCost { get; set; }
    public decimal PartsCost { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
}

public class CreateWorkOrderRequest
{
    public int VehicleId { get; set; }
    public int? AssignedUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";
    public string Status { get; set; } = "Open";
}

