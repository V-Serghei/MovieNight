FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# csproj (до restore) + зависимости
COPY Services/MoviePlayer/MoviePlayer.API/MoviePlayer.API.csproj Services/MoviePlayer/MoviePlayer.API/
COPY Services/MoviePlayer/MoviePlayer.Domain/MoviePlayer.Domain.csproj Services/MoviePlayer/MoviePlayer.Domain/

RUN dotnet restore Services/MoviePlayer/MoviePlayer.API/MoviePlayer.API.csproj

# весь код
COPY . .

WORKDIR /src/Services/MoviePlayer/MoviePlayer.API
RUN dotnet publish MoviePlayer.API.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MoviePlayer.API.dll"]
