@echo off
echo ========================================
echo MediSync Selenium Test Suite
echo ========================================

echo.
echo Cleaning up existing processes...
taskkill /F /IM chrome.exe 2>nul
taskkill /F /IM chromedriver.exe 2>nul

echo.
echo Building test project...
dotnet clean
dotnet restore
dotnet build

if %ERRORLEVEL% NEQ 0 (
    echo Build failed! Exiting...
    pause
    exit /b 1
)

echo.
echo ========================================
echo Running All Tests
echo ========================================

echo.
echo 1. Chrome Startup Tests...
dotnet test --filter "ChromeStartupTest" --logger "console;verbosity=normal"

echo.
echo 2. Login Flow Test...
dotnet test --filter "Login_Complete_Flow_Test" --logger "console;verbosity=normal"

echo.
echo 3. Complete E-Channeling Flow...
dotnet test --filter "Complete_EChanneling_Flow_Single_Browser" --logger "console;verbosity=normal"

echo.
echo ========================================
echo Test Suite Complete
echo ========================================
echo Check Reports folder for screenshots
dir /b Reports\*.png 2>nul | find /c ".png" > temp.txt
set /p count=<temp.txt
del temp.txt
echo Screenshots captured: %count%
echo.

pause