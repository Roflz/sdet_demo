using OpenQA.Selenium;

namespace InsuranceAutomationDemo.UiTests.Pages;

public abstract class BasePage
{
    protected IWebDriver Driver { get; }
    protected string BaseUrl { get; }

    protected BasePage(IWebDriver driver, string baseUrl)
    {
        Driver = driver;
        BaseUrl = baseUrl.TrimEnd('/');
    }

    public void GoTo(string path = "")
    {
        var url = string.IsNullOrEmpty(path) ? BaseUrl : $"{BaseUrl}/{path.TrimStart('/')}";
        Driver.Navigate().GoToUrl(url);
    }
}
