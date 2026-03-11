namespace InsuranceAutomationDemo.Shared.Config;

/// <summary>
/// Test configuration loaded from appsettings.json.
/// </summary>
public class TestConfig
{
    public string BaseApiUrl { get; set; } = "https://localhost:5001";
    public string DatabaseConnectionString { get; set; } = "";
    public string UiBaseUrl { get; set; } = "https://localhost:5000";
    public string AuthToken { get; set; } = "";
}
