using InsuranceAutomationDemo.Shared.Database;
using InsuranceAutomationDemo.Shared.Helpers;

namespace InsuranceAutomationDemo.ApiTests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    public DbHelper Db { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var basePath = AppContext.BaseDirectory;
        var config = ConfigLoader.Load(basePath);
        if (string.IsNullOrEmpty(config.DatabaseConnectionString))
            throw new InvalidOperationException("DatabaseConnectionString is not set in appsettings.json");
        Db = new DbHelper(config.DatabaseConnectionString);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (Db != null)
            await Db.DisposeAsync();
    }
}
