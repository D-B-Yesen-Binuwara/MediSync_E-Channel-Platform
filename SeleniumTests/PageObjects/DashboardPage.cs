using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumTests.Utils;

namespace SeleniumTests.PageObjects
{
    public class DashboardPage : TestBase
    {
        // Locators
        private readonly By dashboardButton = By.XPath("//button[contains(text(), 'Dashboard') or contains(text(), 'Go to Dashboard')]");
        private readonly By myAppointmentsLink = By.XPath("//a[contains(text(), 'My Appointments') or contains(text(), 'Appointments')]");
        private readonly By myFavoritesLink = By.XPath("//a[contains(text(), 'My Favorites') or contains(text(), 'Favorites')]");
        private readonly By appointmentCards = By.ClassName("appointment-card");
        private readonly By favoriteCards = By.ClassName("favorite-card");
        private readonly By userProfile = By.XPath("//*[contains(@class, 'profile') or contains(@class, 'user')]");

        public DashboardPage(IWebDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(TIMEOUT_SECONDS));
        }

        public void NavigateToDashboard()
        {
            try
            {
                WaitForElementToBeClickable(dashboardButton, 5);
                driver.FindElement(dashboardButton).Click();
                LogStep("Clicked 'Go to Dashboard' button");
            }
            catch (NoSuchElementException)
            {
                // Try direct navigation
                driver.Navigate().GoToUrl($"{BASE_URL}/dashboard");
                LogStep("Navigated directly to dashboard");
            }
        }

        public void GoToMyAppointments()
        {
            try
            {
                WaitForElementToBeClickable(myAppointmentsLink);
                driver.FindElement(myAppointmentsLink).Click();
                LogStep("Clicked 'My Appointments' link");
            }
            catch (NoSuchElementException)
            {
                // Try direct navigation
                driver.Navigate().GoToUrl($"{BASE_URL}/appointments");
                LogStep("Navigated directly to appointments page");
            }
        }

        public void GoToMyFavorites()
        {
            try
            {
                WaitForElementToBeClickable(myFavoritesLink);
                driver.FindElement(myFavoritesLink).Click();
                LogStep("Clicked 'My Favorites' link");
            }
            catch (NoSuchElementException)
            {
                // Try direct navigation
                driver.Navigate().GoToUrl($"{BASE_URL}/favorites");
                LogStep("Navigated directly to favorites page");
            }
        }

        public bool IsDashboardLoaded()
        {
            try
            {
                // Check if we're on dashboard page
                return driver.Url.Contains("dashboard") || 
                       driver.FindElements(userProfile).Count > 0 ||
                       driver.FindElements(myAppointmentsLink).Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public int GetAppointmentCount()
        {
            try
            {
                WaitForElement(appointmentCards, 5);
                return driver.FindElements(appointmentCards).Count;
            }
            catch
            {
                return 0;
            }
        }

        public int GetFavoriteCount()
        {
            try
            {
                WaitForElement(favoriteCards, 5);
                return driver.FindElements(favoriteCards).Count;
            }
            catch
            {
                return 0;
            }
        }

        public bool AreAppointmentsDisplayed()
        {
            return GetAppointmentCount() > 0;
        }

        public bool AreFavoritesDisplayed()
        {
            return GetFavoriteCount() > 0;
        }
    }
}