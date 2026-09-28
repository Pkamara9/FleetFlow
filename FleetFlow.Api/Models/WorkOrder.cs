namespace FleetFlow.Api.Models;

public class MaintenanceSchedule
{
    public int ScheduleId { get; set; }
    public int VehicleId { get; set; }
    public int ServiceTypeId { get; set; }
    public int LastServiceMileage { get; set; }
    public DateTime? LastServiceDate { get; set; }
    public int? NextDueMileage { get; set; }
    public DateTime? NextDueDate { get; set; }

    public Vehicle? Vehicle { get; set; }
    public ServiceType? ServiceType { get; set; }
}

