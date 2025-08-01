using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace XperienceCommunity.FormNotifications.UITests;

public abstract class UITestBase
{
    protected WebDriver WebDriver { get; private set; }
    protected string DownloadsDirectory { get; private set; }

    protected void SetupDriver()
    {
        // Define the desired download path
        DownloadsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads"); // Change this to your preferred path

        if (Directory.Exists(DownloadsDirectory))
            Directory.Delete(DownloadsDirectory, true);

        Directory.CreateDirectory(DownloadsDirectory);

        // Create EdgeOptions object
        var edgeOptions = new EdgeOptions();

        // Add the preferences to EdgeOptions
        edgeOptions.AddUserProfilePreference("download.default_directory", DownloadsDirectory);

        var service = EdgeDriverService.CreateDefaultService(AppDomain.CurrentDomain.BaseDirectory);
        WebDriver = new EdgeDriver(service, edgeOptions);

        WebDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Constants.DefaultImplicitWaitTimeSeconds);
    }

    protected void TearDownDriver()
    {
        WebDriver.Quit();
        WebDriver.Dispose();
    }
}
