
# 1. Base Runtime with native multimedia dependencies
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

WORKDIR /app

EXPOSE 8080
EXPOSE 8081

RUN apt-get update && apt-get install -y --no-install-recommends \
    ffmpeg \
    libgomp1 \
    curl \
    && rm -rf /var/lib/apt/lists/*


# 2. Build Stage (Context at Solution Root)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project files for dependency restoration
COPY ["AIEngineConnectivity/AIEngineConnectivity.csproj", "AIEngineConnectivity/"]
COPY ["AIGatewayCore/AIGatewayCore/AIGatewayCore.csproj", "AIGatewayCore/AIGatewayCore/"]
COPY ["AIEngineCore/AIEngineCore.csproj", "AIEngineCore/"]
COPY ["AIEngineSpeechRecognition/AIEngineSpeechRecognition.csproj", "AIEngineSpeechRecognition/"]
COPY ["AIEngineGateway/AIEngineGateway.csproj", "AIEngineGateway/"]
COPY ["AIEngineBridge/AIEngineBridge.csproj", "AIEngineBridge/"]
COPY ["AIEngineRedis/AIEngineRedis.csproj", "AIEngineRedis/"]
COPY ["AIEngineHost/AIEngineHost.csproj", "AIEngineHost/"]

# Restore dependencies
RUN dotnet restore "AIEngineHost/AIEngineHost.csproj"

# Copy remaining source code
COPY . .

# Publish AIEngineHost
WORKDIR "/src/AIEngineHost"

RUN dotnet publish "AIEngineHost.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# 3. Final Production Runtime
FROM base AS final

WORKDIR /app

COPY --from=build /app/publish .

# Copy Angular frontend assets
COPY --from=build /src/AIEngineGateway/wwwroot ./wwwroot

ENV ASPNETCORE_URLS=http://+:8080

# Environment should be provided by Docker Compose / deployment
# Do not hardcode Development for production.

ENTRYPOINT ["dotnet", "AIEngineHost.dll"]
