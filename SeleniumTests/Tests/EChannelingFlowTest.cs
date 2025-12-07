using FluentAssertions;
using SeleniumTests.PageObjects;
using SeleniumTests.Utils;
using Xunit;

namespace SeleniumTests.Tests
{
    public class EChannelingFlowTest : TestBase
    {
        private LoginPage loginPage;
        private DoctorSearchPage searchPage;
        private BookingPage bookingPage;
        private DashboardPage dashboardPage;

        public EChannelingFlowTest()
        {
            loginPage = new LoginPage(driver);
            searchPage = new DoctorSearchPage(driver);
            bookingPage = new BookingPage(driver);
            dashboardPage = new DashboardPage(driver);
        }

        [Fact]
        public void CompleteEChannelingFlow_ShouldWorkEndToEnd()
        {
            LogStep("=== STARTING COMPLETE E-CHANNELING FLOW TEST ===");

            try
            {
                // Step 1: Login
                LogStep("STEP 1: Performing Login");
                PerformLogin();

                // Step 2: Search for Cardio doctors
                LogStep("STEP 2: Searching for Cardio doctors");
                SearchForCardioDoctors();

                // Step 3: Book appointment
                LogStep("STEP 3: Booking appointment");
                BookAppointment();

                // Step 4: Go to dashboard
                LogStep("STEP 4: Navigating to dashboard");
                NavigateToDashboard();

                // Step 5: View appointments
                LogStep("STEP 5: Viewing my appointments");
                ViewMyAppointments();

                // Step 6: Add doctor to favorites
                LogStep("STEP 6: Adding doctor to favorites");
                AddDoctorToFavorites();

                // Step 7: View favorites
                LogStep("STEP 7: Viewing my favorites");
                ViewMyFavorites();

                LogStep("=== E-CHANNELING FLOW TEST COMPLETED SUCCESSFULLY ===");
                TakeScreenshot("CompleteFlow_Success");
            }
            catch (Exception ex)
            {
                LogStep($"=== TEST FAILED: {ex.Message} ===");
                TakeScreenshot("CompleteFlow_Failed");
                throw;
            }
        }

        private void PerformLogin()
        {
            // Using real credentials from your database
            string email = "niki123@gmail.com";
            string password = "niki123";

            loginPage.PerformLogin(email, password);
            
            // Verify login success
            bool loginSuccess = loginPage.IsLoginSuccessful();
            loginSuccess.Should().BeTrue("Login should be successful with valid credentials");
            
            LogStep("✅ Login completed successfully");
        }

        private void SearchForCardioDoctors()
        {
            searchPage.NavigateToSearch();
            searchPage.SearchForSpecialization("cardio");
            
            // Verify doctors are displayed
            bool doctorsFound = searchPage.AreDoctorsDisplayed();
            doctorsFound.Should().BeTrue("Cardio doctors should be found and displayed");
            
            int doctorCount = searchPage.GetDoctorCount();
            LogStep($"✅ Found {doctorCount} cardio doctors");
        }

        private void BookAppointment()
        {
            // Click Book Now on first doctor
            searchPage.ClickFirstBookNow();
            
            // Fill booking details
            bookingPage.CompleteBookingFlow(
                patientName: "Niki User",
                nic: "200213203875",
                email: "niki123@gmail.com",
                contact: "0768614247"
            );
            
            // Verify booking success
            bool bookingSuccess = bookingPage.IsBookingSuccessful();
            bookingSuccess.Should().BeTrue("Appointment booking should be successful");
            
            LogStep("✅ Appointment booked successfully");
        }

        private void NavigateToDashboard()
        {
            dashboardPage.NavigateToDashboard();
            
            // Verify dashboard loaded
            bool dashboardLoaded = dashboardPage.IsDashboardLoaded();
            dashboardLoaded.Should().BeTrue("Dashboard should load successfully");
            
            LogStep("✅ Dashboard loaded successfully");
        }

        private void ViewMyAppointments()
        {
            dashboardPage.GoToMyAppointments();
            
            // Verify appointments are displayed
            bool appointmentsDisplayed = dashboardPage.AreAppointmentsDisplayed();
            appointmentsDisplayed.Should().BeTrue("Booked appointments should be visible");
            
            int appointmentCount = dashboardPage.GetAppointmentCount();
            LogStep($"✅ Viewing {appointmentCount} appointments");
        }

        private void AddDoctorToFavorites()
        {
            // Go back to search page to add favorites
            searchPage.NavigateToSearch();
            searchPage.SearchForSpecialization("cardio");
            
            // Add first doctor to favorites
            searchPage.AddFirstDoctorToFavorites();
            
            LogStep("✅ Doctor added to favorites");
        }

        private void ViewMyFavorites()
        {
            dashboardPage.GoToMyFavorites();
            
            // Verify favorites are displayed
            bool favoritesDisplayed = dashboardPage.AreFavoritesDisplayed();
            favoritesDisplayed.Should().BeTrue("Added doctor should appear in favorites");
            
            int favoriteCount = dashboardPage.GetFavoriteCount();
            LogStep($"✅ Viewing {favoriteCount} favorite doctors");
        }

        [Fact]
        public void LoginFlow_WithValidCredentials_ShouldSucceed()
        {
            LogStep("=== TESTING LOGIN FLOW ===");
            
            PerformLogin();
            
            LogStep("✅ Login flow test completed");
        }

        [Fact]
        public void DoctorSearch_ForCardio_ShouldReturnResults()
        {
            LogStep("=== TESTING DOCTOR SEARCH ===");
            
            PerformLogin();
            SearchForCardioDoctors();
            
            LogStep("✅ Doctor search test completed");
        }

        [Fact]
        public void BookingFlow_WithValidDetails_ShouldSucceed()
        {
            LogStep("=== TESTING BOOKING FLOW ===");
            
            PerformLogin();
            SearchForCardioDoctors();
            BookAppointment();
            
            LogStep("✅ Booking flow test completed");
        }
    }
}