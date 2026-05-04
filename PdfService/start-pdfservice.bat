@echo off
title Sertifika PDF Servisi
cd /d %~dp0

rem Port 5050 zaten LISTENING ise sessizce cikma yerine bilgilendir ve bekle.
for /f "tokens=*" %%A in ('netstat -an ^| findstr /C:"127.0.0.1:5050" ^| findstr /C:"LISTENING"') do (
    echo.
    echo PDF Servisi zaten calisiyor -^> http://127.0.0.1:5050
    echo Baska bir pencerede veya arka planda aktif. Yeni instance baslatilmadi.
    echo.
    echo Kapatmak icin bir tusa basin...
    pause >nul
    exit /b 0
)

if not exist venv (
    echo Sanal ortam olusturuluyor...
    python -m venv venv
    if errorlevel 1 (
        echo [HATA] venv olusturulamadi. Python yuklu mu? 'python --version' kontrol edin.
        pause
        exit /b 1
    )
    venv\Scripts\python.exe -m pip install --upgrade pip
    venv\Scripts\python.exe -m pip install -r requirements.txt
    if errorlevel 1 (
        echo [HATA] Bagimliliklar yuklenemedi.
        pause
        exit /b 1
    )
)

echo PDF Servisi baslatiliyor -^> http://127.0.0.1:5050
echo Durdurmak icin CTRL+C veya pencereyi kapatin.
echo.
venv\Scripts\python.exe main.py
echo.
echo Servis durdu (cikis kodu %errorlevel%).
pause
