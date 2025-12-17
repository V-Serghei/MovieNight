FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY Services/Access/Access.API/Access.API.csproj Services/Access/Access.API/
COPY Services/Access/Access.Infrastructure/Access.Infrastructure.csproj Services/Access/Access.Infrastructure/
COPY Services/Access/Access.Core/Access.Core.csproj Services/Access/Access.Core/

RUN dotnet restore Services/Access/Access.API/Access.API.csproj

COPY . .

WORKDIR /src/Services/Access/Access.API
RUN dotnet publish Access.API.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Access.API.dll"]
