FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY Services/Bookmark/Bookmark.API/Bookmark.API.csproj Services/Bookmark/Bookmark.API/
COPY Services/Bookmark/Bookmark.Domain/Bookmark.Domain.csproj Services/Bookmark/Bookmark.Domain/
COPY Services/Bookmark/Bookmark.Infrastructure/Bookmark.Infrastructure.csproj Services/Bookmark/Bookmark.Infrastructure/

RUN dotnet restore Services/Bookmark/Bookmark.API/Bookmark.API.csproj

COPY . .

WORKDIR /src/Services/Bookmark/Bookmark.API
RUN dotnet publish Bookmark.API.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Bookmark.API.dll"]
