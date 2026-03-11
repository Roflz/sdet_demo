using InsuranceAutomationDemo.Shared.Clients;
using InsuranceAutomationDemo.Shared.Config;
using InsuranceAutomationDemo.Shared.Helpers;

namespace InsuranceAutomationDemo.ApiTests.Fixtures;

/// <summary>
/// xUnit fixture that provides a shared ApiClient and TestConfig for all API tests in a class. When a test class
/// declares IClassFixture&lt;ApiTestFixture&gt;, xUnit creates one instance of this class before running any tests
/// in that class and passes it to the class constructor. The fixture loads appsettings.json from the test output
/// directory (via AppContext.BaseDirectory and ConfigLoader in Shared.Helpers), creates one HttpClient and one
/// ApiClient configured with BaseApiUrl and AuthToken from that config, and exposes ApiClient and Config so tests
/// can send requests without repeating setup. IDisposable is implemented for xUnit cleanup; HttpClient is not
/// explicitly disposed here. Used by CustomersApiTests, QuotesApiTests, PoliciesApiTests, and DatabaseValidationTests.
/// </summary>
/// <remarks>
/// The constructor runs once per test class that uses this fixture. It resolves the test output directory,
/// loads TestConfig from appsettings.json, and constructs ApiClient with that config. The test class receives
/// the fixture and uses fixture.ApiClient for all HTTP calls in its [Fact] methods.
/// </remarks>
public class ApiTestFixture : IDisposable
{
    // ApiClient (Shared.Clients) is the type that wraps HttpClient and sends requests to the API. It uses BaseUrl
    // and AuthToken from TestConfig to set the request base address and Authorization header. Exposed so test
    // classes that receive this fixture can use it (e.g. fixture.ApiClient).
    public ApiClient ApiClient { get; }
    // TestConfig (Shared.Config) is the class that holds BaseApiUrl, AuthToken, DatabaseConnectionString, UiBaseUrl.
    // It is populated from appsettings.json. Exposed in case a test needs to read or override a setting.
    public TestConfig Config { get; }

    // xUnit calls this constructor once per test class that declares IClassFixture<ApiTestFixture>. So for
    // CustomersApiTests, QuotesApiTests, etc., xUnit creates exactly one ApiTestFixture and passes it to their
    // constructors. All tests in that class then share this same ApiClient and Config.
    public ApiTestFixture()
    {
        // AppContext.BaseDirectory is the .NET runtime property that returns the directory of the currently
        // executing assembly. When you run "dotnet test" on ApiTests, the executing assembly is the compiled
        // ApiTests.dll, which lives in the build output folder (e.g. src/ApiTests/bin/Debug/net8.0/). The
        // ApiTests.csproj has a rule that copies appsettings.json from the project folder into that output
        // directory (CopyToOutputDirectory PreserveNewest). So at runtime, appsettings.json sits next to
        // ApiTests.dll, and we need BaseDirectory to know where that file is so we can load BaseApiUrl,
        // AuthToken, and DatabaseConnectionString—without those, the tests wouldn't know which API to call or
        // what token to send.
        var basePath = AppContext.BaseDirectory;

        // ConfigLoader (Shared.Helpers) has a static method Load(string basePath). It reads the file
        // Path.Combine(basePath, "appsettings.json"), deserializes the JSON into a TestConfig object (Shared.Config),
        // and returns it. TestConfig has properties: BaseApiUrl, DatabaseConnectionString, UiBaseUrl, AuthToken.
        // We need this so the ApiClient we create next has the correct base URL and token to talk to the real API.
        Config = ConfigLoader.Load(basePath);

        // HttpClient is from System.Net.Http (.NET BCL). It is the object that actually performs HTTP requests.
        // We create one here and pass it to ApiClient; ApiClient will use it for all GET/POST/PATCH calls and will
        // set its BaseAddress and DefaultRequestHeaders (e.g. Authorization) from Config.
        var http = new HttpClient();

        // ApiClient constructor (Shared.Clients) takes the HttpClient and TestConfig. It sets the HttpClient's
        // BaseAddress to Config.BaseApiUrl (e.g. http://localhost:5000/) and, if Config.AuthToken is non-empty,
        // adds DefaultRequestHeaders.Authorization to "Bearer " + AuthToken. So every request sent through this
        // client will go to the configured API URL with the configured token. We assign it to the ApiClient
        // property so test classes that receive this fixture can call _api.PostCustomerAsync(...), etc.
        ApiClient = new ApiClient(http, Config);
    }

    // IDisposable.Dispose() is called by xUnit after all tests in the class have run. GC.SuppressFinalize(this)
    // tells the .NET garbage collector not to run the finalizer for this object. We don't explicitly dispose
    // the HttpClient here (it would normally be disposed when the fixture is no longer used); this just satisfies
    // the IDisposable contract.
    public void Dispose() => GC.SuppressFinalize(this);
}
