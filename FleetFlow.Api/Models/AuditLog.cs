namespace FleetFlow.Api.Models;

public class ServiceRecord
{
    public int ServiceRecordId { get; set; }
    public int VehicleId { get; set; }
    public int? WorkOrderId { get; set; }
    public int ServiceTypeId { get; set; }
    public DateTime ServiceDate { get; set; }
    public int Mileage { get; set; }
    public decimal Cost { get; set; }
    public string? Notes { get; set; }

    public Vehicle? Vehicle { get; set; }
    public WorkOrder? WorkOrder { get; set; }
    public ServiceType? ServiceType { get; set; }
}

