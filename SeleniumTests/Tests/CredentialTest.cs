using FluentAssertions;
using SeleniumTests.Utils;
using Xunit;
using OpenQA.Selenium;

namespace SeleniumTests.Tests
{
    public class CredentialTest : TestBase
    {
        [Fact]
        public void Test_Login_Credentials()
        {
            LogStep("=== TESTING LOGIN CREDENTIALS ===");

            try
            {
                // Navigate to login
                driver.Navigate().GoToUrl($"{BASE_URL}/login");
                Thread.Sleep(3000);
                
                LogStep($"Login page loaded: {driver.Url}");
                TakeScreenshot("LoginPage");

                // Fill credentials
                var emailInput = wait.Until(d => d.FindElement(By.XPath("//input[@type='email']")));
                var passwordInput = driver.FindElement(By.XPath("//input[@type='password']"));
                var loginButton = driver.FindElement(By.XPath("//button[contains(text(), 'Sign In')]"));

                emailInput.Clear();
                emailInput.SendKeys("niki123@gmail.com");
                
                passwordInput.Clear();
                passwordInput.SendKeys("niki123");
                
                TakeScreenshot("CredentialsFilled");
                
                // Click login
                loginButton.Click();
                LogStep("Login button clicked");
                
                // Wait and check result
                Thread.Sleep(5000);
                
                string finalUrl = driver.Url;
                LogStep($"Final URL: {finalUrl}");
                
                TakeScreenshot("LoginResult");
                
                // Determine result
                if (finalUrl.Contains("/patient"))
                {
                    LogStep("✅ SUCCESS: Login worked, redirected to /patient");
                }
                else if (finalUrl.Contains("/login"))
                {
                    LogStep("❌ FAILED: Still on login page");
                    
                    // Check backend connection
                    LogStep("Checking if backend is running...");
                    try
                    {
                        var client = new System.Net.Http.HttpClient();
                        var response = client.GetAsync("http://localhost:5000/api/doctors").Result;
                        LogStep($"Backend status: {response.StatusCode}");
                    }
                    catch (Exception ex)
                    {
                        LogStep($"❌ Backend not accessible: {ex.Message}");
                    }
                }
                else
                {
                    LogStep($"⚠️ UNEXPECTED: Redirected to {finalUrl}");
                }

                LogStep("=== CREDENTIAL TEST COMPLETED ===");
            }
            catch (Exception ex)
            {
                LogStep($"❌ Test failed: {ex.Message}");
                TakeScreenshot("TestError");
                throw;
            }
        }
    }
}