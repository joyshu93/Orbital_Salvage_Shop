@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0test-unity.ps1" %*
exit /b %ERRORLEVEL%
