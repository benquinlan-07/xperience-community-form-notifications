using OpenQA.Selenium;

namespace XperienceCommunity.FormNotifications.UITests.Helpers
{
    internal class FormHelper
    {
        public static void GoToContactFormSubmissions(WebDriver driver)
        {
            // Open the admin
            NavigationHelpers.GoToAdmin(driver);
            // Click on the forms application tile
            var formsTile = driver.FindByAttribute("data-testid", "action-tile-forms");
            formsTile.Click();
            // Go to the contact form
            var contactForm = driver.FindByAttribute("title", "Contact Us");
            contactForm.Click();
            // Go to submissions
            var submissionsMenuItem = driver.FindByAttribute("aria-label", "Submissions");
            submissionsMenuItem.Click();
        }

        public static void EnsureContactFormSubmissions(WebDriver driver)
        {
            GoToContactFormSubmissions(driver);

            // Check for table rows
            var tableRows = driver.WithReducedWaitTime(d => driver.FindAllByAttribute("data-testid", "table-row"));
            if (tableRows.Count > 0)
                return;

            SubmitContactForm(driver);

            EnsureContactFormSubmissions(driver);
        }

        public static void EnsureNoContactFormSubmissions(WebDriver driver)
        {
            GoToContactFormSubmissions(driver);

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

            EnsureNoContactFormSubmissions(driver);
        }

        public static void SubmitContactForm(WebDriver driver)
        {
            // Open the contact page
            NavigationHelpers.GoToPath(driver, "/contacts");
            // Find the contact form
            var form = driver.FindElement(By.TagName("form"));
            var formInputs = form.FindElements(By.CssSelector("input[type='text']"));
            formInputs[0].SendKeys("TestFirst");
            formInputs[1].SendKeys("TestLast");
            var formEmails = form.FindElements(By.CssSelector("input[type='email']"));
            formEmails[0].SendKeys($"testemail+{Guid.NewGuid()}@test.benquinlan.dev");
            var formTextareas = form.FindElements(By.TagName("textarea"));
            formTextareas[0].SendKeys("Test message");
            var submitButton = form.FindElement(By.CssSelector("input[type='submit'],button[type='submit']"));
            submitButton.Click();
        }
    }
}
