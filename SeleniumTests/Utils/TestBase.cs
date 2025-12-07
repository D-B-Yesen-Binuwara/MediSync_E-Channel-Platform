using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace SeleniumTests.Utils
{
    public class TestBase : IDisposable
    {
        protected IWebDriver driver;
        protected WebDriverWait wait;
        protected const string BASE_URL = "http://localhost:5173"; // React app URL
        protected const int TIMEOUT_SECONDS = 15;

        public TestBase()
        {
            SetupDriver();
        }

        private void SetupDriver()
        {
            LogStep("Setting up Chrome driver...");
            
            // Kill any existing Chrome processes first
            KillExistingChromeProcesses();
            
            var options = new ChromeOptions();
            
            // Essential Chrome options for stability and testing
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1280,720");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-web-security");
            options.AddArgument("--allow-running-insecure-content");
            options.AddArgument("--ignore-certificate-errors");
            options.AddArgument("--ignore-ssl-errors");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--disable-background-timer-throttling");
            options.AddArgument("--disable-renderer-backgrounding");
            options.AddArgument("--disable-backgrounding-occluded-windows");
            options.AddArgument("--disable-features=TranslateUI");
            options.AddArgument("--disable-ipc-flooding-protection");
            options.AddArgument("--force-device-scale-factor=1");
            
            // Remove automation indicators
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);
            
            // Set page load strategy to reduce hanging
            options.PageLoadStrategy = PageLoadStrategy.Normal;
            
            try
            {
                LogStep("Using WebDriverManager to setup compatible ChromeDriver...");
                
                // Use WebDriverManager to automatically download compatible driver
                try
                {
                    new DriverManager().SetUpDriver(new ChromeConfig());
                    LogStep("WebDriverManager setup completed");
                }
                catch (Exception ex)
                {
                    LogStep($"WebDriverManager failed: {ex.Message}, trying Selenium Manager");
                }
                
                LogStep("Creating ChromeDriver instance...");
                
                // Let Selenium Manager handle driver compatibility automatically
                driver = new ChromeDriver(options);
                
                // Set conservative timeouts
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
                driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(15);
                
                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(TIMEOUT_SECONDS));
                
                LogStep($"ChromeDriver initialized successfully");
            }
            catch (Exception ex)
            {
                LogStep($"Failed to initialize ChromeDriver: {ex.Message}");
                throw new Exception($"Chrome driver setup failed: {ex.Message}", ex);
            }
        }
        
        private void KillExistingChromeProcesses()
        {
            try
            {
                LogStep("Killing existing Chrome processes...");
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = "/F /IM chrome.exe",
                    UseShellExecute = false,
                    CreateNoWindow = true
                })?.WaitForExit(5000);
                
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = "/F /IM chromedriver.exe",
                    UseShellExecute = false,
                    CreateNoWindow = true
                })?.WaitForExit(5000);
                
                Thread.Sleep(1000); // Wait for processes to be killed
            }
            catch (Exception ex)
            {
                LogStep($"Warning: Could not kill existing processes: {ex.Message}");
            }
        }

        protected void NavigateToHome()
        {
            driver.Navigate().GoToUrl(BASE_URL);
            LogStep($"Navigated to: {BASE_URL}");
        }

        protected void LogStep(string message)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        protected void TakeScreenshot(string testName)
        {
            try
            {
                // Ensure Reports directory exists
                var reportsDir = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
                if (!Directory.Exists(reportsDir))
                {
                    Directory.CreateDirectory(reportsDir);
                }
                
                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                var fileName = Path.Combine(reportsDir, $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                screenshot.SaveAsFile(fileName);
                LogStep($"Screenshot saved: {fileName}");
            }
            catch (Exception ex)
            {
                LogStep($"Failed to take screenshot: {ex.Message}");
            }
        }

        protected void WaitForElement(By locator, int timeoutSeconds = TIMEOUT_SECONDS)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
            wait.Until(driver => driver.FindElement(locator));
        }

        protected void WaitForElementToBeClickable(By locator, int timeoutSeconds = TIMEOUT_SECONDS)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(locator));
        }

        protected void ScrollToElement(IWebElement element)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
            Thread.Sleep(500); // Small delay after scroll
        }

        public void Dispose()
        {
            try
            {
                LogStep("Disposing Chrome driver...");
                driver?.Quit();
                driver?.Dispose();
                LogStep("Chrome driver disposed successfully");
            }
            catch (Exception ex)
            {
                LogStep($"Error disposing driver: {ex.Message}");
            }
        }
    }
}