using FleetFlow.Api.DTOs;
using FleetFlow.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkOrdersController : ControllerBase
{
    private readonly IWorkOrderService _service;

    public WorkOrdersController(IWorkOrderService service)
    {
        _service = service;
    }

    private int GetCompanyId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "companyId");
        return claim is null ? 0 : int.Parse(claim.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetWorkOrdersAsync(GetCompanyId()));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetWorkOrderByIdAsync(GetCompanyId(), id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateWorkOrderRequest request)
    {
        var item = await _service.CreateWorkOrderAsync(GetCompanyId(), request);
        return CreatedAtAction(nameof(GetById), new { id = item.WorkOrderId }, item);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager,Technician")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateWorkOrderRequest request)
    {
        var item = await _service.UpdateWorkOrderAsync(GetCompanyId(), id, request);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteWorkOrderAsync(GetCompanyId(), id);
        return deleted ? NoContent() : NotFound();
    }
}

