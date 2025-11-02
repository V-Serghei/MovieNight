FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/Achievements/Achievements.API/Achievements.API.csproj", "Services/Achievements/Achievements.API/"]
RUN dotnet restore "Services/Achievements/Achievements.API/Achievements.API.csproj"
COPY . .
WORKDIR "/src/Services/Achievements/Achievements.API"
RUN dotnet build "./Achievements.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Achievements.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Achievements.API.dll"]
