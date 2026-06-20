@echo off
:: MovieNight — stop all running services + SQL Server container
::
:: Usage:
::   stop.bat            — stop everything including SQL Server container
::   stop.bat --keep-db  — stop services but leave SQL Server running

setlocal

set "BASH="
if exist "C:\Program Files\Git\bin\bash.exe"       set "BASH=C:\Program Files\Git\bin\bash.exe"
if exist "C:\Program Files (x86)\Git\bin\bash.exe" set "BASH=C:\Program Files (x86)\Git\bin\bash.exe"

if "%BASH%"=="" (
    for /f "delims=" %%i in ('where git 2^>nul') do (
        if "%BASH%"=="" set "BASH=%%~dpi..\bin\bash.exe"
    )
)

if "%BASH%"=="" (
    echo.
    echo  ERROR: Git Bash not found.
    echo  Install Git for Windows from https://git-scm.com
    echo.
    pause
    exit /b 1
)

:: By default stop everything including Docker container
:: Pass --keep-db to leave the container running
if "%1"=="--keep-db" (
    call "%BASH%" --noprofile --norc "%~dp0stop-local.sh"
) else (
    call "%BASH%" --noprofile --norc "%~dp0stop-local.sh" --docker
)
exit /b %ERRORLEVEL%
