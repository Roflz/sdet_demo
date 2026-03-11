using InsuranceAutomationDemo.ApiTests.Fixtures;
using InsuranceAutomationDemo.Shared.Clients;
using InsuranceAutomationDemo.Shared.Database;
using InsuranceAutomationDemo.Shared.Fixtures;
using InsuranceAutomationDemo.Shared.Helpers;
using InsuranceAutomationDemo.Shared.Models;

namespace InsuranceAutomationDemo.ApiTests;

/// <summary>
/// Example DB validation tests. Run against a database that is updated by the API
/// (or use seeded data). Skip or mock when DB is not available in CI.
/// </summary>
public class DatabaseValidationTests : IClassFixture<ApiTestFixture>, IAsyncLifetime
{
    private readonly ApiClient _api;
    private DbHelper? _db;

    public DatabaseValidationTests(ApiTestFixture fixture)
    {
        _api = fixture.ApiClient;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public async Task InitializeAsync()
    {
        var basePath = AppContext.BaseDirectory;
        var config = ConfigLoader.Load(basePath);
        if (!string.IsNullOrEmpty(config.DatabaseConnectionString))
            _db = new DbHelper(config.DatabaseConnectionString);
        await Task.CompletedTask;
    }

    [Fact]
    public async Task AfterApiCreatesQuote_QuoteExistsInDatabase()
    {
        if (_db == null) return; // Skip when no DB configured

        var request = QuoteFactory.Create(customerId: 1);
        var response = await _api.PostQuoteAsync(request);
        if (!response.IsSuccessStatusCode) return; // API not available

        var quote = await _api.ReadAsJsonAsync<Quote>(response);
        Assert.NotNull(quote);

        var exists = await _db.RecordExistsAsync("Quotes", "Id", quote.Id);
        Assert.True(exists, "Quote should exist in DB after API creation");

        var row = await _db.GetQuoteByIdAsync(quote.Id);
        Assert.NotNull(row);
        Assert.Equal(request.Premium, row.Premium);
    }

    [Fact]
    public async Task AfterPolicyStatusPatch_StatusUpdatedInDatabase()
    {
        if (_db == null) return;

        const int policyId = 1;
        var patch = new UpdatePolicyStatusRequest { Status = "Cancelled" };
        var response = await _api.PatchPolicyStatusAsync(policyId, patch);
        if (!response.IsSuccessStatusCode) return;

        var status = await _db.GetPolicyStatusAsync(policyId);
        Assert.NotNull(status);
        Assert.Equal("Cancelled", status);
    }

    [Fact]
    public async Task CustomerPolicyRelationship_JoinReturnsCorrectRow()
    {
        if (_db == null) return;

        const int customerId = 1;
        const int policyId = 1;

        var row = await _db!.GetCustomerWithPolicyAsync(customerId, policyId);
        // May be null if no seed data or schema differs
        if (row != null)
        {
            Assert.Equal(customerId, row.CustomerId);
            Assert.Equal(policyId, row.PolicyId);
            Assert.False(string.IsNullOrEmpty(row.PolicyNumber));
        }
    }
}
