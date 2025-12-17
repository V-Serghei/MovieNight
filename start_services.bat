@echo off
setlocal

echo ================================
echo  Starting MovieNight backend...
echo ================================

REM === APIs ===
start "Access.API" cmd /k "cd /d Services\Access\Access.API && dotnet run --launch-profile http"
start "Achievements.API" cmd /k "cd /d Services\Achievements\Achievements.API && dotnet run --launch-profile http"
start "Auth.API" cmd /k "cd /d Services\Auth\Auth.API && dotnet run --launch-profile http"
start "Bookmark.API" cmd /k "cd /d Services\Bookmark\Bookmark.API && dotnet run --launch-profile http"
start "Friends.API" cmd /k "cd /d Services\Friends\Friends.API && dotnet run --launch-profile http"
start "Messages.API" cmd /k "cd /d Services\Messages\Messages.API && dotnet run --launch-profile http"
start "MoviePlayer.API" cmd /k "cd /d Services\MoviePlayer\MoviePlayer.API && dotnet run --launch-profile http"
start "Review.API" cmd /k "cd /d Services\Review\Review.API && dotnet run --launch-profile http"
start "Users.API" cmd /k "cd /d Services\User\Users.API && dotnet run --launch-profile http"
start "Media.API" cmd /k "cd /d Services\Media\Media.API && dotnet run --launch-profile http"
start "People.API" cmd /k "cd /d Services\People\People.API && dotnet run --launch-profile http"
start "MovieRatings.API" cmd /k "cd /d Services\MovieRatings\MovieRatings.API && dotnet run --launch-profile http"

echo ================================
echo  Starting Gateway...
echo ================================

start "Gateway" cmd /k "cd /d MovieNight.Gateway && dotnet run --launch-profile http"

echo ================================
echo  Starting frontend (Next.js)...
echo ================================

start "MovieNight.UI" cmd /k "cd /d MovieNight.UI\clientapp && npm run dev"

echo ================================
echo  All services were started in separate windows.
echo ================================
pause
