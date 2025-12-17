FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY MovieNight.sln ./
COPY NuGet.config ./
COPY Directory.Build.props ./
COPY Directory.Packages.props ./

COPY MovieNight.Gateway/MovieNight.Gateway.csproj MovieNight.Gateway/
RUN dotnet restore MovieNight.Gateway/MovieNight.Gateway.csproj

COPY . .
WORKDIR /src/MovieNight.Gateway
RUN dotnet publish MovieNight.Gateway.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MovieNight.Gateway.dll"]
