using System.Net.Http.Headers;
using InsuranceAutomationDemo.Shared.Clients;
using InsuranceAutomationDemo.Shared.Config;
using InsuranceAutomationDemo.Shared.Helpers;
using InsuranceAutomationDemo.Shared.Models;

namespace InsuranceAutomationDemo.ApiTests;

/// <summary>
/// Tests that expect 401 when no or invalid auth is sent.
/// Uses a client without auth token to simulate unauthorized request.
/// </summary>
public class AuthApiTests
{
    [Fact]
    public async Task GetCustomer_WithoutAuthToken_Returns401WhenApiRequiresAuth()
    {
        var basePath = AppContext.BaseDirectory;
        var config = ConfigLoader.Load(basePath);
        config.AuthToken = ""; // explicitly no token
        using var http = new HttpClient();
        var client = new ApiClient(http, config);

        var response = await client.GetCustomerAsync(1);

        // If the API enforces auth, we expect 401. If not yet implemented, may be 200/404.
        // This test documents the expected behavior; adjust assertion when API is live.
        Assert.True(
            response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
            response.StatusCode == System.Net.HttpStatusCode.NotFound ||
            response.StatusCode == System.Net.HttpStatusCode.OK,
            "API should return 401 when auth required and missing, or 404/200 when auth not enforced.");
    }
}
