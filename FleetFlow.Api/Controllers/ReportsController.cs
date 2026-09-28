using FleetFlow.Api.DTOs;
using FleetFlow.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _service;

    public MaintenanceController(IMaintenanceService service)
    {
        _service = service;
    }

    private int GetCompanyId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "companyId");
        return claim is null ? 0 : int.Parse(claim.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetMaintenance()
    {
        return Ok(await _service.GetMaintenanceAsync(GetCompanyId()));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateMaintenanceRequest request)
    {
        var item = await _service.CreateMaintenanceAsync(GetCompanyId(), request);
        return Ok(item);
    }
}

