FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY Services/Friends/Friends.API/Friends.API.csproj Services/Friends/Friends.API/
COPY Services/Friends/Friends.Core/Friends.Core.csproj Services/Friends/Friends.Core/
COPY Services/Friends/Friends.Infrastructure/Friends.Infrastructure.csproj Services/Friends/Friends.Infrastructure/
COPY Services/Friends/Friends.Domain/Friends.Domain.csproj Services/Friends/Friends.Domain/

RUN dotnet restore Services/Friends/Friends.API/Friends.API.csproj

COPY . .

WORKDIR /src/Services/Friends/Friends.API
RUN dotnet publish Friends.API.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Friends.API.dll"]
