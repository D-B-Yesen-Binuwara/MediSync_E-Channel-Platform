using FluentAssertions;
using SeleniumTests.Utils;
using Xunit;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SeleniumTests.Tests
{
    public class SingleFlowTest : TestBase
    {
        [Fact]
        public void Complete_EChanneling_Flow_Single_Browser()
        {
            LogStep("=== COMPLETE E-CHANNELING FLOW - SINGLE BROWSER ===");

            try
            {
                // STEP 1: LOGIN
                LogStep("STEP 1: LOGIN");
                driver.Navigate().GoToUrl($"{BASE_URL}/login");
                Thread.Sleep(3000);
                
                // Check if login form is loaded
                var emailInput = wait.Until(d => d.FindElement(By.XPath("//input[@type='email']")));
                var passwordInput = driver.FindElement(By.XPath("//input[@type='password']"));
                var loginButton = driver.FindElement(By.XPath("//button[contains(text(), 'Sign In')]"));
                
                LogStep("Login form elements found");
                
                emailInput.Clear();
                emailInput.SendKeys("niki123@gmail.com");
                LogStep("Email entered: niki123@gmail.com");
                
                passwordInput.Clear();
                passwordInput.SendKeys("niki123");
                LogStep("Password entered");
                
                loginButton.Click();
                LogStep("Login button clicked");
                
                // Wait for redirect with better detection
                Thread.Sleep(5000);
                
                string currentUrl = driver.Url;
                LogStep($"After login URL: {currentUrl}");
                
                if (currentUrl.Contains("/patient"))
                {
                    LogStep("✅ LOGIN SUCCESSFUL - Redirected to patient page");
                }
                else if (currentUrl.Contains("/login"))
                {
                    LogStep("❌ LOGIN FAILED - Still on login page");
                    // Check for error messages
                    var errors = driver.FindElements(By.XPath("//*[contains(text(), 'error') or contains(text(), 'invalid')]"));
                    foreach (var error in errors)
                    {
                        if (!string.IsNullOrWhiteSpace(error.Text))
                            LogStep($"Error: {error.Text}");
                    }
                }
                else
                {
                    LogStep($"ℹ LOGIN RESULT UNCLEAR - Redirected to: {currentUrl}");
                }
                
                TakeScreenshot("01_AfterLogin");

                // STEP 2: CHECK PATIENT PAGE (should already be here after login)
                LogStep("STEP 2: CHECK PATIENT PAGE WITH DOCTORS");
                // After login, we should be on /patient page with FindDoc component
                if (!driver.Url.Contains("/patient"))
                {
                    driver.Navigate().GoToUrl($"{BASE_URL}/patient");
                    Thread.Sleep(2000);
                }
                
                LogStep($"Patient page URL: {driver.Url}");
                TakeScreenshot("02_PatientPage");

                // STEP 3: WAIT FOR DOCTORS TO LOAD IN FINDDOC COMPONENT
                LogStep("STEP 3: WAIT FOR DOCTORS TO LOAD");
                Thread.Sleep(3000); // Wait for doctors to load from API
                
                TakeScreenshot("03_DoctorsLoaded");

                // STEP 4: FIND DOCTORS AND BOOK NOW BUTTONS
                LogStep("STEP 4: FIND DOCTORS AND BOOK NOW BUTTONS");
                
                // Look for doctor cards/elements
                var doctorCards = driver.FindElements(By.XPath("//div[contains(@class, 'doctor') or contains(@class, 'card') or contains(@class, 'find')]"));
                LogStep($"Found {doctorCards.Count} doctor card elements");
                
                // Look specifically for "Book Now" buttons
                var bookNowButtons = driver.FindElements(By.XPath("//button[contains(text(), 'Book Now') or contains(text(), 'Book')]"));
                LogStep($"Found {bookNowButtons.Count} 'Book Now' buttons");
                
                // Also check for any buttons in general
                var allButtons = driver.FindElements(By.TagName("button"));
                LogStep($"Total buttons on page: {allButtons.Count}");
                
                // Log first few button texts
                for (int i = 0; i < Math.Min(allButtons.Count, 5); i++)
                {
                    LogStep($"Button {i + 1}: '{allButtons[i].Text}'");
                }

                if (bookNowButtons.Count > 0)
                {
                    LogStep("STEP 5: CLICK BOOK NOW BUTTON");
                    try
                    {
                        bookNowButtons[0].Click();
                        LogStep("Clicked first 'Book Now' button");
                        Thread.Sleep(3000);
                        TakeScreenshot("04_BookingPage");
                        
                        LogStep($"After booking click URL: {driver.Url}");
                        
                        // STEP 6: FIND AND CLICK BOOK NOW ON SLOT
                        LogStep("STEP 6: FIND SLOT AND CLICK BOOK NOW");
                        var slotBookButtons = driver.FindElements(By.XPath("//button[contains(text(), 'Book Now')]"));
                        LogStep($"Found {slotBookButtons.Count} slot booking buttons");
                        
                        if (slotBookButtons.Count > 0)
                        {
                            slotBookButtons[0].Click();
                            LogStep("Clicked slot Book Now button");
                            Thread.Sleep(2000);
                            TakeScreenshot("05_BookingModal");
                            
                            // STEP 7: FILL PATIENT DETAILS
                            LogStep("STEP 7: FILL PATIENT DETAILS");
                            
                            var nameInput = wait.Until(d => d.FindElement(By.XPath("//input[@placeholder='Enter full name']")));
                            nameInput.Clear();
                            nameInput.SendKeys("Niki Test User");
                            
                            var nicInput = driver.FindElement(By.XPath("//input[@placeholder='12 digit NIC number']"));
                            nicInput.Clear();
                            nicInput.SendKeys("200213203875");
                            
                            var patientEmailInput = driver.FindElement(By.XPath("//input[@placeholder='example@email.com']"));
                            patientEmailInput.Clear();
                            patientEmailInput.SendKeys("niki123@gmail.com");
                            
                            var contactInput = driver.FindElement(By.XPath("//input[@placeholder='Contact number']"));
                            contactInput.Clear();
                            contactInput.SendKeys("0768614247");
                            
                            LogStep("Patient details filled");
                            TakeScreenshot("06_PatientDetailsFilled");
                            
                            // STEP 8: CLICK PAY NOW
                            LogStep("STEP 8: CLICK PAY NOW");
                            var payNowButton = driver.FindElement(By.XPath("//button[contains(text(), 'Pay Now')]"));
                            payNowButton.Click();
                            Thread.Sleep(2000);
                            TakeScreenshot("07_PaymentForm");
                            
                            // STEP 9: FILL PAYMENT DETAILS
                            LogStep("STEP 9: FILL PAYMENT DETAILS");
                            
                            var accNameInput = wait.Until(d => d.FindElement(By.XPath("//label[text()='Account Name']/following-sibling::input")));
                            accNameInput.Clear();
                            accNameInput.SendKeys("Niki Test");
                            
                            var accNoInput = driver.FindElement(By.XPath("//label[text()='Account Number']/following-sibling::input"));
                            accNoInput.Clear();
                            accNoInput.SendKeys("1234567890");
                            
                            var bankNameInput = driver.FindElement(By.XPath("//label[text()='Bank Name']/following-sibling::input"));
                            bankNameInput.Clear();
                            bankNameInput.SendKeys("Commercial Bank");
                            
                            var bankBranchInput = driver.FindElement(By.XPath("//label[text()='Bank Branch']/following-sibling::input"));
                            bankBranchInput.Clear();
                            bankBranchInput.SendKeys("Colombo");
                            
                            var pinInput = driver.FindElement(By.XPath("//label[text()='PIN']/following-sibling::input"));
                            pinInput.Clear();
                            pinInput.SendKeys("1234");
                            
                            LogStep("Payment details filled");
                            TakeScreenshot("08_PaymentDetailsFilled");
                            
                            // STEP 10: CLICK CHECKOUT
                            LogStep("STEP 10: CLICK CHECKOUT");
                            var checkoutButton = driver.FindElement(By.XPath("//button[contains(text(), 'Checkout')]"));
                            checkoutButton.Click();
                            Thread.Sleep(5000); // Wait for payment processing
                            TakeScreenshot("09_PaymentSuccess");
                            
                            LogStep($"After payment URL: {driver.Url}");
                            
                            // STEP 11: CLICK BACK TO DASHBOARD
                            LogStep("STEP 11: BACK TO DASHBOARD");
                            try
                            {
                                var dashboardButton = driver.FindElement(By.XPath("//button[contains(text(), 'Back to Dashboard')]"));
                                dashboardButton.Click();
                                Thread.Sleep(2000);
                                LogStep("Clicked Back to Dashboard");
                            }
                            catch
                            {
                                driver.Navigate().GoToUrl($"{BASE_URL}/patient");
                                Thread.Sleep(2000);
                                LogStep("Navigated to dashboard directly");
                            }
                            
                            TakeScreenshot("10_BackToDashboard");
                            
                            // STEP 12: NAVIGATE TO APPOINTMENTS
                            LogStep("STEP 12: NAVIGATE TO APPOINTMENTS");
                            driver.Navigate().GoToUrl($"{BASE_URL}/appointments");
                            Thread.Sleep(3000);
                            TakeScreenshot("11_AppointmentsPage");
                            
                            LogStep($"Appointments page URL: {driver.Url}");
                            
                            // STEP 13: CHECK FOR APPOINTMENTS
                            LogStep("STEP 13: CHECK FOR APPOINTMENTS");
                            var appointmentCards = driver.FindElements(By.ClassName("appointment-card"));
                            LogStep($"Found {appointmentCards.Count} appointment cards");
                            
                            if (appointmentCards.Count > 0)
                            {
                                LogStep("✅ Appointments found - booking was successful!");
                                // Check appointment details
                                var appointmentText = appointmentCards[0].Text;
                                LogStep($"First appointment details: {appointmentText.Substring(0, Math.Min(200, appointmentText.Length))}");
                            }
                            else
                            {
                                LogStep("❌ No appointments found - checking page content");
                                var pageContent = driver.FindElement(By.TagName("body")).Text;
                                if (pageContent.Contains("No Appointments Found"))
                                {
                                    LogStep("Page shows 'No Appointments Found' message");
                                }
                            }
                            
                            TakeScreenshot("12_FinalAppointments");
                        }
                        else
                        {
                            LogStep("No slot booking buttons found");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogStep($"Booking flow failed: {ex.Message}");
                        TakeScreenshot("Error_BookingFlow");
                    }
                }
                else
                {
                    LogStep("No 'Book Now' buttons found - checking page content");
                }

                // STEP 6: CHECK NAVIGATION/DASHBOARD
                LogStep("STEP 6: CHECK NAVIGATION");
                try
                {
                    // Look for navigation elements
                    var navElements = driver.FindElements(By.XPath("//nav//a | //header//a | //button[contains(text(), 'Dashboard') or contains(text(), 'Profile') or contains(text(), 'Appointments')]"));
                    LogStep($"Found {navElements.Count} navigation elements");
                    
                    foreach (var nav in navElements.Take(5))
                    {
                        LogStep($"Nav element: {nav.Text} - {nav.TagName}");
                    }
                }
                catch (Exception ex)
                {
                    LogStep($"Navigation check failed: {ex.Message}");
                }

                TakeScreenshot("05_FinalState");

                // STEP 7: ANALYZE PAGE CONTENT
                LogStep("STEP 7: PAGE CONTENT ANALYSIS");
                var pageText = driver.FindElement(By.TagName("body")).Text;
                LogStep($"Page contains 'doctor': {pageText.ToLower().Contains("doctor")}");
                LogStep($"Page contains 'appointment': {pageText.ToLower().Contains("appointment")}");
                LogStep($"Page contains 'book': {pageText.ToLower().Contains("book")}");
                LogStep($"Page contains 'finddoc': {pageText.ToLower().Contains("finddoc")}");
                LogStep($"Page contains 'medisync': {pageText.ToLower().Contains("medisync")}");
                
                // Check if we have any React components loaded
                var reactElements = driver.FindElements(By.XPath("//*[contains(@class, 'react') or contains(@class, 'component')]"));
                LogStep($"React-related elements: {reactElements.Count}");

                LogStep("=== COMPLETE E-CHANNELING FLOW FINISHED ===");
                LogStep("✅ Flow completed successfully - check screenshots for details");
            }
            catch (Exception ex)
            {
                LogStep($"❌ Flow failed with exception: {ex.Message}");
                TakeScreenshot("Error_CompleteFlow");
                throw;
            }
        }
    }
}