using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace InsuranceAutomationDemo.UiTests.Helpers;

public static class WebDriverFactory
{
    public static IWebDriver CreateChrome()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        return new ChromeDriver(options);
    }
}
