using FluentAssertions;
using SeleniumTests.Utils;
using Xunit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SeleniumTests.Tests
{
    public class ChromeStartupTest : TestBase
    {
        [Fact]
        public void Chrome_ShouldStart_AndNavigateToGoogle()
        {
            LogStep("=== TESTING CHROME STARTUP ===");

            try
            {
                LogStep("Testing basic Chrome functionality...");
                
                // Test basic Chrome functionality
                driver.Navigate().GoToUrl("https://www.google.com");
                LogStep("Navigated to Google");

                // Wait for page to load
                wait.Until(d => d.Title.Length > 0);
                
                // Check if page loaded
                string title = driver.Title;
                string url = driver.Url;
                
                LogStep($"Page title: {title}");
                LogStep($"Current URL: {url}");
                
                // Verify we can interact with the page
                title.Should().Contain("Google");
                url.Should().Contain("google.com");
                
                LogStep("✅ Chrome startup test successful");
                TakeScreenshot("ChromeStartup_Success");
            }
            catch (Exception ex)
            {
                LogStep($"❌ Chrome startup failed: {ex.Message}");
                TakeScreenshot("ChromeStartup_Failed");
                throw;
            }
        }

        [Fact]
        public void LocalApp_ShouldBeAccessible()
        {
            LogStep("=== TESTING LOCAL APP ACCESS ===");

            try
            {
                LogStep("Testing local app accessibility...");
                
                // Test if local app is accessible
                driver.Navigate().GoToUrl(BASE_URL);
                LogStep($"Navigated to local app: {BASE_URL}");

                // Wait for page to load (give it more time for React app)
                Thread.Sleep(3000);

                string title = driver.Title;
                string url = driver.Url;
                string pageSource = driver.PageSource;
                
                LogStep($"Page title: '{title}'");
                LogStep($"Current URL: {url}");
                LogStep($"Page source length: {pageSource.Length} characters");
                
                // Check if we can access the page
                url.Should().Contain("localhost:5173");
                
                // Check if it's not an error page
                pageSource.Should().NotContain("This site can't be reached");
                pageSource.Should().NotContain("ERR_CONNECTION_REFUSED");
                
                LogStep("✅ Local app access test successful");
                TakeScreenshot("LocalApp_Success");
            }
            catch (WebDriverException ex) when (ex.Message.Contains("ERR_CONNECTION_REFUSED"))
            {
                LogStep($"❌ Local app not running: {ex.Message}");
                LogStep("Make sure your React app is running on http://localhost:5173");
                TakeScreenshot("LocalApp_NotRunning");
                throw new Exception("Local app is not running. Start your React app first.", ex);
            }
            catch (Exception ex)
            {
                LogStep($"❌ Local app access failed: {ex.Message}");
                TakeScreenshot("LocalApp_Failed");
                throw;
            }
        }
        
        [Fact]
        public void Chrome_DriverInfo_ShouldBeDisplayed()
        {
            LogStep("=== DISPLAYING CHROME DRIVER INFO ===");
            
            try
            {
                var capabilities = ((ChromeDriver)driver).Capabilities;
                
                LogStep($"Browser Name: {capabilities.GetCapability("browserName")}");
                LogStep($"Browser Version: {capabilities.GetCapability("browserVersion")}");
                LogStep($"Chrome Driver Version: {capabilities.GetCapability("chrome")}");
                LogStep($"Platform: {capabilities.GetCapability("platformName")}");
                
                LogStep("✅ Chrome driver info displayed successfully");
            }
            catch (Exception ex)
            {
                LogStep($"❌ Failed to get driver info: {ex.Message}");
                throw;
            }
        }
    }
}