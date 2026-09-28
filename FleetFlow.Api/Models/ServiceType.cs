namespace FleetFlow.Api.Models;

public class Vehicle
{
    public int VehicleId { get; set; }
    public int CompanyId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string VIN { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? LicensePlate { get; set; }
    public int CurrentMileage { get; set; }
    public string Status { get; set; } = "Active";
    public string? AssignedDriver { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public Company? Company { get; set; }
    public ICollection<MaintenanceSchedule> MaintenanceSchedules { get; set; } = new List<MaintenanceSchedule>();
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    public ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();
}

