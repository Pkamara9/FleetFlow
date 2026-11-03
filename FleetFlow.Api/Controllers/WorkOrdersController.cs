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
    public async Task<IActionResult> GetAll([FromQuery] int? vehicleId)
    {
        var items = await _service.GetWorkOrdersAsync(GetCompanyId());
        return Ok(vehicleId.HasValue ? items.Where(w => w.VehicleId == vehicleId.Value).ToList() : items);
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty;
        public int? AssignedUserId { get; set; }
        public bool Reassign { get; set; }
    }

    private static readonly string[] ValidStatuses = { "Open", "In Progress", "Waiting Parts", "Completed" };

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin,Manager,Technician")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        if (!ValidStatuses.Contains(request.Status)) throw new InvalidOperationException("Invalid status.");
        var companyId = GetCompanyId();
        var existing = await _service.GetWorkOrderByIdAsync(companyId, id);
        if (existing is null) return NotFound();
        var updated = await _service.UpdateWorkOrderAsync(companyId, id, new CreateWorkOrderRequest
        {
            VehicleId = existing.VehicleId,
            AssignedUserId = request.Reassign ? request.AssignedUserId : existing.AssignedUserId,
            Title = existing.Title,
            Description = existing.Description,
            Priority = existing.Priority,
            Status = request.Status
        });
        return updated is null ? NotFound() : Ok(updated);
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
