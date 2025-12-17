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

COPY Services/Review/Review.API/Review.API.csproj Services/Review/Review.API/
COPY Services/Review/Review.Core/Review.Core.csproj Services/Review/Review.Core/
COPY Services/Review/Review.Infrastructure/Review.Infrastructure.csproj Services/Review/Review.Infrastructure/
COPY Services/Review/Review.Domain/Review.Domain.csproj Services/Review/Review.Domain/

RUN dotnet restore Services/Review/Review.API/Review.API.csproj

COPY . .

WORKDIR /src/Services/Review/Review.API
RUN dotnet publish Review.API.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Review.API.dll"]
