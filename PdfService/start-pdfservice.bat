@echo off
title Sertifika PDF Servisi
cd /d %~dp0

rem Zaten calisiyor mu? 5050 portu LISTENING ise sessizce cik.
netstat -ano | findstr /R /C:"TCP.*127\.0\.0\.1:5050.*LISTENING" >nul
if %errorlevel%==0 (
    echo PDF Servisi zaten calisiyor (http://127.0.0.1:5050). Yeni instance baslatilmadi.
    timeout /t 2 >nul
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

echo PDF Servisi baslatiliyor (http://127.0.0.1:5050)...
echo Durdurmak icin CTRL+C veya pencereyi kapatin.
echo.
venv\Scripts\python.exe main.py
if errorlevel 1 (
    echo.
    echo [HATA] PDF servisi basarisiz oldu. Hata mesaji yukarida.
    pause
)
