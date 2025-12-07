using FluentAssertions;
using SeleniumTests.Utils;
using Xunit;
using OpenQA.Selenium;

namespace SeleniumTests.Tests
{
    public class DebugLoginTest : TestBase
    {
        [Fact]
        public void Debug_Login_Issue()
        {
            LogStep("=== DEBUG LOGIN ISSUE ===");

            try
            {
                // Step 1: Navigate to login page
                LogStep("Step 1: Navigate to login page");
                driver.Navigate().GoToUrl($"{BASE_URL}/login");
                Thread.Sleep(3000);
                
                LogStep($"Current URL: {driver.Url}");
                LogStep($"Page Title: {driver.Title}");
                TakeScreenshot("01_LoginPage");

                // Step 2: Check page elements
                LogStep("Step 2: Check page elements");
                var emailInputs = driver.FindElements(By.XPath("//input[@type='email']"));
                var passwordInputs = driver.FindElements(By.XPath("//input[@type='password']"));
                var loginButtons = driver.FindElements(By.XPath("//button[contains(text(), 'Sign In')]"));
                
                LogStep($"Email inputs found: {emailInputs.Count}");
                LogStep($"Password inputs found: {passwordInputs.Count}");
                LogStep($"Sign In buttons found: {loginButtons.Count}");

                if (emailInputs.Count == 0 || passwordInputs.Count == 0 || loginButtons.Count == 0)
                {
                    LogStep("❌ Login form elements not found!");
                    var pageSource = driver.PageSource;
                    LogStep($"Page source length: {pageSource.Length}");
                    return;
                }

                // Step 3: Fill credentials
                LogStep("Step 3: Fill credentials");
                emailInputs[0].Clear();
                emailInputs[0].SendKeys("niki123@gmail.com");
                LogStep("Email entered");

                passwordInputs[0].Clear();
                passwordInputs[0].SendKeys("niki123");
                LogStep("Password entered");

                TakeScreenshot("02_CredentialsFilled");

                // Step 4: Click login
                LogStep("Step 4: Click login button");
                loginButtons[0].Click();
                LogStep("Login button clicked");

                // Step 5: Wait and check result
                LogStep("Step 5: Wait for response");
                Thread.Sleep(5000);

                string finalUrl = driver.Url;
                LogStep($"Final URL: {finalUrl}");
                TakeScreenshot("03_AfterLogin");

                // Step 6: Check for error messages or alerts
                LogStep("Step 6: Check for errors");
                try
                {
                    var alert = driver.SwitchTo().Alert();
                    LogStep($"Alert found: {alert.Text}");
                    alert.Accept();
                }
                catch
                {
                    LogStep("No alert found");
                }

                // Check for error messages on page
                var errorElements = driver.FindElements(By.XPath("//*[contains(text(), 'error') or contains(text(), 'invalid') or contains(text(), 'wrong') or contains(text(), 'failed')]"));
                LogStep($"Error elements found: {errorElements.Count}");
                foreach (var error in errorElements)
                {
                    LogStep($"Error text: {error.Text}");
                }

                // Check if still on login page
                if (finalUrl.Contains("/login"))
                {
                    LogStep("❌ Still on login page - login failed");
                    
                    // Check network/console errors
                    var logs = driver.Manage().Logs.GetLog("browser");
                    LogStep($"Browser console logs: {logs.Count}");
                    foreach (var log in logs.Take(5))
                    {
                        LogStep($"Console: {log.Level} - {log.Message}");
                    }
                }
                else
                {
                    LogStep("✅ Redirected - login appears successful");
                }

                LogStep("=== DEBUG LOGIN COMPLETED ===");
            }
            catch (Exception ex)
            {
                LogStep($"❌ Debug login failed: {ex.Message}");
                TakeScreenshot("Error_DebugLogin");
                throw;
            }
        }
    }
}