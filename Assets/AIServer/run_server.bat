@echo off
title AI Server - FastAPI
cd /d "%~dp0"
echo ====================================================
echo   Menjalankan AI Server di http://localhost:8000
echo ====================================================
python -m uvicorn server:app --reload --port 8000
pause
