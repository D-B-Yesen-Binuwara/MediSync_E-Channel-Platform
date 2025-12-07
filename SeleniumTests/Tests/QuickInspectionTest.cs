using FluentAssertions;
using SeleniumTests.Utils;
using Xunit;
using OpenQA.Selenium;

namespace SeleniumTests.Tests
{
    public class QuickInspectionTest : TestBase
    {
        [Fact]
        public void InspectLoginPage_ShouldShowElements()
        {
            LogStep("=== INSPECTING LOGIN PAGE ===");

            try
            {
                // Navigate to login page
                driver.Navigate().GoToUrl($"{BASE_URL}/login");
                LogStep($"Navigated to: {BASE_URL}/login");

                Thread.Sleep(3000); // Wait for React to load

                // Get page info
                string title = driver.Title;
                string url = driver.Url;
                string pageSource = driver.PageSource;

                LogStep($"Page title: '{title}'");
                LogStep($"Current URL: {url}");
                LogStep($"Page source length: {pageSource.Length} characters");

                // Look for common login elements
                var elements = new Dictionary<string, By>
                {
                    ["Email input by ID"] = By.Id("email"),
                    ["Email input by name"] = By.Name("email"),
                    ["Email input by type"] = By.XPath("//input[@type='email']"),
                    ["Password input by ID"] = By.Id("password"),
                    ["Password input by name"] = By.Name("password"),
                    ["Password input by type"] = By.XPath("//input[@type='password']"),
                    ["Login button by text"] = By.XPath("//button[contains(text(), 'Login') or contains(text(), 'Sign In')]"),
                    ["Submit button"] = By.XPath("//button[@type='submit']"),
                    ["Form element"] = By.TagName("form")
                };

                LogStep("Checking for login elements:");
                foreach (var element in elements)
                {
                    try
                    {
                        var found = driver.FindElements(element.Value);
                        if (found.Count > 0)
                        {
                            LogStep($"✅ Found {element.Key}: {found.Count} element(s)");
                            if (found.Count == 1)
                            {
                                var elem = found[0];
                                LogStep($"   - Tag: {elem.TagName}");
                                LogStep($"   - Text: '{elem.Text}'");
                                LogStep($"   - Placeholder: '{elem.GetAttribute("placeholder")}'");
                                LogStep($"   - Class: '{elem.GetAttribute("class")}'");
                            }
                        }
                        else
                        {
                            LogStep($"❌ Not found: {element.Key}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogStep($"❌ Error checking {element.Key}: {ex.Message}");
                    }
                }

                // Look for any input fields
                var allInputs = driver.FindElements(By.TagName("input"));
                LogStep($"Total input fields found: {allInputs.Count}");
                for (int i = 0; i < allInputs.Count && i < 5; i++)
                {
                    var input = allInputs[i];
                    LogStep($"Input {i + 1}: type='{input.GetAttribute("type")}', name='{input.GetAttribute("name")}', id='{input.GetAttribute("id")}', placeholder='{input.GetAttribute("placeholder")}'");
                }

                // Look for any buttons
                var allButtons = driver.FindElements(By.TagName("button"));
                LogStep($"Total buttons found: {allButtons.Count}");
                for (int i = 0; i < allButtons.Count && i < 5; i++)
                {
                    var button = allButtons[i];
                    LogStep($"Button {i + 1}: text='{button.Text}', type='{button.GetAttribute("type")}', class='{button.GetAttribute("class")}'");
                }

                TakeScreenshot("LoginPage_Inspection");
                LogStep("✅ Login page inspection completed");
            }
            catch (Exception ex)
            {
                LogStep($"❌ Login page inspection failed: {ex.Message}");
                TakeScreenshot("LoginPage_Inspection_Failed");
                throw;
            }
        }
    }
}