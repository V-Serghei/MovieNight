FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# csproj графа зависимостей (до restore)
COPY Services/Messages/Messages.API/Messages.API.csproj Services/Messages/Messages.API/
COPY Services/Messages/Messages.Core/Messages.Core.csproj Services/Messages/Messages.Core/
COPY Services/Messages/Messages.Infrastructure/Messages.Infrastructure.csproj Services/Messages/Messages.Infrastructure/
COPY Services/Messages/Messages.Domain/Messages.Domain.csproj Services/Messages/Messages.Domain/

RUN dotnet restore Services/Messages/Messages.API/Messages.API.csproj

# весь код
COPY . .

WORKDIR /src/Services/Messages/Messages.API
RUN dotnet publish Messages.API.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Messages.API.dll"]
