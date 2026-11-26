@echo off
set "SITE=%~dp0"

if exist "%SITE%app_offline.htm" (
    del "%SITE%app_offline.htm"
    echo Maintenance OFF - site is LIVE
) else if exist "%SITE%maintenance.html" (
    copy "%SITE%maintenance.html" "%SITE%app_offline.htm" >nul
    echo Maintenance ON - site is OFFLINE
) else (
    echo ERROR: maintenance.html not found in %SITE%
)
pause