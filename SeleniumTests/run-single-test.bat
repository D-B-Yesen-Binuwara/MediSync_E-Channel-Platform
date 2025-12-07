@echo off
echo ========================================
echo MediSync Single Test Runner
echo ========================================

echo.
echo Cleaning up existing processes...
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
echo Running Complete E-Channeling Flow Test
echo ========================================

dotnet test --filter "Complete_EChanneling_Flow_Single_Browser" --logger "console;verbosity=detailed"

echo.
echo ========================================
echo Test Complete
echo ========================================
echo Check Reports folder for screenshots
echo.

pause