using OpenQA.Selenium;

namespace XperienceCommunity.FormNotifications.UITests.Helpers
{
    internal class NavigationHelpers
    {
        public static void GoToHomepage(WebDriver driver)
        {
            driver.Navigate().GoToUrl(Constants.BaseUrl);
        }

        public static void GoToPath(WebDriver driver, string urlPath)
        {
            var url = $"{Constants.BaseUrl}/{urlPath.TrimStart('/')}";
            if (url.Equals(driver.Url, StringComparison.InvariantCultureIgnoreCase))
                return;
            driver.Navigate().GoToUrl(url);
        }

        public static void GoToAdmin(WebDriver driver)
        {
            GoToPath(driver, "/admin/dashboard");
        }

        public static void SignInToAdmin(WebDriver driver)
        {
            GoToAdmin(driver);
            var usernameInput = driver.FindElement(By.CssSelector("[data-testid=\"userName\"]"));
            usernameInput.SendKeys(Constants.AdministratorUsername);
            var passwordInput = driver.FindElement(By.CssSelector("[data-testid=\"password\"]"));
            passwordInput.SendKeys(Constants.AdministratorPassword);
            var signinButton = driver.FindElement(By.CssSelector("[data-testid=\"submit\"]"));
            signinButton.Click();
            // Give the system some time to process the login
            Thread.Sleep(500);
        }
    }
}
