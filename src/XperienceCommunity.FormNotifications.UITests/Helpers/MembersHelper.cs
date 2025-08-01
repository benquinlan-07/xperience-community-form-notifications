using OpenQA.Selenium;

namespace XperienceCommunity.FormNotifications.UITests.Helpers
{
    internal class MembersHelper
    {
        public static void GoToMembers(WebDriver driver)
        {
            // Open the admin
            NavigationHelpers.GoToAdmin(driver);
            // Click on the forms application tile
            var formsTile = driver.FindByAttribute("data-testid", "action-tile-members");
            formsTile.Click();
        }

        public static void EnsureMembersExist(WebDriver driver)
        {
            GoToMembers(driver);

            // Check for table rows
            var tableRows = driver.WithReducedWaitTime(d => driver.FindAllByAttribute("data-testid", "table-row"));
            if (tableRows.Count > 0)
                return;

            RegisterNewMember(driver);

            EnsureMembersExist(driver);
        }

        public static void EnsureNoMembers(WebDriver driver)
        {
            GoToMembers(driver);

            // Check for table rows
            var tableRows = driver.WithReducedWaitTime(d => driver.FindAllByAttribute("data-testid", "table-row"));
            if (tableRows.Count == 0)
                return;

            // find all submission delete buttons
            var deleteButtons = driver.FindAllByAttribute("data-testid", "button-Delete");
            foreach (var deleteButton in deleteButtons)
            {
                // Click the delete button
                deleteButton.Click();
                // Confirm the delete
                var confirmButton = driver.FindByAttribute("data-testid", "confirm-action");
                confirmButton.Click();
            }

            EnsureNoMembers(driver);
        }

        public static void RegisterNewMember(WebDriver driver)
        {
            // Open the contact page
            NavigationHelpers.GoToPath(driver, "/en/account/register");
            // Find the contact form
            var form = driver.FindElement(By.TagName("form"));
            var formInputs = form.FindElements(By.CssSelector("input[type='text']"));
            formInputs[0].SendKeys($"{Guid.NewGuid()}");
            var formEmails = form.FindElements(By.CssSelector("input[type='email']"));
            formEmails[0].SendKeys($"testemail+{Guid.NewGuid()}@test.benquinlan.dev");
            var formPasswords = form.FindElements(By.CssSelector("input[type='password']"));
            formPasswords[0].SendKeys("xperience");
            formPasswords[1].SendKeys("xperience");
            var submitButton = form.FindElement(By.CssSelector("input[type='submit'],button[type='submit']"));
            submitButton.Click();
        }
    }
}
