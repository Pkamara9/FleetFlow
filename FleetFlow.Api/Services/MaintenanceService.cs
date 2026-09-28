using FleetFlow.Api.Data;
using FleetFlow.Api.DTOs;
using FleetFlow.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetFlow.Api.Services;

public interface IWorkOrderService
{
    Task<List<WorkOrderDto>> GetWorkOrdersAsync(int companyId);
    Task<WorkOrderDto?> GetWorkOrderByIdAsync(int companyId, int id);
    Task<WorkOrderDto> CreateWorkOrderAsync(int companyId, CreateWorkOrderRequest request);
    Task<WorkOrderDto?> UpdateWorkOrderAsync(int companyId, int id, CreateWorkOrderRequest request);
    Task<bool> DeleteWorkOrderAsync(int companyId, int id);
}

public class WorkOrderService : IWorkOrderService
{
    private readonly FleetFlowDbContext _context;

    public WorkOrderService(FleetFlowDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkOrderDto>> GetWorkOrdersAsync(int companyId)
    {
        var query = await _context.WorkOrders
            .Include(w => w.Vehicle)
            .Where(w => w.Vehicle != null && w.Vehicle.CompanyId == companyId)
            .OrderByDescending(w => w.CreatedDate)
            .ToListAsync();

        return query.Select(w => new WorkOrderDto
        {
            WorkOrderId = w.WorkOrderId,
            VehicleId = w.VehicleId,
            AssignedUserId = w.AssignedUserId,
            VehicleNumber = w.Vehicle?.VehicleNumber ?? string.Empty,
            Title = w.Title,
            Description = w.Description,
            Priority = w.Priority,
            Status = w.Status,
            LaborCost = w.LaborCost,
            PartsCost = w.PartsCost,
            TotalCost = w.TotalCost,
            CreatedDate = w.CreatedDate,
            CompletedDate = w.CompletedDate
        }).ToList();
    }

    public async Task<WorkOrderDto?> GetWorkOrderByIdAsync(int companyId, int id)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.Vehicle)
            .FirstOrDefaultAsync(w => w.WorkOrderId == id && w.Vehicle != null && w.Vehicle.CompanyId == companyId);

        if (workOrder is null) return null;

        return new WorkOrderDto
        {
            WorkOrderId = workOrder.WorkOrderId,
            VehicleId = workOrder.VehicleId,
            AssignedUserId = workOrder.AssignedUserId,
            VehicleNumber = workOrder.Vehicle?.VehicleNumber ?? string.Empty,
            Title = workOrder.Title,
            Description = workOrder.Description,
            Priority = workOrder.Priority,
            Status = workOrder.Status,
            LaborCost = workOrder.LaborCost,
            PartsCost = workOrder.PartsCost,
            TotalCost = workOrder.TotalCost,
            CreatedDate = workOrder.CreatedDate,
            CompletedDate = workOrder.CompletedDate
        };
    }

    public async Task<WorkOrderDto> CreateWorkOrderAsync(int companyId, CreateWorkOrderRequest request)
    {
        var vehicleExists = await _context.Vehicles.AnyAsync(v => v.VehicleId == request.VehicleId && v.CompanyId == companyId);
        if (!vehicleExists) throw new InvalidOperationException("Vehicle not found in your company.");

        var workOrder = new WorkOrder
        {
            VehicleId = request.VehicleId,
            AssignedUserId = request.AssignedUserId,
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            Status = request.Status,
            LaborCost = 0,
            PartsCost = 0,
            TotalCost = 0
        };

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync();

        return await GetWorkOrderByIdAsync(companyId, workOrder.WorkOrderId) ?? new WorkOrderDto();
    }

    public async Task<WorkOrderDto?> UpdateWorkOrderAsync(int companyId, int id, CreateWorkOrderRequest request)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.Vehicle)
            .FirstOrDefaultAsync(w => w.WorkOrderId == id && w.Vehicle != null && w.Vehicle.CompanyId == companyId);

        if (workOrder is null) return null;

        workOrder.VehicleId = request.VehicleId;
        workOrder.AssignedUserId = request.AssignedUserId;
        workOrder.Title = request.Title;
        workOrder.Description = request.Description;
        workOrder.Priority = request.Priority;
        workOrder.Status = request.Status;

        if (request.Status == "Completed")
        {
            workOrder.CompletedDate ??= DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return await GetWorkOrderByIdAsync(companyId, id);
    }

    public async Task<bool> DeleteWorkOrderAsync(int companyId, int id)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.Vehicle)
            .FirstOrDefaultAsync(w => w.WorkOrderId == id && w.Vehicle != null && w.Vehicle.CompanyId == companyId);

        if (workOrder is null) return false;

        _context.WorkOrders.Remove(workOrder);
        await _context.SaveChangesAsync();
        return true;
    }
}

