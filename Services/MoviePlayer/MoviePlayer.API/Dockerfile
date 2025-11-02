FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/MoviePlayer/MoviePlayer.API/MoviePlayer.API.csproj", "Services/MoviePlayer/MoviePlayer.API/"]
RUN dotnet restore "Services/MoviePlayer/MoviePlayer.API/MoviePlayer.API.csproj"
COPY . .
WORKDIR "/src/Services/MoviePlayer/MoviePlayer.API"
RUN dotnet build "./MoviePlayer.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MoviePlayer.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MoviePlayer.API.dll"]
