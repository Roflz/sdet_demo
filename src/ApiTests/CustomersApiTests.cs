using InsuranceAutomationDemo.ApiTests.Fixtures;
using InsuranceAutomationDemo.Shared.Fixtures;
using InsuranceAutomationDemo.Shared.Models;

namespace InsuranceAutomationDemo.ApiTests;

/// <summary>
/// Tests for the Customers API: creating customers (POST /customers) and validating that the API accepts
/// valid payloads and rejects invalid ones. Uses the shared ApiTestFixture so all tests in this class use the
/// same ApiClient (configured with BaseApiUrl and AuthToken from src/ApiTests/appsettings.json). Requires the
/// API to be running. The API must validate required fields (e.g. FirstName, LastName, Email) and return 400
/// when they are missing or invalid.
/// </summary>
/// <remarks>
/// xUnit injects ApiTestFixture into the constructor; the class stores the fixture's ApiClient in _api. Each
/// test builds a request (via CustomerFactory), sends it with _api.PostCustomerAsync, and asserts on the
/// response status and body. The API and a writable database are required for the success test.
/// </remarks>
public class CustomersApiTests : IClassFixture<ApiTestFixture>
{
    // ApiClient is from Shared.Clients. It is the wrapper that sends HTTP requests to the API (using BaseUrl and
    // AuthToken from config). We store the one that ApiTestFixture created so every test method in this class
    // can use it. "readonly" means it is set only in the constructor (by the fixture injection from xUnit).
    private readonly ApiClient _api;

    // xUnit discovers test classes and creates one instance per test class. When the class has IClassFixture<ApiTestFixture>,
    // xUnit creates ApiTestFixture first, then calls this constructor and passes that fixture. We assign the fixture's
    // ApiClient property to _api so all tests in this class use the same client.
    public CustomersApiTests(ApiTestFixture fixture)
    {
        _api = fixture.ApiClient;
    }

    /// <summary>
    /// Builds a valid create-customer request via CustomerFactory, sends POST /customers, and asserts the response
    /// is successful (2xx), the body deserializes to a Customer with a positive Id, and FirstName, LastName, and
    /// Email match the request. Verifies that the API accepts valid input and returns the created resource with
    /// the correct data. Depends on the API and a writable database (the API inserts into the Customers table).
    /// </summary>
    [Fact]
    public async Task CreateCustomer_WithValidPayload_ReturnsSuccess()
    {
        // CustomerFactory is in Shared.Fixtures. Create() returns a CreateCustomerRequest (Shared.Models) with
        // valid FirstName, LastName, Email, and optional Phone. The factory uses an internal counter so each call
        // gets a unique email (e.g. test1@example.com, test2@example.com), which avoids duplicate-key errors if
        // we run multiple tests or the same test twice. We use this so we don't have to manually construct the
        // request object and so the data is always in a valid shape for the API.
        var request = CustomerFactory.Create();

        // PostCustomerAsync is on ApiClient (Shared.Clients). It serializes the request to JSON and sends HTTP POST
        // to the path "customers" (relative to BaseUrl from appsettings.json). The client adds the Authorization
        // header from config. The method returns the raw HttpResponseMessage so we can check status and body.
        var response = await _api.PostCustomerAsync(request);

        // EnsureSuccessStatusCode is a method on HttpResponseMessage (.NET). It throws an exception if the response
        // status code is not in the 2xx range (e.g. 400, 404, 500). We use it so the test fails immediately with
        // a clear error if the API returned an error status, instead of continuing and failing later on a null
        // or unexpected body.
        response.EnsureSuccessStatusCode();

        // ReadAsJsonAsync is on ApiClient (Shared.Clients). It reads the response body stream, parses it as JSON,
        // and deserializes it into an instance of the type we pass (Customer, from Shared.Models). Customer has Id,
        // FirstName, LastName, Email, Phone—the shape the API returns. We need this to assert that the API
        // returned the correct data and assigned an Id.
        var customer = await _api.ReadAsJsonAsync<Customer>(response);

        // Assert.NotNull is from xUnit. It fails the test if customer is null (e.g. empty response or deserialize
        // failure). The API is expected to return the created customer in the response body.
        Assert.NotNull(customer);

        // Assert.True fails the test if the condition is false. The API is expected to assign a new Id when it
        // inserts the customer into the database; Id > 0 confirms the record was actually created and we're not
        // seeing a default or placeholder value.
        Assert.True(customer.Id > 0);

        // Assert.Equal is from xUnit. It compares the two values and fails if they are not equal. We're verifying
        // that the API echoed back the same FirstName, LastName, and Email we sent in the request—i.e. the API
        // stored and returned our data correctly.
        Assert.Equal(request.FirstName, customer.FirstName);
        Assert.Equal(request.LastName, customer.LastName);
        Assert.Equal(request.Email, customer.Email);
    }

    /// <summary>
    /// Builds a create-customer request with an empty Email (invalid). Sends POST /customers and asserts the
    /// response status is 400 Bad Request. Verifies that the API validates required fields and rejects invalid
    /// input instead of accepting it or returning 500. Depends on the API enforcing validation on at least one
    /// required field (e.g. Email).
    /// </summary>
    [Fact]
    public async Task CreateCustomer_MissingRequiredField_Returns400()
    {
        // CustomerFactory.Create() accepts an optional callback. We pass "r => r.Email = """, which runs after the
        // factory sets default values: it sets the Email property of the request to empty string. The API is
        // expected to require a non-empty Email (and possibly other fields) and return 400 Bad Request when
        // validation fails. We build this invalid request to assert that the API validates input.
        var request = CustomerFactory.Create(r => r.Email = "");

        // Same as in the success test: send POST /customers with this (invalid) body. We do not call
        // EnsureSuccessStatusCode because we expect a 400 response.
        var response = await _api.PostCustomerAsync(request);

        // Assert that the response status is exactly 400 Bad Request. System.Net.HttpStatusCode is the .NET
        // enum for HTTP status codes. This confirms the API rejected the request due to invalid/missing data
        // instead of accepting it or returning a different error (e.g. 500).
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
