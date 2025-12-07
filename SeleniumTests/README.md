# MediSync Selenium Tests

Automated E2E testing for the MediSync E-Channeling platform using Selenium WebDriver.

## Prerequisites

- .NET 8.0 SDK
- Chrome browser (latest version)
- MediSync Backend running on `http://localhost:5000`
- MediSync Frontend running on `http://localhost:5173`

## Test Credentials

- **Email:** `niki123@gmail.com`
- **Password:** `niki123`

## Quick Start

```bash
# Navigate to SeleniumTests folder
cd SeleniumTests

# Restore packages
dotnet restore

# Run all tests
dotnet test

# Run specific test
dotnet test --filter "Complete_EChanneling_Flow_Single_Browser"
```

## Available Tests

### 1. Chrome Startup Tests
```bash
dotnet test --filter "ChromeStartupTest"
```

### 2. Login Flow Test
```bash
dotnet test --filter "Login_Complete_Flow_Test"
```

### 3. Complete E-Channeling Flow
```bash
dotnet test --filter "Complete_EChanneling_Flow_Single_Browser"
```

## Test Flow Coverage

The complete E-channeling flow test covers:

1. **Login** → `/patient`
2. **Find Doctors** → View available doctors
3. **Book Appointment** → `/book/{doctorId}`
4. **Select Time Slot** → Choose available slot
5. **Fill Patient Details** → Name, NIC, Email, Contact
6. **Payment Processing** → Bank transfer details
7. **Appointment Confirmation** → Success page
8. **View Appointments** → `/appointments` page

## Test Results

- **Screenshots:** Saved in `bin/Debug/net8.0/Reports/`
- **Logs:** Console output with detailed steps
- **Success Indicators:** ✅ or ❌ for each step

## Troubleshooting

### Chrome Version Issues
If you get ChromeDriver version errors:
```bash
# Clean and restore
dotnet clean
dotnet restore
dotnet build
```

### Application Not Running
Ensure both services are running:
- Backend: `http://localhost:5000`
- Frontend: `http://localhost:5173`

### Test Hanging
Kill existing Chrome processes:
```bash
taskkill /F /IM chrome.exe
taskkill /F /IM chromedriver.exe
```

## Test Configuration

- **Browser:** Chrome (headful mode)
- **Timeouts:** 15 seconds default
- **Screenshots:** Automatic on each step
- **Credentials:** Pre-configured test user