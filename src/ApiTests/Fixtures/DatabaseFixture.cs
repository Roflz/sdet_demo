using InsuranceAutomationDemo.Shared.Database;
using InsuranceAutomationDemo.Shared.Helpers;

namespace InsuranceAutomationDemo.ApiTests.Fixtures;

/// <summary>
/// Optional xUnit fixture that provides a shared DbHelper (Shared.Database) for test classes that need to run
/// SQL against the same database the API uses. Implements IAsyncLifetime so xUnit calls InitializeAsync() before
/// running tests; in InitializeAsync we load appsettings.json from the test output directory and create a DbHelper
/// with the DatabaseConnectionString from that file. If the connection string is missing or empty, we throw so
/// the test run fails with a clear message. The DbHelper exposes methods like RecordExistsAsync, GetQuoteByIdAsync,
/// GetPolicyStatusAsync, and GetCustomerWithPolicyAsync for verifying persisted data. DisposeAsync cleans up the
/// DbHelper. This fixture is not currently used by DatabaseValidationTests (they create their own DbHelper in
/// their class's InitializeAsync); it is available for other test classes that want a shared DbHelper via
/// IClassFixture&lt;DatabaseFixture&gt;.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    // DbHelper (Shared.Database) is a class that takes a SQL Server connection string and provides methods like
    // ExecuteScalarAsync, RecordExistsAsync, GetQuoteByIdAsync, GetPolicyStatusAsync. It opens a connection when
    // you call those methods. We set it in InitializeAsync so any test class that uses this fixture can run
    // database queries. "= null!" tells the compiler we'll set it before use (in InitializeAsync).
    public DbHelper Db { get; private set; } = null!;

    // IAsyncLifetime is an xUnit interface. When a test class uses IClassFixture<DatabaseFixture> and
    // IAsyncLifetime, xUnit creates the DatabaseFixture and then calls InitializeAsync() on it before running
    // any tests. We use this to load config and create DbHelper once, so tests don't each have to do it.
    public async Task InitializeAsync()
    {
        // Same as ApiTestFixture: the directory where the test assembly runs (e.g. bin/Debug/net8.0/), so we can
        // find appsettings.json. We need appsettings.json because it contains DatabaseConnectionString—the connection
        // string (server, database name, user, password) that DbHelper needs to connect to SQL Server. Without it
        // we cannot run any database queries.
        var basePath = AppContext.BaseDirectory;

        // ConfigLoader.Load (Shared.Helpers) reads appsettings.json from basePath and returns TestConfig. TestConfig
        // has a property DatabaseConnectionString (the value from the "DatabaseConnectionString" key in the JSON).
        var config = ConfigLoader.Load(basePath);

        // If DatabaseConnectionString was not set in appsettings.json (null or empty), we throw so the test run
        // fails immediately with a clear message instead of failing later when a test tries to use Db and gets a
        // null reference or connection error. This way the author knows they must set the connection string to
        // run tests that need the database.
        if (string.IsNullOrEmpty(config.DatabaseConnectionString))
            throw new InvalidOperationException("DatabaseConnectionString is not set in appsettings.json");

        // DbHelper (Shared.Database) constructor takes the connection string. It stores it and uses it to create
        // a SqlConnection (Microsoft.Data.SqlClient) when one of its methods (e.g. RecordExistsAsync) is called.
        // We assign it to the Db property so test classes that receive this fixture can call fixture.Db.RecordExistsAsync(...).
        Db = new DbHelper(config.DatabaseConnectionString);
        await Task.CompletedTask;
    }

    // IAsyncLifetime also requires DisposeAsync. We implement it so that when the fixture is disposed, we call
    // DbHelper.DisposeAsync() (or the appropriate cleanup). DbHelper implements IAsyncDisposable and can release
    // any resources; calling it here ensures we don't leave connections open after the test class is done.
    public async Task DisposeAsync()
    {
        if (Db != null)
            await Db.DisposeAsync();
    }
}
