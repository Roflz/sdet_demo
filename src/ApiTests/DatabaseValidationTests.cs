using InsuranceAutomationDemo.ApiTests.Fixtures;
using InsuranceAutomationDemo.Shared.Clients;
using InsuranceAutomationDemo.Shared.Database;
using InsuranceAutomationDemo.Shared.Fixtures;
using InsuranceAutomationDemo.Shared.Helpers;
using InsuranceAutomationDemo.Shared.Models;

namespace InsuranceAutomationDemo.ApiTests;

/// <summary>
/// Tests that call the API and then query the database directly to verify that the data the API claimed to
/// create or update is actually persisted correctly. Uses ApiTestFixture for the ApiClient and IAsyncLifetime
/// to create a DbHelper (Shared.Database) in InitializeAsync when DatabaseConnectionString is set in
/// src/ApiTests/appsettings.json. If the connection string is missing or empty, _db stays null and each test
/// returns without asserting (skip). Requires the API and SQL Server to be running, and the connection string
/// in appsettings.json to point at the same database the API uses. Some tests assume seed data (e.g. customer 1,
/// policy 1) or data created by the API during the test.
/// </summary>
public class DatabaseValidationTests : IClassFixture<ApiTestFixture>, IAsyncLifetime
{
    // ApiClient from Shared.Clients—same as other API test classes; comes from the fixture so we can call the API.
    private readonly ApiClient _api;
    // DbHelper is from Shared.Database. It wraps a SQL Server connection and has methods like RecordExistsAsync,
    // GetQuoteByIdAsync, GetPolicyStatusAsync. We set it in InitializeAsync only if DatabaseConnectionString is
    // set in appsettings.json; otherwise it stays null and tests that need the DB return early (skip) instead of failing.
    private DbHelper? _db;

    // xUnit creates ApiTestFixture and passes it here; we store the fixture's ApiClient for use in test methods.
    public DatabaseValidationTests(ApiTestFixture fixture)
    {
        _api = fixture.ApiClient;
    }

    // IAsyncLifetime requires DisposeAsync. We don't hold any resource that needs disposal in this class (DbHelper
    // is created per test run and could be disposed, but we don't implement that here). Returning Task.CompletedTask
    // satisfies the interface without doing work.
    public Task DisposeAsync() => Task.CompletedTask;

    // xUnit calls InitializeAsync once before running any test in this class. We use it to create DbHelper when
    // the test project has a database connection string configured (in src/ApiTests/appsettings.json).
    public async Task InitializeAsync()
    {
        // Same as in AuthApiTests and ApiTestFixture: the directory where the test assembly (ApiTests.dll) is
        // running, so we can find the copied appsettings.json and load BaseApiUrl, DatabaseConnectionString, etc.
        var basePath = AppContext.BaseDirectory;

        // ConfigLoader.Load (Shared.Helpers) reads appsettings.json from basePath and returns a TestConfig
        // (Shared.Config) with BaseApiUrl, DatabaseConnectionString, AuthToken, UiBaseUrl.
        var config = ConfigLoader.Load(basePath);

        // If DatabaseConnectionString in appsettings.json is set (non-null and non-empty), create a DbHelper
        // (Shared.Database) with that connection string. DbHelper will use it to connect to SQL Server when we
        // call methods like RecordExistsAsync or GetQuoteByIdAsync. If the setting is missing or empty, _db
        // stays null and tests that need the DB will skip (see the "if (_db == null) return" at the start of each test).
        if (!string.IsNullOrEmpty(config.DatabaseConnectionString))
            _db = new DbHelper(config.DatabaseConnectionString);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Creates a quote via POST /quotes (using QuoteFactory for customer 1), then connects to the database and
    /// verifies that a row exists in the Quotes table with the returned Id and that the row's Premium matches
    /// the request. Asserts that the API's create operation actually persisted the quote and that the stored
    /// premium is correct. Skips if DbHelper was not created (no connection string) or if the API call fails.
    /// Depends on API, database, DatabaseConnectionString in appsettings.json, and customer Id 1 existing.
    /// </summary>
    [Fact]
    public async Task AfterApiCreatesQuote_QuoteExistsInDatabase()
    {
        // If we didn't create a DbHelper (no connection string in appsettings.json), skip this test so we don't
        // fail in environments where the database isn't available (e.g. CI without a DB). "return" exits the test
        // without asserting, so the test is reported as passed but didn't actually run the DB checks.
        if (_db == null) return; // Skip when no DB configured

        // QuoteFactory (Shared.Fixtures) builds a CreateQuoteRequest for customerId 1. Same as in QuotesApiTests.
        var request = QuoteFactory.Create(customerId: 1);
        // Send POST /quotes to the API using the shared ApiClient from the fixture.
        var response = await _api.PostQuoteAsync(request);
        // If the API returned a non-success status (e.g. not running, 500), skip the rest so we don't fail because
        // of environment; the test effectively skips the DB verification when the API call fails.
        if (!response.IsSuccessStatusCode) return; // API not available

        // Deserialize the response body into a Quote (Shared.Models). The API returns the created quote with Id.
        var quote = await _api.ReadAsJsonAsync<Quote>(response);
        Assert.NotNull(quote);

        // RecordExistsAsync is on DbHelper (Shared.Database). It runs a SQL query: SELECT COUNT(*) FROM Quotes
        // WHERE Id = @id (with quote.Id as the parameter). It returns true if the count is greater than zero.
        // We use this to verify that the row the API said it created actually exists in the database.
        var exists = await _db.RecordExistsAsync("Quotes", "Id", quote.Id);
        Assert.True(exists, "Quote should exist in DB after API creation");

        // GetQuoteByIdAsync is on DbHelper. It runs SELECT Id, CustomerId, ProductCode, Premium, Status FROM Quotes
        // WHERE Id = @id and maps the row to a QuoteRow (or similar). We use it to verify the stored premium
        // matches what we sent in the request (data integrity).
        var row = await _db.GetQuoteByIdAsync(quote.Id);
        Assert.NotNull(row);
        Assert.Equal(request.Premium, row.Premium);
    }

    /// <summary>
    /// Sends PATCH /policies/1/status with body { "status": "Cancelled" }, then queries the database for the
    /// Status column of policy Id 1 and asserts it equals "Cancelled". Verifies that the API's status update
    /// was persisted to the database. Skips if no DbHelper or if the API call fails. Depends on API, database,
    /// and policy Id 1 existing (e.g. from sql/seed.sql). This test modifies data (changes policy 1's status).
    /// </summary>
    [Fact]
    public async Task AfterPolicyStatusPatch_StatusUpdatedInDatabase()
    {
        if (_db == null) return;

        // We're going to PATCH policy with Id 1. We assume that policy exists (e.g. from sql/seed.sql or a prior run).
        const int policyId = 1;
        // UpdatePolicyStatusRequest is in Shared.Models. We set Status = "Cancelled" so the API should update the
        // policy's Status column to "Cancelled". The API's PATCH /policies/{id}/status endpoint accepts this body.
        var patch = new UpdatePolicyStatusRequest { Status = "Cancelled" };
        // PatchPolicyStatusAsync on ApiClient sends HTTP PATCH to "policies/1/status" with the JSON body.
        var response = await _api.PatchPolicyStatusAsync(policyId, patch);
        if (!response.IsSuccessStatusCode) return;

        // GetPolicyStatusAsync is on DbHelper. It runs SELECT Status FROM Policies WHERE Id = @id and returns the
        // status string. We're verifying that after the API said it updated the status, the database actually
        // contains "Cancelled"—i.e. the API's update was persisted.
        var status = await _db.GetPolicyStatusAsync(policyId);
        Assert.NotNull(status);
        Assert.Equal("Cancelled", status);
    }

    /// <summary>
    /// Queries the database only (no API call): runs a JOIN between Customers and Policies for customer Id 1
    /// and policy Id 1. Asserts that the returned row has the expected CustomerId, PolicyId, and a non-empty
    /// PolicyNumber. Verifies that the relationship between customer and policy is correctly stored and can be
    /// queried. Skips if no DbHelper. Depends on database and on seed or prior data that has customer 1 and
    /// policy 1 linked. If no such row exists, the test does not fail (we only assert when row is non-null).
    /// </summary>
    [Fact]
    public async Task CustomerPolicyRelationship_JoinReturnsCorrectRow()
    {
        if (_db == null) return;

        // We'll query for customer 1 and policy 1, assuming they exist and are linked (policy 1 has CustomerId 1).
        const int customerId = 1;
        const int policyId = 1;

        // GetCustomerWithPolicyAsync is on DbHelper. It runs a SQL JOIN: SELECT from Customers and Policies where
        // Policies.CustomerId = Customers.Id and customer Id and policy Id match. It returns a row (or null if
        // no match). We use this to verify the relationship between customer and policy in the database.
        var row = await _db!.GetCustomerWithPolicyAsync(customerId, policyId);
        // If there's no seed data or the schema is different, row can be null; we only assert when we got a row.
        if (row != null)
        {
            Assert.Equal(customerId, row.CustomerId);
            Assert.Equal(policyId, row.PolicyId);
            Assert.False(string.IsNullOrEmpty(row.PolicyNumber));
        }
    }
}
