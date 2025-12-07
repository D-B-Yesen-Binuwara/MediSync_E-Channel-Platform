# Selenium Test Guide

## Prerequisites

1. Make sure your React frontend is running on http://localhost:3000
2. Make sure your ASP.NET backend is running on http://localhost:5000
3. Chrome browser should be installed

## Setup Commands

### 1. Install Dependencies
```bash
cd SeleniumTests
dotnet restore
```

### 2. Build the Test Project
```bash
dotnet build
```

## Test Commands

### 1. Run Complete E-Channeling Flow Test
```bash
dotnet test SeleniumTests --filter "CompleteEChannelingFlow_ShouldWorkEndToEnd" --verbosity detailed
```

Scope: Tests the complete user journey from login to booking appointment
Test Flow:
1. Login with real credentials (alutheka@gmail.com / password123)
2. Search for "cardio" doctors
3. Click "Book Now" on first doctor
4. Select time schedule
5. Fill patient details form
6. Fill payment details form
7. Confirm booking
8. Navigate to dashboard
9. View "My Appointments"
10. Add doctor to favorites (heart icon)
11. View "My Favorites"

Expected Results:
- Login successful
- Cardio doctors found and displayed
- Appointment booking successful
- Dashboard navigation working
- Appointments visible in dashboard
- Doctor added to favorites
- Favorites displayed correctly

### 2. Run Individual Test Components

Login Test Only:
```bash
dotnet test SeleniumTests --filter "LoginFlow_WithValidCredentials_ShouldSucceed" --verbosity detailed
```

Doctor Search Test Only:
```bash
dotnet test SeleniumTests --filter "DoctorSearch_ForCardio_ShouldReturnResults" --verbosity detailed
```

Booking Flow Test Only:
```bash
dotnet test SeleniumTests --filter "BookingFlow_WithValidDetails_ShouldSucceed" --verbosity detailed
```

### 3. Run All Selenium Tests
```bash
dotnet test SeleniumTests --verbosity detailed
```

### 4. Run Tests with Screenshots
```bash
dotnet test SeleniumTests --logger "console;verbosity=detailed"
```

## Test Reports and Screenshots

Test Screenshots: SeleniumTests/Reports/
- Success screenshots: CompleteFlow_Success_[timestamp].png
- Failure screenshots: CompleteFlow_Failed_[timestamp].png

Console Logs: Real-time step-by-step execution logs

## Test Configuration

Base URL: http://localhost:3000 (React frontend)
Browser: Chrome (automatically managed)
Timeout: 10 seconds for element waits
Screenshots: Automatically taken on test completion/failure

## Test Credentials

The tests use real credentials from your database:
- Email: alutheka@gmail.com
- Password: password123
- NIC: 200213203875
- Phone: 0768614247

## Known Issues and Limitations

1. BCrypt/PBKDF2 Compatibility Issue
   - If login fails, it's likely due to the password hashing incompatibility
   - The test will fail at the login step if this issue exists

2. Element Locator Dependencies
   - Tests depend on specific HTML elements and classes
   - If frontend UI changes, locators may need updates

3. Timing Issues
   - Some operations may need longer wait times
   - Network delays can affect test reliability

4. Browser Dependencies
   - Requires Chrome browser installation
   - ChromeDriver is automatically managed

## Troubleshooting

If tests fail:

1. Check if both frontend and backend are running
2. Verify the URLs are accessible
3. Check console logs for specific error messages
4. Review screenshots in Reports folder
5. Ensure test credentials are valid in database

Common Fixes:
- Increase timeout values in TestBase.cs
- Update element locators in PageObjects
- Check network connectivity
- Verify application is fully loaded before running tests

## Test Architecture

PageObjects Pattern:
- LoginPage.cs: Handles login functionality
- DoctorSearchPage.cs: Handles doctor search and favorites
- BookingPage.cs: Handles appointment booking
- DashboardPage.cs: Handles dashboard navigation

Utils:
- TestBase.cs: Base class with WebDriver setup and utilities

Tests:
- EChannelingFlowTest.cs: Main test class with complete flow

## Extending Tests

To add new tests:
1. Create new page objects in PageObjects folder
2. Add new test methods in Tests folder
3. Follow the existing pattern for logging and screenshots
4. Use FluentAssertions for test assertions