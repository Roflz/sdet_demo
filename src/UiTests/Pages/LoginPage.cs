using OpenQA.Selenium;

namespace InsuranceAutomationDemo.UiTests.Pages;

public class LoginPage : BasePage
{
    public LoginPage(IWebDriver driver, string baseUrl) : base(driver, baseUrl) { }

    public void GoTo() => GoTo("/login");

    public IWebElement? UserNameInput => Driver.FindElementOrDefault(By.Id("username"));
    public IWebElement? PasswordInput => Driver.FindElementOrDefault(By.Id("password"));
    public IWebElement? LoginButton => Driver.FindElementOrDefault(By.CssSelector("button[type='submit']"));

    public void Login(string username, string password)
    {
        UserNameInput?.SendKeys(username);
        PasswordInput?.SendKeys(password);
        LoginButton?.Click();
    }
}
