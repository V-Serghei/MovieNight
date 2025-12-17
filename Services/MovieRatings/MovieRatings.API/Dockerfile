FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY Services/MovieRatings/MovieRatings.API/MovieRatings.API.csproj Services/MovieRatings/MovieRatings.API/
COPY Services/MovieRatings/MovieRatings.Domain/MovieRatings.Domain.csproj Services/MovieRatings/MovieRatings.Domain/
COPY Services/MovieRatings/MovieRatings.Infrastructure/MovieRatings.Infrastructure.csproj Services/MovieRatings/MovieRatings.Infrastructure/

RUN dotnet restore Services/MovieRatings/MovieRatings.API/MovieRatings.API.csproj

COPY . .

WORKDIR /src/Services/MovieRatings/MovieRatings.API
RUN dotnet publish MovieRatings.API.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MovieRatings.API.dll"]
