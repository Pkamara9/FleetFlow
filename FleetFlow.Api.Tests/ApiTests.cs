using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace FleetFlow.Api.Tests;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiTests(WebApplicationFactory<Program> factory)
    {
        // Program reads configuration eagerly, so use an environment variable.
        Environment.SetEnvironmentVariable("Database__UseInMemory", "true");
        _factory = factory;
    }

    private static async Task<JsonElement> LoginAsync(HttpClient client, string email = "manager@fleetflow.com", string password = "Manager123!")
    {
        var res = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<JsonElement>();
    }

    [Fact]
    public async Task Login_with_bad_password_returns_401()
    {
        var client = _factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/login", new { email = "manager@fleetflow.com", password = "nope" });
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Login_refresh_and_me_work()
    {
        var client = _factory.CreateClient();
        var auth = await LoginAsync(client);

        var refresh = await client.PostAsJsonAsync("/api/auth/refresh", new
        {
            accessToken = auth.GetProperty("accessToken").GetString(),
            refreshToken = auth.GetProperty("refreshToken").GetString()
        });
        refresh.EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.GetProperty("accessToken").GetString());
        var me = await client.GetFromJsonAsync<JsonElement>("/api/auth/me");
        Assert.Equal("Manager", me.GetProperty("role").GetString());
    }

    [Fact]
    public async Task Work_order_status_can_be_patched_and_dashboard_loads()
    {
        var client = _factory.CreateClient();
        var auth = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.GetProperty("accessToken").GetString());

        var orders = await client.GetFromJsonAsync<JsonElement>("/api/workorders");
        var id = orders[0].GetProperty("workOrderId").GetInt32();

        var patch = await client.PatchAsJsonAsync($"/api/workorders/{id}/status", new { status = "Waiting Parts" });
        patch.EnsureSuccessStatusCode();
        var updated = await patch.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Waiting Parts", updated.GetProperty("status").GetString());

        var bad = await client.PatchAsJsonAsync($"/api/workorders/{id}/status", new { status = "Bogus" });
        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);

        var dash = await client.GetFromJsonAsync<JsonElement>("/api/reports/dashboard");
        Assert.Equal(6, dash.GetProperty("maintenanceCostChart").GetArrayLength());
    }

    [Fact]
    public async Task Unauthenticated_requests_are_rejected()
    {
        var res = await _factory.CreateClient().GetAsync("/api/vehicles");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }
}
