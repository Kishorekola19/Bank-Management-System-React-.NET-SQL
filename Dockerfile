# Multi-stage Dockerfile for Full-Stack ASP.NET Core + React App
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project references
COPY ["EnterpriseBankingSystem.sln", "./"]
COPY ["src/EnterpriseBankingSystem.API/EnterpriseBankingSystem.API.csproj", "src/EnterpriseBankingSystem.API/"]
COPY ["src/EnterpriseBankingSystem.Application/EnterpriseBankingSystem.Application.csproj", "src/EnterpriseBankingSystem.Application/"]
COPY ["src/EnterpriseBankingSystem.Domain/EnterpriseBankingSystem.Domain.csproj", "src/EnterpriseBankingSystem.Domain/"]
COPY ["src/EnterpriseBankingSystem.Infrastructure/EnterpriseBankingSystem.Infrastructure.csproj", "src/EnterpriseBankingSystem.Infrastructure/"]
COPY ["src/EnterpriseBankingSystem.Web/EnterpriseBankingSystem.Web.csproj", "src/EnterpriseBankingSystem.Web/"]

RUN dotnet restore "EnterpriseBankingSystem.sln"

# Install Node.js for React build
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

# Copy all source files and build React web frontend
COPY . .
WORKDIR /src/src/EnterpriseBankingSystem.Web
RUN npm ci && npm run build

# Publish ASP.NET Core Web API
WORKDIR /src
RUN dotnet publish "src/EnterpriseBankingSystem.API/EnterpriseBankingSystem.API.csproj" -c Release -o /app/publish

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EnterpriseBankingSystem.API.dll"]
