using OpenQA.Selenium;

namespace InsuranceAutomationDemo.UiTests.Helpers;

public static class WebDriverExtensions
{
    public static IWebElement? FindElementOrDefault(this IWebDriver driver, By by)
    {
        try
        {
            return driver.FindElement(by);
        }
        catch (NoSuchElementException)
        {
            return null;
        }
    }
}
