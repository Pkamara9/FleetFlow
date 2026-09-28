namespace FleetFlow.Api.DTOs;

public class DashboardDto
{
    public int TotalVehicles { get; set; }
    public int OpenWorkOrders { get; set; }
    public int DueIn30Days { get; set; }
    public int OverdueMaintenance { get; set; }
    public decimal MonthlyMaintenanceCost { get; set; }
    public List<ChartValue> VehicleStatusChart { get; set; } = new();
    public List<ChartValue> MaintenanceCostChart { get; set; } = new();
    public List<ChartValue> WorkOrderStatusChart { get; set; } = new();
}

public class ChartValue
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

