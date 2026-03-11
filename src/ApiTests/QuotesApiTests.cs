using InsuranceAutomationDemo.ApiTests.Fixtures;
using InsuranceAutomationDemo.Shared.Fixtures;
using InsuranceAutomationDemo.Shared.Models;

namespace InsuranceAutomationDemo.ApiTests;

/// <summary>
/// Tests for the Quotes API: creating quotes (POST /quotes) and verifying the API returns the created quote
/// with the correct Id, CustomerId, and Premium. Uses the shared ApiTestFixture for a single ApiClient.
/// Requires the API to be running and at least one customer to exist (e.g. Id 1 from sql/seed.sql), since
/// creating a quote requires a valid CustomerId.
/// </summary>
/// <remarks>
/// The constructor receives ApiTestFixture and stores its ApiClient in _api. The single test builds a
/// CreateQuoteRequest for customer 1 via QuoteFactory, sends POST /quotes with _api, and asserts on the
/// response and deserialized Quote. Customer Id 1 must exist in the database.
/// </remarks>
public class QuotesApiTests : IClassFixture<ApiTestFixture>
{
    // The shared ApiClient (from Shared.Clients) that the fixture created. It already has BaseUrl and
    // AuthToken set from appsettings.json, so we just use it to send requests.
    private readonly ApiClient _api;

    // xUnit injects the fixture when it creates this test class; we store the fixture's ApiClient.
    public QuotesApiTests(ApiTestFixture fixture)
    {
        _api = fixture.ApiClient;
    }

    /// <summary>
    /// Builds a valid create-quote request for customer 1 via QuoteFactory, sends POST /quotes, and asserts the
    /// response is successful, the body deserializes to a Quote with a positive Id, and CustomerId and Premium
    /// match the request. Verifies that the API accepts valid quote input and returns the created resource.
    /// Depends on the API, a writable database, and the existence of customer Id 1.
    /// </summary>
    [Fact]
    public async Task CreateQuote_WithValidPayload_ReturnsSuccess()
    {
        // QuoteFactory is in Shared.Fixtures. Create(customerId: 1) returns a CreateQuoteRequest (Shared.Models)
        // with CustomerId = 1, and default ProductCode and Premium from the factory. We pass customerId: 1
        // because the API requires a valid customer to create a quote; we assume customer with Id 1 exists
        // (e.g. from running sql/seed.sql or from a previous test that created a customer).
        var request = QuoteFactory.Create(customerId: 1);

        // PostQuoteAsync is on ApiClient. It sends HTTP POST to "quotes" with the request body as JSON and
        // the Authorization header from config. Returns the raw HttpResponseMessage.
        var response = await _api.PostQuoteAsync(request);

        // Throw if status is not 2xx, so we fail fast if the API returned an error.
        response.EnsureSuccessStatusCode();

        // Read the response body and deserialize it into a Quote (Shared.Models): Id, CustomerId, ProductCode,
        // Premium, Status. We need this to assert the API returned the created quote with the right values.
        var quote = await _api.ReadAsJsonAsync<Quote>(response);

        Assert.NotNull(quote);
        // Id > 0 means the API assigned a real Id (record was inserted).
        Assert.True(quote.Id > 0);
        // Confirm the API returned the same CustomerId and Premium we sent.
        Assert.Equal(request.CustomerId, quote.CustomerId);
        Assert.Equal(request.Premium, quote.Premium);
    }
}
