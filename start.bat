@echo off
:: MovieNight — one-command local startup
:: Requires: Git for Windows (Git Bash), .NET 10 SDK, Docker Desktop, Node.js 18+
::
:: Install .NET 10 SDK if missing:
::   winget install Microsoft.DotNet.SDK.10

setlocal

:: Find Git Bash — check common install locations
set BASH=
if exist "C:\Program Files\Git\bin\bash.exe"       set BASH=C:\Program Files\Git\bin\bash.exe
if exist "C:\Program Files (x86)\Git\bin\bash.exe" set BASH=C:\Program Files (x86)\Git\bin\bash.exe

:: Also check PATH for git, then derive bash location from it
if "%BASH%"=="" (
    for /f "delims=" %%i in ('where git 2^>nul') do (
        if "%BASH%"=="" set BASH=%%~dpi..\bin\bash.exe
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

"%BASH%" --login "%~dp0start-local.sh"
