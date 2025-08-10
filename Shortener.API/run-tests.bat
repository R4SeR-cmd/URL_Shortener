@echo off
echo ========================================
echo URL Shortener Backend Tests
echo ========================================

echo.
echo Building solution...
dotnet build

echo.
echo Running all tests...
dotnet test --verbosity normal

echo.
echo Tests completed!
pause
