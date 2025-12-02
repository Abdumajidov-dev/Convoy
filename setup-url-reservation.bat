@echo off
echo ====================================
echo  Convoy API - URL Reservation Setup
echo ====================================
echo.
echo Bu script Admin huquqlarini talab qiladi!
echo.

REM Check for admin rights
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [ERROR] Bu script Admin sifatida ishga tushirilishi kerak!
    echo.
    echo O'ng tugma bosing va "Run as administrator" tanlang
    echo.
    pause
    exit /b 1
)

echo [INFO] Admin huquqlari tasdiqlandi
echo.

echo [1/3] HTTP URL Reservation qo'shilmoqda...
echo.

REM Remove old reservations if exists
netsh http delete urlacl url=http://*:5084/ >nul 2>&1

REM Add HTTP reservation
netsh http add urlacl url=http://*:5084/ user=Everyone

if %errorLevel% equ 0 (
    echo [SUCCESS] HTTP URL Reservation muvaffaqiyatli qo'shildi!
    echo.
    echo URL: http://*:5084/
    echo User: Everyone
    echo.
) else (
    echo [ERROR] HTTP URL Reservation qo'shilmadi!
    echo.
    pause
    exit /b 1
)

echo [2/3] HTTPS URL Reservation qo'shilmoqda...
echo.

REM Remove old HTTPS reservation if exists
netsh http delete urlacl url=https://*:7147/ >nul 2>&1

REM Add HTTPS reservation
netsh http add urlacl url=https://*:7147/ user=Everyone

if %errorLevel% equ 0 (
    echo [SUCCESS] HTTPS URL Reservation muvaffaqiyatli qo'shildi!
    echo.
    echo URL: https://*:7147/
    echo User: Everyone
    echo.
) else (
    echo [WARNING] HTTPS URL Reservation qo'shilmadi (bu normal bo'lishi mumkin)
    echo.
)

echo [3/3] URL Reservations tekshirilmoqda...
echo.
echo HTTP:
netsh http show urlacl url=http://*:5084/
echo.
echo HTTPS:
netsh http show urlacl url=https://*:7147/
echo.

echo ====================================
echo  Setup Tugadi!
echo ====================================
echo.
echo Endi Visual Studio'ni ODDIY USER sifatida ishga tushirishingiz mumkin!
echo Network URL ishlaydi: http://10.100.104.128:5084
echo.
echo VS'ni oching va run qiling (Ctrl+F5)
echo.
pause
