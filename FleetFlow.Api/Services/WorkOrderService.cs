using FleetFlow.Api.Data;
using FleetFlow.Api.DTOs;
using FleetFlow.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetFlow.Api.Services;

public interface IVehicleService
{
    Task<List<VehicleDto>> GetVehiclesAsync(int companyId);
    Task<VehicleDto?> GetVehicleByIdAsync(int companyId, int id);
    Task<VehicleDto> CreateVehicleAsync(int companyId, CreateVehicleRequest request);
    Task<VehicleDto?> UpdateVehicleAsync(int companyId, int id, CreateVehicleRequest request);
    Task<bool> DeleteVehicleAsync(int companyId, int id);
}

public class VehicleService : IVehicleService
{
    private readonly FleetFlowDbContext _context;

    public VehicleService(FleetFlowDbContext context)
    {
        _context = context;
    }

    public async Task<List<VehicleDto>> GetVehiclesAsync(int companyId)
    {
        return await _context.Vehicles
            .Where(v => v.CompanyId == companyId)
            .OrderBy(v => v.VehicleNumber)
            .Select(v => new VehicleDto
            {
                VehicleId = v.VehicleId,
                CompanyId = v.CompanyId,
                VehicleNumber = v.VehicleNumber,
                VIN = v.VIN,
                Year = v.Year,
                Make = v.Make,
                Model = v.Model,
                LicensePlate = v.LicensePlate,
                CurrentMileage = v.CurrentMileage,
                Status = v.Status,
                AssignedDriver = v.AssignedDriver,
                CreatedDate = v.CreatedDate
            })
            .ToListAsync();
    }

    public async Task<VehicleDto?> GetVehicleByIdAsync(int companyId, int id)
    {
        return await _context.Vehicles
            .Where(v => v.CompanyId == companyId && v.VehicleId == id)
            .Select(v => new VehicleDto
            {
                VehicleId = v.VehicleId,
                CompanyId = v.CompanyId,
                VehicleNumber = v.VehicleNumber,
                VIN = v.VIN,
                Year = v.Year,
                Make = v.Make,
                Model = v.Model,
                LicensePlate = v.LicensePlate,
                CurrentMileage = v.CurrentMileage,
                Status = v.Status,
                AssignedDriver = v.AssignedDriver,
                CreatedDate = v.CreatedDate
            })
            .FirstOrDefaultAsync();
    }

    public async Task<VehicleDto> CreateVehicleAsync(int companyId, CreateVehicleRequest request)
    {
        var vehicle = new Vehicle
        {
            CompanyId = companyId,
            VehicleNumber = request.VehicleNumber,
            VIN = request.VIN,
            Year = request.Year,
            Make = request.Make,
            Model = request.Model,
            LicensePlate = request.LicensePlate,
            CurrentMileage = request.CurrentMileage,
            Status = request.Status,
            AssignedDriver = request.AssignedDriver
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        return new VehicleDto
        {
            VehicleId = vehicle.VehicleId,
            CompanyId = vehicle.CompanyId,
            VehicleNumber = vehicle.VehicleNumber,
            VIN = vehicle.VIN,
            Year = vehicle.Year,
            Make = vehicle.Make,
            Model = vehicle.Model,
            LicensePlate = vehicle.LicensePlate,
            CurrentMileage = vehicle.CurrentMileage,
            Status = vehicle.Status,
            AssignedDriver = vehicle.AssignedDriver,
            CreatedDate = vehicle.CreatedDate
        };
    }

    public async Task<VehicleDto?> UpdateVehicleAsync(int companyId, int id, CreateVehicleRequest request)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.CompanyId == companyId && v.VehicleId == id);
        if (vehicle is null) return null;

        vehicle.VehicleNumber = request.VehicleNumber;
        vehicle.VIN = request.VIN;
        vehicle.Year = request.Year;
        vehicle.Make = request.Make;
        vehicle.Model = request.Model;
        vehicle.LicensePlate = request.LicensePlate;
        vehicle.CurrentMileage = request.CurrentMileage;
        vehicle.Status = request.Status;
        vehicle.AssignedDriver = request.AssignedDriver;

        await _context.SaveChangesAsync();

        return await GetVehicleByIdAsync(companyId, id);
    }

    public async Task<bool> DeleteVehicleAsync(int companyId, int id)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.CompanyId == companyId && v.VehicleId == id);
        if (vehicle is null) return false;

        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync();
        return true;
    }
}

