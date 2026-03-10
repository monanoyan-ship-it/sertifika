@echo off
title Sertifika PDF Servisi
cd /d %~dp0

if not exist venv (
    echo Sanal ortam olusturuluyor...
    python -m venv venv
    call venv\Scripts\activate.bat
    pip install -r requirements.txt
) else (
    call venv\Scripts\activate.bat
)

echo PDF Servisi baslatiliyor (port 5050)...
python main.py
