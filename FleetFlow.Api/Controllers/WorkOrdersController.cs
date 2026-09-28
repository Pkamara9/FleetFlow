using FleetFlow.Api.DTOs;
using FleetFlow.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FleetFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    private int GetCompanyId()
    {
        var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "companyId");
        return companyIdClaim is null ? 0 : int.Parse(companyIdClaim.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var companyId = GetCompanyId();
        return Ok(await _vehicleService.GetVehiclesAsync(companyId));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var companyId = GetCompanyId();
        var vehicle = await _vehicleService.GetVehicleByIdAsync(companyId, id);
        return vehicle is null ? NotFound() : Ok(vehicle);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateVehicleRequest request)
    {
        var companyId = GetCompanyId();
        var vehicle = await _vehicleService.CreateVehicleAsync(companyId, request);
        return CreatedAtAction(nameof(GetById), new { id = vehicle.VehicleId }, vehicle);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateVehicleRequest request)
    {
        var companyId = GetCompanyId();
        var vehicle = await _vehicleService.UpdateVehicleAsync(companyId, id, request);
        return vehicle is null ? NotFound() : Ok(vehicle);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        var companyId = GetCompanyId();
        var deleted = await _vehicleService.DeleteVehicleAsync(companyId, id);
        return deleted ? NoContent() : NotFound();
    }
}

