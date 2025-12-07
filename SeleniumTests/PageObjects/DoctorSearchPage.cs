using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumTests.Utils;

namespace SeleniumTests.PageObjects
{
    public class DoctorSearchPage : TestBase
    {
        // Locators
        private readonly By searchInput = By.XPath("//input[@placeholder='Search doctors...' or @placeholder='Search' or contains(@class, 'search')]");
        private readonly By searchButton = By.XPath("//button[contains(text(), 'Search')]");
        private readonly By doctorCards = By.ClassName("doctor-card");
        private readonly By bookNowButtons = By.XPath("//button[contains(text(), 'Book Now') or contains(text(), 'Book Appointment')]");
        private readonly By favoriteButtons = By.XPath("//button[contains(@class, 'favorite') or contains(@class, 'heart')]");
        private readonly By specializationFilter = By.Id("specialization");

        public DoctorSearchPage(IWebDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(TIMEOUT_SECONDS));
        }

        public void NavigateToSearch()
        {
            driver.Navigate().GoToUrl($"{BASE_URL}/doctors");
            LogStep("Navigated to doctor search page");
        }

        public void SearchForSpecialization(string specialization)
        {
            try
            {
                WaitForElement(searchInput, 5);
                var searchField = driver.FindElement(searchInput);
                searchField.Clear();
                searchField.SendKeys(specialization);
                LogStep($"Entered search term: {specialization}");

                // Try to click search button if it exists
                try
                {
                    driver.FindElement(searchButton).Click();
                    LogStep("Clicked search button");
                }
                catch (NoSuchElementException)
                {
                    // If no search button, press Enter
                    searchField.SendKeys(Keys.Enter);
                    LogStep("Pressed Enter to search");
                }

                Thread.Sleep(2000); // Wait for search results
            }
            catch (NoSuchElementException)
            {
                LogStep("Search input not found, trying alternative approach");
                // Try filtering by specialization dropdown
                TrySpecializationFilter(specialization);
            }
        }

        private void TrySpecializationFilter(string specialization)
        {
            try
            {
                var dropdown = driver.FindElement(specializationFilter);
                var select = new SelectElement(dropdown);
                select.SelectByText(specialization);
                LogStep($"Selected specialization from dropdown: {specialization}");
            }
            catch (NoSuchElementException)
            {
                LogStep("No specialization filter found");
            }
        }

        public void ClickFirstBookNow()
        {
            WaitForElement(bookNowButtons, 10);
            var bookButtons = driver.FindElements(bookNowButtons);
            
            if (bookButtons.Count > 0)
            {
                ScrollToElement(bookButtons[0]);
                bookButtons[0].Click();
                LogStep("Clicked first 'Book Now' button");
            }
            else
            {
                throw new NoSuchElementException("No 'Book Now' buttons found");
            }
        }

        public void AddFirstDoctorToFavorites()
        {
            try
            {
                var favoriteButtons = driver.FindElements(this.favoriteButtons);
                if (favoriteButtons.Count > 0)
                {
                    ScrollToElement(favoriteButtons[0]);
                    favoriteButtons[0].Click();
                    LogStep("Added first doctor to favorites");
                    Thread.Sleep(1000); // Wait for favorite action to complete
                }
                else
                {
                    LogStep("No favorite buttons found");
                }
            }
            catch (Exception ex)
            {
                LogStep($"Failed to add to favorites: {ex.Message}");
            }
        }

        public int GetDoctorCount()
        {
            try
            {
                return driver.FindElements(doctorCards).Count;
            }
            catch
            {
                return 0;
            }
        }

        public bool AreDoctorsDisplayed()
        {
            try
            {
                WaitForElement(doctorCards, 5);
                return GetDoctorCount() > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}