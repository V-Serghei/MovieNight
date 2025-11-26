@echo off
setlocal

echo ==========================================
echo  Starting MovieNight backend (DEBUG/WATCH)
echo ==========================================

REM === APIs (dotnet watch run) ===
start "Access.API (watch)" cmd /k "cd /d Services\Access\Access.API && dotnet watch run --launch-profile http"
start "Achievements.API (watch)" cmd /k "cd /d Services\Achievements\Achievements.API && dotnet watch run --launch-profile http"
start "Auth.API (watch)" cmd /k "cd /d Services\Auth\Auth.API && dotnet watch run --launch-profile http"
REM start "Bookmark.API (watch)" cmd /k "cd /d Services\Bookmark\Bookmark.API && dotnet watch run --launch-profile http"
start "Friends.API (watch)" cmd /k "cd /d Services\Friends\Friends.API && dotnet watch run --launch-profile http"
REM start "Messages.API (watch)" cmd /k "cd /d Services\Messages\Messages.API && dotnet watch run --launch-profile http"
start "MoviePlayer.API (watch)" cmd /k "cd /d Services\MoviePlayer\MoviePlayer.API && dotnet watch run --launch-profile http"
REM start "Review.API (watch)" cmd /k "cd /d Services\Review\Review.API && dotnet watch run --launch-profile http"
start "Users.API (watch)" cmd /k "cd /d Services\User\Users.API && dotnet watch run --launch-profile http"

echo ================================
echo  Starting Gateway (WATCH)...
echo ================================

start "Gateway (watch)" cmd /k "cd /d MovieNight.Gateway && dotnet watch run --launch-profile http"

echo ================================
echo  Starting frontend (Next.js)...
echo ================================

start "MovieNight.UI" cmd /k "cd /d MovieNight.UI\clientapp && npm run dev"

echo ================================
echo  All services were started in DEBUG/WATCH mode.
echo ================================
pause
