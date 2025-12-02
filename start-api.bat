@echo off
echo ====================================
echo  Convoy API - Starting...
echo ====================================
echo.
echo Port: 5084 (HTTP)
echo IP: 10.100.104.128
echo.
echo Flutter URL: http://10.100.104.128:5084
echo Swagger: http://10.100.104.128:5084/swagger
echo.
echo ====================================
echo.

cd /d "%~dp0"
dotnet run --launch-profile http

pause
