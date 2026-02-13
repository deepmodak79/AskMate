@echo off
echo.
echo ============================================================
echo   Deep Overflow - Live Demo
echo ============================================================
echo.
echo   Starting server on http://localhost:8080
echo.
echo   Opening browser...
echo.
echo   Press Ctrl+C to stop the server
echo.
echo ============================================================
echo.

start http://localhost:8080

python -m http.server 8080 --directory "%~dp0"
