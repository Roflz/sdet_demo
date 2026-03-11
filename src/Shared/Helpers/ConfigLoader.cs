using System.Text.Json;
using InsuranceAutomationDemo.Shared.Config;

namespace InsuranceAutomationDemo.Shared.Helpers;

public static class ConfigLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static TestConfig Load(string basePath)
    {
        var path = Path.Combine(basePath, "appsettings.json");
        if (!File.Exists(path))
            return new TestConfig();

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<TestConfig>(json, JsonOptions) ?? new TestConfig();
    }
}
