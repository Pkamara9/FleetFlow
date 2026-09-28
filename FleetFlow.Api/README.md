using FleetFlow.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly IBillingService _service;

    public BillingController(IBillingService service)
    {
        _service = service;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] BillingRequest request)
    {
        var result = await _service.CreateCheckoutSessionAsync(request.PlanName, request.CustomerEmail, request.SuccessUrl, request.CancelUrl);
        return Ok(result);
    }
}

public class BillingRequest
{
    public string PlanName { get; set; } = "Starter";
    public string CustomerEmail { get; set; } = string.Empty;
    public string SuccessUrl { get; set; } = "http://localhost:5173/billing/success";
    public string CancelUrl { get; set; } = "http://localhost:5173/billing/cancel";
}

