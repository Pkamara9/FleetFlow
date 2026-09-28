namespace FleetFlow.Api.Models;

public class ServiceType
{
    public int ServiceTypeId { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? IntervalMiles { get; set; }
    public int? IntervalDays { get; set; }

    public Company? Company { get; set; }
    public ICollection<MaintenanceSchedule> MaintenanceSchedules { get; set; } = new List<MaintenanceSchedule>();
    public ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();
}

