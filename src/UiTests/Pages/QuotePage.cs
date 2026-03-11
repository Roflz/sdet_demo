using OpenQA.Selenium;
using InsuranceAutomationDemo.UiTests.Helpers;

namespace InsuranceAutomationDemo.UiTests.Pages;

public class QuotePage : BasePage
{
    public QuotePage(IWebDriver driver, string baseUrl) : base(driver, baseUrl) { }

    public void GoTo() => GoTo("/quotes");

    public IWebElement? PageTitle => Driver.FindElementOrDefault(By.TagName("h1"));
    public IWebElement? GetQuoteButton => Driver.FindElementOrDefault(By.LinkText("Get Quote"));
}
