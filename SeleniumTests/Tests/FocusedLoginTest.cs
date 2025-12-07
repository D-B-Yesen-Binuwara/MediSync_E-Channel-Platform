using FluentAssertions;
using SeleniumTests.Utils;
using Xunit;
using OpenQA.Selenium;

namespace SeleniumTests.Tests
{
    public class FocusedLoginTest : TestBase
    {
        [Fact]
        public void Login_Complete_Flow_Test()
        {
            LogStep("=== FOCUSED LOGIN FLOW TEST ===");

            try
            {
                // Step 1: Navigate to login page
                LogStep("Step 1: Navigating to login page...");
                driver.Navigate().GoToUrl($"{BASE_URL}/login");
                Thread.Sleep(2000);
                
                LogStep($"Current URL: {driver.Url}");
                LogStep($"Page Title: {driver.Title}");
                
                TakeScreenshot("01_LoginPage");

                // Step 2: Find and fill email
                LogStep("Step 2: Finding email input...");
                var emailInput = wait.Until(d => d.FindElement(By.XPath("//input[@type='email']")));
                emailInput.Clear();
                emailInput.SendKeys("niki123@gmail.com");
                LogStep("Email entered successfully");

                // Step 3: Find and fill password
                LogStep("Step 3: Finding password input...");
                var passwordInput = driver.FindElement(By.XPath("//input[@type='password']"));
                passwordInput.Clear();
                passwordInput.SendKeys("niki123");
                LogStep("Password entered successfully");

                TakeScreenshot("02_CredentialsEntered");

                // Step 4: Click login button
                LogStep("Step 4: Finding and clicking login button...");
                var loginButton = driver.FindElement(By.XPath("//button[contains(text(), 'Sign In')]"));
                loginButton.Click();
                LogStep("Login button clicked");

                // Step 5: Wait for response
                LogStep("Step 5: Waiting for login response...");
                Thread.Sleep(3000);

                TakeScreenshot("03_AfterLogin");

                // Step 6: Check result
                string finalUrl = driver.Url;
                LogStep($"Final URL: {finalUrl}");
                
                if (finalUrl.Contains("/login"))
                {
                    LogStep("❌ Still on login page - login may have failed");
                    
                    // Check for error messages
                    try
                    {
                        var errorElements = driver.FindElements(By.XPath("//*[contains(text(), 'error') or contains(text(), 'invalid') or contains(text(), 'wrong')]"));
                        foreach (var error in errorElements)
                        {
                            LogStep($"Error found: {error.Text}");
                        }
                    }
                    catch { }
                }
                else
                {
                    LogStep("✅ Redirected away from login page - login appears successful");
                }

                LogStep("=== LOGIN FLOW TEST COMPLETED ===");
            }
            catch (Exception ex)
            {
                LogStep($"❌ Test failed with exception: {ex.Message}");
                TakeScreenshot("Error_LoginTest");
                throw;
            }
        }

        [Fact]
        public void Quick_Page_Load_Test()
        {
            LogStep("=== QUICK PAGE LOAD TEST ===");

            try
            {
                LogStep("Loading home page...");
                driver.Navigate().GoToUrl(BASE_URL);
                Thread.Sleep(2000);
                
                LogStep($"URL: {driver.Url}");
                LogStep($"Title: {driver.Title}");
                
                TakeScreenshot("HomePage");
                
                LogStep("✅ Page load test completed");
            }
            catch (Exception ex)
            {
                LogStep($"❌ Page load test failed: {ex.Message}");
                TakeScreenshot("Error_PageLoad");
                throw;
            }
        }
    }
}