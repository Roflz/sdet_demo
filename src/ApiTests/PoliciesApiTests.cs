using InsuranceAutomationDemo.ApiTests.Fixtures;

namespace InsuranceAutomationDemo.ApiTests;

public class PoliciesApiTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiClient _api;

    public PoliciesApiTests(ApiTestFixture fixture)
    {
        _api = fixture.ApiClient;
    }

    [Fact]
    public async Task GetPolicy_NonexistentId_Returns404()
    {
        const int nonexistentId = 999999;

        var response = await _api.GetPolicyAsync(nonexistentId);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}
