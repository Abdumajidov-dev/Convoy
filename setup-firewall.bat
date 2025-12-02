@echo off
echo ====================================
echo  Convoy API - Firewall Setup
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

echo [1/3] Firewall rule qo'shilmoqda...
echo.

REM Remove old rule if exists
netsh advfirewall firewall delete rule name="Convoy API Port 5084" >nul 2>&1

REM Add new rule
netsh advfirewall firewall add rule name="Convoy API Port 5084" dir=in action=allow protocol=TCP localport=5084

if %errorLevel% equ 0 (
    echo [SUCCESS] Firewall rule muvaffaqiyatli qo'shildi!
    echo.
    echo Rule Name: Convoy API Port 5084
    echo Port: 5084
    echo Protocol: TCP
    echo Direction: Inbound
    echo Action: Allow
    echo.
) else (
    echo [ERROR] Firewall rule qo'shilmadi!
    echo.
    pause
    exit /b 1
)

echo [2/3] Firewall rule tekshirilmoqda...
echo.
netsh advfirewall firewall show rule name="Convoy API Port 5084"
echo.

echo [3/3] Firewall holati...
echo.
netsh advfirewall show currentprofile state
echo.

echo ====================================
echo  Setup Tugadi!
echo ====================================
echo.
echo Endi API'ni ishga tushiring:
echo   start-api.bat
echo.
echo Flutter'dan test qiling:
echo   http://10.100.104.128:5084
echo.
pause
