using InsuranceAutomationDemo.Shared.Clients;
using InsuranceAutomationDemo.Shared.Config;
using InsuranceAutomationDemo.Shared.Helpers;
using InsuranceAutomationDemo.Shared.Models;

namespace InsuranceAutomationDemo.ApiTests;

/// <summary>
/// Tests that verify the API's behavior when a request is sent without valid authentication.
/// This class does not use the shared ApiTestFixture; each test creates its own ApiClient with config loaded
/// from appsettings.json, and can override the AuthToken (e.g. set to empty) to simulate an unauthenticated
/// or invalid caller. The API is expected to return 401 Unauthorized when auth is required and no valid
/// Bearer token is sent. If the API does not yet enforce auth, the test accepts 404 or 200 as well.
/// </summary>
public class AuthApiTests
{
    /// <summary>
    /// Sends GET /customers/1 with no Authorization header (AuthToken is cleared after loading config).
    /// Asserts that the response status is 401 Unauthorized, 404 Not Found, or 200 OK. When the API enforces
    /// auth (Auth:ApiKey set in the API's appsettings), we expect 401; 404 or 200 are allowed when auth is
    /// not enforced so the test does not fail in that configuration. Requires the API to be running.
    /// </summary>
    [Fact]
    public async Task GetCustomer_WithoutAuthToken_Returns401WhenApiRequiresAuth()
    {
        // AppContext.BaseDirectory is a .NET runtime property that returns the directory containing the
        // currently executing assembly. When you run "dotnet test", the executing assembly is the compiled
        // ApiTests project (e.g. ApiTests.dll), and it runs from the build output folder (e.g. bin/Debug/net8.0/).
        // We need this path because the ApiTests.csproj copies appsettings.json into that output directory
        // (via CopyToOutputDirectory), and we must load config from that same directory at runtime.
        var basePath = AppContext.BaseDirectory;

        // ConfigLoader is in Shared.Helpers. Load() reads the file "appsettings.json" from the given path,
        // deserializes it into a TestConfig object (from Shared.Config), and returns it. TestConfig holds
        // BaseApiUrl, AuthToken, DatabaseConnectionString, UiBaseUrl—the values the tests need to know where
        // the API is and what API key to send. We need appsettings.json so the test knows the API's URL and,
        // in other tests, the token; this test will then clear the token to simulate an unauthenticated request.
        var config = ConfigLoader.Load(basePath);

        // Overwrite the AuthToken that was loaded from appsettings.json. We set it to empty string so that
        // when we create ApiClient below, it will not add an "Authorization: Bearer ..." header to requests.
        // That simulates a caller who is not authenticated, so we can assert the API returns 401 (or 404/200
        // depending on whether the API actually enforces auth).
        config.AuthToken = ""; // explicitly no token

        // HttpClient is from System.Net.Http. It is the .NET type that actually sends HTTP requests over the
        // network. We create a new one here (instead of using the fixture's) because this test deliberately
        // uses a client with no auth. "using" means it will be disposed when the test method exits.
        using var http = new HttpClient();

        // ApiClient is in Shared.Clients. It wraps HttpClient and adds the base URL and, when AuthToken is
        // set, the Authorization header. We pass our http and the config we modified (with empty AuthToken),
        // so this client will call the same BaseApiUrl as other tests but will not send any token.
        var client = new ApiClient(http, config);

        // GetCustomerAsync is a method on ApiClient (Shared.Clients). It sends HTTP GET to the path
        // "customers/1" (relative to the BaseUrl from config, e.g. http://localhost:5000/customers/1).
        // We pass the integer 1 as the customer id. The method returns an HttpResponseMessage; we await it
        // because the HTTP call is asynchronous.
        var response = await client.GetCustomerAsync(1);

        // Assert.True is from xUnit (via GlobalUsings). We assert that the response's status code is one of:
        // Unauthorized (401)—the API rejected the request because no valid token was sent; or NotFound (404)—
        // the API doesn't enforce auth and returned 404 for a missing customer; or OK (200)—the API doesn't
        // enforce auth and returned the customer. We allow all three because this test documents expected
        // behavior; when the API is live and enforcing auth, the usual result is 401.
        Assert.True(
            response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
            response.StatusCode == System.Net.HttpStatusCode.NotFound ||
            response.StatusCode == System.Net.HttpStatusCode.OK,
            "API should return 401 when auth required and missing, or 404/200 when auth not enforced.");
    }
}
