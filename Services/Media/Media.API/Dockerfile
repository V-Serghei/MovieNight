FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY Services/Media/Media.API/Media.API.csproj Services/Media/Media.API/
COPY Services/Media/Media.Core/Media.Core.csproj Services/Media/Media.Core/
COPY Services/Media/Media.Domain/Media.Domain.csproj Services/Media/Media.Domain/
COPY Services/Media/Media.Infrastructure/Media.Infrastructure.csproj Services/Media/Media.Infrastructure/

RUN dotnet restore Services/Media/Media.API/Media.API.csproj

COPY . .

WORKDIR /src/Services/Media/Media.API
RUN dotnet publish Media.API.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Media.API.dll"]
