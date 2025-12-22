FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY Services/People/People.API/People.API.csproj Services/People/People.API/
COPY Services/People/People.Core/People.Core.csproj Services/People/People.Core/
COPY Services/People/People.Infrastructure/People.Infrastructure.csproj Services/People/People.Infrastructure/
COPY Services/People/People.Domain/People.Domain.csproj Services/People/People.Domain/

RUN dotnet restore Services/People/People.API/People.API.csproj

COPY . .

WORKDIR /src/Services/People/People.API
RUN dotnet publish People.API.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "People.API.dll"]
