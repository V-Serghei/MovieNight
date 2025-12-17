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

COPY Services/User/Users.API/Users.API.csproj Services/User/Users.API/
COPY Services/User/Users.Core/Users.Core.csproj Services/User/Users.Core/
COPY Services/User/Users.Infrastructure/Users.Infrastructure.csproj Services/User/Users.Infrastructure/
COPY Services/Common/SharedKernel/SharedKernel.csproj Services/Common/SharedKernel/

RUN dotnet restore Services/User/Users.API/Users.API.csproj

COPY . .
WORKDIR /src/Services/User/Users.API
RUN dotnet publish Users.API.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Users.API.dll"]
