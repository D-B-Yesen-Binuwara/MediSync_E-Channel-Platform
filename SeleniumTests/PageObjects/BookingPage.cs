using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumTests.Utils;

namespace SeleniumTests.PageObjects
{
    public class BookingPage : TestBase
    {
        // Locators
        private readonly By timeSlots = By.XPath("//button[contains(@class, 'time-slot') or contains(text(), ':')]");
        private readonly By patientNameInput = By.Id("patientName");
        private readonly By nicInput = By.Id("nic");
        private readonly By emailInput = By.Id("email");
        private readonly By contactInput = By.Id("contactNo");
        private readonly By accountNameInput = By.Id("accountName");
        private readonly By accountNumberInput = By.Id("accountNumber");
        private readonly By bankNameInput = By.Id("bankName");
        private readonly By bankBranchInput = By.Id("bankBranch");
        private readonly By confirmBookingButton = By.XPath("//button[contains(text(), 'Confirm Booking') or contains(text(), 'Book Appointment')]");
        private readonly By successMessage = By.XPath("//*[contains(text(), 'success') or contains(text(), 'booked')]");

        public BookingPage(IWebDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(TIMEOUT_SECONDS));
        }

        public void SelectFirstAvailableTimeSlot()
        {
            WaitForElement(timeSlots, 10);
            var slots = driver.FindElements(timeSlots);
            
            if (slots.Count > 0)
            {
                ScrollToElement(slots[0]);
                slots[0].Click();
                LogStep($"Selected time slot: {slots[0].Text}");
            }
            else
            {
                throw new NoSuchElementException("No time slots available");
            }
        }

        public void FillPatientDetails(string name, string nic, string email, string contact)
        {
            LogStep("Filling patient details...");

            FillField(patientNameInput, name, "Patient Name");
            FillField(nicInput, nic, "NIC");
            FillField(emailInput, email, "Email");
            FillField(contactInput, contact, "Contact");
        }

        public void FillPaymentDetails(string accountName, string accountNumber, string bankName, string bankBranch)
        {
            LogStep("Filling payment details...");

            FillField(accountNameInput, accountName, "Account Name");
            FillField(accountNumberInput, accountNumber, "Account Number");
            FillField(bankNameInput, bankName, "Bank Name");
            FillField(bankBranchInput, bankBranch, "Bank Branch");
        }

        private void FillField(By locator, string value, string fieldName)
        {
            try
            {
                WaitForElement(locator, 5);
                var field = driver.FindElement(locator);
                ScrollToElement(field);
                field.Clear();
                field.SendKeys(value);
                LogStep($"Filled {fieldName}: {value}");
            }
            catch (NoSuchElementException)
            {
                LogStep($"Field {fieldName} not found, skipping...");
            }
        }

        public void ConfirmBooking()
        {
            WaitForElementToBeClickable(confirmBookingButton);
            var confirmButton = driver.FindElement(confirmBookingButton);
            ScrollToElement(confirmButton);
            confirmButton.Click();
            LogStep("Clicked confirm booking button");
        }

        public bool IsBookingSuccessful()
        {
            try
            {
                WaitForElement(successMessage, 15);
                var message = driver.FindElement(successMessage).Text;
                LogStep($"Booking success message: {message}");
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                LogStep("No success message found");
                return false;
            }
        }

        public void CompleteBookingFlow(string patientName, string nic, string email, string contact)
        {
            SelectFirstAvailableTimeSlot();
            FillPatientDetails(patientName, nic, email, contact);
            FillPaymentDetails("John Doe", "1234567890", "Commercial Bank", "Colombo");
            ConfirmBooking();
        }
    }
}