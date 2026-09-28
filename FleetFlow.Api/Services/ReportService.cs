using FleetFlow.Api.Data;
using FleetFlow.Api.DTOs;
using FleetFlow.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetFlow.Api.Services;

public interface IMaintenanceService
{
    Task<List<MaintenanceItemDto>> GetMaintenanceAsync(int companyId);
    Task<MaintenanceItemDto> CreateMaintenanceAsync(int companyId, CreateMaintenanceRequest request);
}

public class MaintenanceService : IMaintenanceService
{
    private readonly FleetFlowDbContext _context;

    public MaintenanceService(FleetFlowDbContext context)
    {
        _context = context;
    }

    public async Task<List<MaintenanceItemDto>> GetMaintenanceAsync(int companyId)
    {
        var schedules = await _context.MaintenanceSchedules
            .Include(s => s.Vehicle)
            .Include(s => s.ServiceType)
            .Where(s => s.Vehicle != null && s.Vehicle.CompanyId == companyId)
            .ToListAsync();

        var result = new List<MaintenanceItemDto>();

        foreach (var schedule in schedules)
        {
            var status = "Current";
            var today = DateTime.UtcNow;
            if (schedule.NextDueDate.HasValue && schedule.NextDueDate.Value < today)
                status = "Overdue";
            else if (schedule.NextDueDate.HasValue && schedule.NextDueDate.Value <= today.AddDays(30))
                status = "Due Soon";

            result.Add(new MaintenanceItemDto
            {
                ScheduleId = schedule.ScheduleId,
                VehicleNumber = schedule.Vehicle?.VehicleNumber ?? string.Empty,
                ServiceType = schedule.ServiceType?.Name ?? string.Empty,
                LastServiceDate = schedule.LastServiceDate,
                DueDate = schedule.NextDueDate,
                CurrentMileage = schedule.Vehicle?.CurrentMileage ?? 0,
                DueMileage = schedule.NextDueMileage,
                Status = status
            });
        }

        return result;
    }

    public async Task<MaintenanceItemDto> CreateMaintenanceAsync(int companyId, CreateMaintenanceRequest request)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == request.VehicleId && v.CompanyId == companyId);
        if (vehicle is null) throw new InvalidOperationException("Vehicle not found in company.");

        var serviceType = await _context.ServiceTypes.FirstOrDefaultAsync(s => s.ServiceTypeId == request.ServiceTypeId && s.CompanyId == companyId);
        if (serviceType is null) throw new InvalidOperationException("Service type not found.");

        var schedule = new MaintenanceSchedule
        {
            VehicleId = request.VehicleId,
            ServiceTypeId = request.ServiceTypeId,
            LastServiceMileage = request.LastServiceMileage,
            LastServiceDate = request.LastServiceDate,
            NextDueMileage = request.NextDueMileage,
            NextDueDate = request.NextDueDate
        };

        _context.MaintenanceSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        return new MaintenanceItemDto
        {
            ScheduleId = schedule.ScheduleId,
            VehicleNumber = vehicle.VehicleNumber,
            ServiceType = serviceType.Name,
            LastServiceDate = schedule.LastServiceDate,
            DueDate = schedule.NextDueDate,
            CurrentMileage = vehicle.CurrentMileage,
            DueMileage = schedule.NextDueMileage,
            Status = "Current"
        };
    }
}

