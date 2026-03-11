using InsuranceAutomationDemo.Shared.Helpers;
using InsuranceAutomationDemo.UiTests.Helpers;
using InsuranceAutomationDemo.UiTests.Pages;

namespace InsuranceAutomationDemo.UiTests;

public class QuoteSmokeTests : IDisposable
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl;

    public QuoteSmokeTests()
    {
        _driver = WebDriverFactory.CreateChrome();
        var config = ConfigLoader.Load(AppContext.BaseDirectory);
        _baseUrl = config.UiBaseUrl;
    }

    [Fact]
    public void QuotePage_LoadsWithoutError()
    {
        var page = new QuotePage(_driver, _baseUrl);
        page.GoTo();
        // If we get here without throwing, page load succeeded
        Assert.NotNull(_driver.Title);
    }

    [Fact]
    public void LoginPage_LoadsWithoutError()
    {
        var page = new LoginPage(_driver, _baseUrl);
        page.GoTo();
        Assert.NotNull(_driver.Title);
    }

    public void Dispose() => _driver?.Quit();
}
