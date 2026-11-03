using FleetFlow.Api.Data;
using FleetFlow.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FleetFlow.Api.Services;

public interface IReportService
{
    Task<DashboardDto> GetDashboardAsync(int companyId);
}

public class ReportService : IReportService
{
    private readonly FleetFlowDbContext _context;

    public ReportService(FleetFlowDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto> GetDashboardAsync(int companyId)
    {
        var vehicles = await _context.Vehicles.Where(v => v.CompanyId == companyId).ToListAsync();
        var workOrders = await _context.WorkOrders
            .Include(w => w.Vehicle)
            .Where(w => w.Vehicle != null && w.Vehicle.CompanyId == companyId)
            .ToListAsync();

        var maintenanceSchedules = await _context.MaintenanceSchedules
            .Include(s => s.Vehicle)
            .Where(s => s.Vehicle != null && s.Vehicle.CompanyId == companyId)
            .ToListAsync();

        var totalVehicles = vehicles.Count;
        var openWorkOrders = workOrders.Count(w => w.Status != "Completed");
        var dueIn30Days = maintenanceSchedules.Count(s => s.NextDueDate.HasValue && s.NextDueDate.Value <= DateTime.UtcNow.AddDays(30));
        var overdueMaintenance = maintenanceSchedules.Count(s => s.NextDueDate.HasValue && s.NextDueDate.Value < DateTime.UtcNow);
        var monthlyMaintenanceCost = workOrders
            .Where(w => w.CreatedDate >= DateTime.UtcNow.AddDays(-30))
            .Sum(w => w.TotalCost);

        var vehicleStatusChart = new List<ChartValue>
        {
            new() { Name = "Active", Value = vehicles.Count(v => v.Status == "Active") },
            new() { Name = "In Service", Value = vehicles.Count(v => v.Status == "In Service") },
            new() { Name = "Maintenance", Value = vehicles.Count(v => v.Status == "Maintenance") }
        };

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var maintenanceCostChart = Enumerable.Range(0, 6)
            .Select(i => monthStart.AddMonths(i - 5))
            .Select(m => new ChartValue
            {
                Name = m.ToString("MMM yyyy"),
                Value = workOrders.Where(w => w.CreatedDate.Year == m.Year && w.CreatedDate.Month == m.Month).Sum(w => w.TotalCost)
            })
            .ToList();

        var workOrderStatusChart = new List<ChartValue>
        {
            new() { Name = "Open", Value = workOrders.Count(w => w.Status == "Open") },
            new() { Name = "In Progress", Value = workOrders.Count(w => w.Status == "In Progress") },
            new() { Name = "Waiting Parts", Value = workOrders.Count(w => w.Status == "Waiting Parts") },
            new() { Name = "Completed", Value = workOrders.Count(w => w.Status == "Completed") }
        };

        return new DashboardDto
        {
            TotalVehicles = totalVehicles,
            OpenWorkOrders = openWorkOrders,
            DueIn30Days = dueIn30Days,
            OverdueMaintenance = overdueMaintenance,
            MonthlyMaintenanceCost = monthlyMaintenanceCost,
            VehicleStatusChart = vehicleStatusChart,
            MaintenanceCostChart = maintenanceCostChart,
            WorkOrderStatusChart = workOrderStatusChart
        };
    }
}
