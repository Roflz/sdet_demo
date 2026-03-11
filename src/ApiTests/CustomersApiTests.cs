using InsuranceAutomationDemo.ApiTests.Fixtures;
using InsuranceAutomationDemo.Shared.Fixtures;
using InsuranceAutomationDemo.Shared.Models;

namespace InsuranceAutomationDemo.ApiTests;

public class CustomersApiTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiClient _api;

    public CustomersApiTests(ApiTestFixture fixture)
    {
        _api = fixture.ApiClient;
    }

    [Fact]
    public async Task CreateCustomer_WithValidPayload_ReturnsSuccess()
    {
        var request = CustomerFactory.Create();

        var response = await _api.PostCustomerAsync(request);

        response.EnsureSuccessStatusCode();
        var customer = await _api.ReadAsJsonAsync<Customer>(response);
        Assert.NotNull(customer);
        Assert.True(customer.Id > 0);
        Assert.Equal(request.FirstName, customer.FirstName);
        Assert.Equal(request.LastName, customer.LastName);
        Assert.Equal(request.Email, customer.Email);
    }

    [Fact]
    public async Task CreateCustomer_MissingRequiredField_Returns400()
    {
        var request = CustomerFactory.Create(r => r.Email = "");

        var response = await _api.PostCustomerAsync(request);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
