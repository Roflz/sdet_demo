using InsuranceAutomationDemo.Shared.Clients;
using InsuranceAutomationDemo.Shared.Config;
using InsuranceAutomationDemo.Shared.Helpers;

namespace InsuranceAutomationDemo.ApiTests.Fixtures;

public class ApiTestFixture : IDisposable
{
    public ApiClient ApiClient { get; }
    public TestConfig Config { get; }

    public ApiTestFixture()
    {
        var basePath = AppContext.BaseDirectory;
        Config = ConfigLoader.Load(basePath);
        var http = new HttpClient();
        ApiClient = new ApiClient(http, Config);
    }

    public void Dispose() => GC.SuppressFinalize(this);
}
