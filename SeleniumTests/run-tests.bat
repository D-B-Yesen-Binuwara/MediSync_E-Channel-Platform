@echo off
echo ========================================
echo MediSync Selenium Test Runner
echo ========================================

echo.
echo Killing any existing Chrome/ChromeDriver processes...
taskkill /F /IM chrome.exe 2>nul
taskkill /F /IM chromedriver.exe 2>nul

echo.
echo Building test project...
dotnet build

if %ERRORLEVEL% NEQ 0 (
    echo Build failed! Exiting...
    pause
    exit /b 1
)

echo.
echo ========================================
echo Running Chrome Startup Tests
echo ========================================
dotnet test --filter "ChromeStartupTest" --logger "console;verbosity=detailed"

echo.
echo ========================================
echo Running Individual Tests
echo ========================================

echo.
echo 1. Testing Chrome Driver Info...
dotnet test --filter "Chrome_DriverInfo_ShouldBeDisplayed" --logger "console;verbosity=detailed"

echo.
echo 2. Testing Local App Access...
dotnet test --filter "LocalApp_ShouldBeAccessible" --logger "console;verbosity=detailed"

echo.
echo 3. Testing Login Flow...
dotnet test --filter "LoginFlow_WithValidCredentials_ShouldSucceed" --logger "console;verbosity=detailed"

echo.
echo ========================================
echo Test Results Summary
echo ========================================
echo Check the Reports folder for screenshots
echo.

pause