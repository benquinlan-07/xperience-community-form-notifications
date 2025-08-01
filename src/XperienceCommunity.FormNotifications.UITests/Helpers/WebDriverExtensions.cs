using System.Collections.ObjectModel;
using OpenQA.Selenium;

namespace XperienceCommunity.FormNotifications.UITests.Helpers
{
    internal static class WebDriverExtensions
    {
        public static IWebElement FindByAttribute(this WebDriver input, string attributeName, string attributeValue)
        {
            return input.FindElement(By.CssSelector($"[{attributeName}=\"{attributeValue}\"]"));
        }

        public static IWebElement FindByAttribute(this IWebElement input, string attributeName, string attributeValue)
        {
            return input.FindElement(By.CssSelector($"[{attributeName}=\"{attributeValue}\"]"));
        }

        public static ReadOnlyCollection<IWebElement> FindAllByAttribute(this WebDriver input, string attributeName, string attributeValue)
        {
            return input.FindElements(By.CssSelector($"[{attributeName}=\"{attributeValue}\"]"));
        }

        public static ReadOnlyCollection<IWebElement> FindAllByAttribute(this IWebElement input, string attributeName, string attributeValue)
        {
            return input.FindElements(By.CssSelector($"[{attributeName}=\"{attributeValue}\"]"));
        }

        public static void WithReducedWaitTime(this WebDriver driver, Action<WebDriver> action, int reducedWaitTimeSeconds = 1)
        {
            try
            {
                // Reduce wait time
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(reducedWaitTimeSeconds);
                // Perform action
                action(driver);
            }
            finally
            {
                // Reset to default wait time
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Constants.DefaultImplicitWaitTimeSeconds); 
            }
        }

        public static T WithReducedWaitTime<T>(this WebDriver driver, Func<WebDriver, T> func, int reducedWaitTimeSeconds = 1)
        {
            try
            {
                // Reduce wait time
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(reducedWaitTimeSeconds);
                // Perform action
                return func(driver);
            }
            finally
            {
                // Reset to default wait time
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Constants.DefaultImplicitWaitTimeSeconds);
            }
        }
    }
}
