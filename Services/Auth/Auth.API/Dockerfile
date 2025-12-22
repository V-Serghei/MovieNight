FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY Services/Auth/Auth.API/Auth.API.csproj Services/Auth/Auth.API/
COPY Services/Auth/Auth.Core/Auth.Core.csproj Services/Auth/Auth.Core/
COPY Services/Auth/Auth.Infrastructure/Auth.Infrastructure.csproj Services/Auth/Auth.Infrastructure/
COPY Services/Auth/Auth.Domain/Auth.Domain.csproj Services/Auth/Auth.Domain/

RUN dotnet restore Services/Auth/Auth.API/Auth.API.csproj

COPY . .

WORKDIR /src/Services/Auth/Auth.API
RUN dotnet publish Auth.API.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Auth.API.dll"]
