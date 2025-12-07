using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumTests.Utils;

namespace SeleniumTests.PageObjects
{
    public class LoginPage : TestBase
    {
        // Locators based on actual page inspection
        private readonly By emailInput = By.XPath("//input[@type='email']");
        private readonly By passwordInput = By.XPath("//input[@type='password']");
        private readonly By signInButton = By.XPath("//button[contains(text(), 'Sign In')]");
        private readonly By loginButton = By.XPath("//button[contains(text(), 'Login')]");
        private readonly By registerButton = By.XPath("//button[contains(text(), 'Register')]");
        private readonly By errorMessage = By.ClassName("error-message");
        private readonly By formElement = By.ClassName("form");

        public LoginPage(IWebDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(TIMEOUT_SECONDS));
        }

        public void NavigateToLogin()
        {
            driver.Navigate().GoToUrl($"{BASE_URL}/login");
            LogStep("Navigated to login page");
        }

        public void EnterEmail(string email)
        {
            WaitForElement(emailInput);
            var emailField = driver.FindElement(emailInput);
            emailField.Clear();
            emailField.SendKeys(email);
            LogStep($"Entered email: {email}");
        }

        public void EnterPassword(string password)
        {
            var passwordField = driver.FindElement(passwordInput);
            passwordField.Clear();
            passwordField.SendKeys(password);
            LogStep("Entered password");
        }

        public void ClickLogin()
        {
            try
            {
                // Try Sign In button first (primary login button)
                WaitForElementToBeClickable(signInButton, 5);
                driver.FindElement(signInButton).Click();
                LogStep("Clicked 'Sign In' button");
            }
            catch (WebDriverTimeoutException)
            {
                // Fallback to Login button
                try
                {
                    WaitForElementToBeClickable(loginButton, 5);
                    driver.FindElement(loginButton).Click();
                    LogStep("Clicked 'Login' button");
                }
                catch (WebDriverTimeoutException)
                {
                    LogStep("Neither Sign In nor Login button found");
                    throw new NoSuchElementException("No login button found on the page");
                }
            }
        }

        public bool IsLoginSuccessful()
        {
            try
            {
                // Wait for redirect away from login page
                wait.Until(driver => !driver.Url.Contains("/login"));
                
                string currentUrl = driver.Url;
                LogStep($"Login successful - redirected to: {currentUrl}");
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                string currentUrl = driver.Url;
                LogStep($"Login failed - still on: {currentUrl}");
                
                // Check for error messages
                string errorMsg = GetErrorMessage();
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    LogStep($"Error message: {errorMsg}");
                }
                
                return false;
            }
        }

        public string GetErrorMessage()
        {
            try
            {
                return driver.FindElement(errorMessage).Text;
            }
            catch (NoSuchElementException)
            {
                return "";
            }
        }

        public void PerformLogin(string email, string password)
        {
            NavigateToLogin();
            EnterEmail(email);
            EnterPassword(password);
            ClickLogin();
        }
    }
}