using InsuranceAutomationDemo.ApiTests.Fixtures;
using InsuranceAutomationDemo.Shared.Fixtures;
using InsuranceAutomationDemo.Shared.Models;

namespace InsuranceAutomationDemo.ApiTests;

public class QuotesApiTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiClient _api;

    public QuotesApiTests(ApiTestFixture fixture)
    {
        _api = fixture.ApiClient;
    }

    [Fact]
    public async Task CreateQuote_WithValidPayload_ReturnsSuccess()
    {
        var request = QuoteFactory.Create(customerId: 1);

        var response = await _api.PostQuoteAsync(request);

        response.EnsureSuccessStatusCode();
        var quote = await _api.ReadAsJsonAsync<Quote>(response);
        Assert.NotNull(quote);
        Assert.True(quote.Id > 0);
        Assert.Equal(request.CustomerId, quote.CustomerId);
        Assert.Equal(request.Premium, quote.Premium);
    }
}
