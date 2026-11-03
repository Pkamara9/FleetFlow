using FleetFlow.Api.Data;
using FleetFlow.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetFlow.Api.Controllers;

public abstract class CompanyControllerBase : ControllerBase
{
    protected int GetCompanyId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "companyId");
        return claim is null ? 0 : int.Parse(claim.Value);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : CompanyControllerBase
{
    private readonly FleetFlowDbContext _db;
    public UsersController(FleetFlowDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var companyId = GetCompanyId();
        var users = await _db.Users
            .Where(u => u.CompanyId == companyId && u.IsActive)
            .OrderBy(u => u.FirstName)
            .Select(u => new { u.UserId, u.FirstName, u.LastName, u.Email, u.Role })
            .ToListAsync();
        return Ok(users);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServiceTypesController : CompanyControllerBase
{
    private readonly FleetFlowDbContext _db;
    public ServiceTypesController(FleetFlowDbContext db) => _db = db;

    public class ServiceTypeRequest
    {
        public string Name { get; set; } = string.Empty;
        public int? IntervalMiles { get; set; }
        public int? IntervalDays { get; set; }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var companyId = GetCompanyId();
        return Ok(await _db.ServiceTypes
            .Where(s => s.CompanyId == companyId)
            .OrderBy(s => s.Name)
            .Select(s => new { s.ServiceTypeId, s.Name, s.IntervalMiles, s.IntervalDays })
            .ToListAsync());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] ServiceTypeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) throw new InvalidOperationException("Name is required.");
        var st = new ServiceType
        {
            CompanyId = GetCompanyId(),
            Name = request.Name.Trim(),
            IntervalMiles = request.IntervalMiles,
            IntervalDays = request.IntervalDays
        };
        _db.ServiceTypes.Add(st);
        await _db.SaveChangesAsync();
        return Ok(new { st.ServiceTypeId, st.Name, st.IntervalMiles, st.IntervalDays });
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServiceRecordsController : CompanyControllerBase
{
    private readonly FleetFlowDbContext _db;
    public ServiceRecordsController(FleetFlowDbContext db) => _db = db;

    public class ServiceRecordRequest
    {
        public int VehicleId { get; set; }
        public int ServiceTypeId { get; set; }
        public int? WorkOrderId { get; set; }
        public DateTime? ServiceDate { get; set; }
        public int Mileage { get; set; }
        public decimal Cost { get; set; }
        public string? Notes { get; set; }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? vehicleId)
    {
        var companyId = GetCompanyId();
        var query = _db.ServiceRecords
            .Where(s => s.Vehicle!.CompanyId == companyId);
        if (vehicleId.HasValue) query = query.Where(s => s.VehicleId == vehicleId.Value);

        var items = await query
            .OrderByDescending(s => s.ServiceDate)
            .Select(s => new
            {
                s.ServiceRecordId,
                s.VehicleId,
                VehicleNumber = s.Vehicle!.VehicleNumber,
                s.WorkOrderId,
                s.ServiceTypeId,
                ServiceType = s.ServiceType!.Name,
                s.ServiceDate,
                s.Mileage,
                s.Cost,
                s.Notes
            })
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Technician")]
    public async Task<IActionResult> Create([FromBody] ServiceRecordRequest request)
    {
        var companyId = GetCompanyId();
        var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == request.VehicleId && v.CompanyId == companyId)
            ?? throw new InvalidOperationException("Vehicle not found.");
        var type = await _db.ServiceTypes.FirstOrDefaultAsync(s => s.ServiceTypeId == request.ServiceTypeId && s.CompanyId == companyId)
            ?? throw new InvalidOperationException("Service type not found.");

        var date = request.ServiceDate ?? DateTime.UtcNow;
        var record = new ServiceRecord
        {
            VehicleId = vehicle.VehicleId,
            ServiceTypeId = type.ServiceTypeId,
            WorkOrderId = request.WorkOrderId,
            ServiceDate = date,
            Mileage = request.Mileage,
            Cost = request.Cost,
            Notes = request.Notes
        };
        _db.ServiceRecords.Add(record);

        if (request.Mileage > vehicle.CurrentMileage) vehicle.CurrentMileage = request.Mileage;

        // Roll the matching schedule forward using the service type's intervals.
        var schedule = await _db.MaintenanceSchedules
            .FirstOrDefaultAsync(m => m.VehicleId == vehicle.VehicleId && m.ServiceTypeId == type.ServiceTypeId);
        if (schedule is not null)
        {
            schedule.LastServiceDate = date;
            schedule.LastServiceMileage = request.Mileage;
            if (type.IntervalDays.HasValue) schedule.NextDueDate = date.AddDays(type.IntervalDays.Value);
            if (type.IntervalMiles.HasValue) schedule.NextDueMileage = request.Mileage + type.IntervalMiles.Value;
        }

        await _db.SaveChangesAsync();
        return Ok(new { record.ServiceRecordId });
    }
}
