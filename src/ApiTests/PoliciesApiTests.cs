using InsuranceAutomationDemo.ApiTests.Fixtures;

namespace InsuranceAutomationDemo.ApiTests;

/// <summary>
/// Tests for the Policies API that verify error handling and status codes. Uses the shared ApiTestFixture.
/// Currently contains one test: requesting a nonexistent policy by Id and asserting the API returns 404 Not
/// Found. Requires the API to be running; no specific data is required (we use an Id that is assumed not to exist).
/// </summary>
/// <remarks>
/// The constructor receives ApiTestFixture and stores its ApiClient in _api. The test calls _api.GetPolicyAsync
/// with a high Id (999999) that is assumed not to exist in the database, and asserts the response status is 404.
/// Only the API need be running.
/// </remarks>
public class PoliciesApiTests : IClassFixture<ApiTestFixture>
{
    // Shared ApiClient from the fixture (Shared.Clients), configured with BaseUrl and AuthToken from appsettings.json.
    private readonly ApiClient _api;

    // xUnit passes the fixture into the constructor; we keep a reference to its ApiClient.
    public PoliciesApiTests(ApiTestFixture fixture)
    {
        _api = fixture.ApiClient;
    }

    /// <summary>
    /// Sends GET /policies/999999 (an Id assumed not to exist in the database). Asserts the response status is
    /// 404 Not Found. Verifies that the API returns the correct status for a missing resource instead of 200
    /// (with empty or placeholder data) or 500. Requires only the API to be running.
    /// </summary>
    [Fact]
    public async Task GetPolicy_NonexistentId_Returns404()
    {
        // We use a policy Id that we assume does not exist in the database (e.g. 999999). The API is expected to
        // look up the policy, find nothing, and return 404 Not Found. This test verifies that the API does not
        // return 200 or 500 for a missing resource.
        const int nonexistentId = 999999;

        // GetPolicyAsync is on ApiClient. It sends HTTP GET to "policies/999999" (relative to BaseUrl). The
        // response should be 404 if the API is implemented correctly.
        var response = await _api.GetPolicyAsync(nonexistentId);

        // Assert the response status is exactly 404 Not Found. HttpResponseMessage.StatusCode is the .NET
        // property that holds the HTTP status code of the response.
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}
