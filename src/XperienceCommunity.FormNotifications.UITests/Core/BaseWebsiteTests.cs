using NUnit.Framework.Legacy;
using OpenQA.Selenium;
using XperienceCommunity.FormNotifications.UITests.Helpers;

namespace XperienceCommunity.FormNotifications.UITests.Core
{
    public class BaseWebsiteTests : UITestBase
    {
        [SetUp]
        public void Setup()
        {
            SetupDriver();
        }

        [Test]
        public void Website_Did_Start()
        {
            NavigationHelpers.GoToHomepage(WebDriver);
            var logo = WebDriver.FindElement(By.ClassName("logo-image"));
            ClassicAssert.IsNotNull(logo);
        }

        [Test]
        public void Can_Access_Admin()
        {
            NavigationHelpers.SignInToAdmin(WebDriver);
            var menuElement = WebDriver.FindElement(By.CssSelector("[data-testid=application-menu]"));
            ClassicAssert.IsNotNull(menuElement);
        }

        [TearDown]
        public void TearDown()
        {
            TearDownDriver();
        }
    }
}
