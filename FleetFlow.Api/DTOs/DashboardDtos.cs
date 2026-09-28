namespace FleetFlow.Api.DTOs;

public class MaintenanceItemDto
{
    public int ScheduleId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public DateTime? LastServiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int CurrentMileage { get; set; }
    public int? DueMileage { get; set; }
    public string Status { get; set; } = "Current";
}

public class CreateMaintenanceRequest
{
    public int VehicleId { get; set; }
    public int ServiceTypeId { get; set; }
    public int LastServiceMileage { get; set; }
    public DateTime? LastServiceDate { get; set; }
    public int? NextDueMileage { get; set; }
    public DateTime? NextDueDate { get; set; }
}

