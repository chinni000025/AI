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

COPY ["AIEngineConnectivity/AIEngineConnectivity.csproj", "AIEngineConnectivity/"]
COPY ["AIGatewayCore/AIGatewayCore/AIGatewayCore.csproj", "AIGatewayCore/AIGatewayCore/"]
COPY ["AIEngineCore/AIEngineCore.csproj", "AIEngineCore/"]
COPY ["AIEngineSpeechRecognition/AIEngineSpeechRecognition.csproj", "AIEngineSpeechRecognition/"]
COPY ["AIEngineGateway/AIEngineGateway.csproj", "AIEngineGateway/"]
COPY ["AIEngineBridge/AIEngineBridge.csproj", "AIEngineBridge/"]
COPY ["AIEngineHost/AIEngineHost.csproj", "AIEngineHost/"]

RUN dotnet restore "AIEngineHost/AIEngineHost.csproj"

COPY . .

WORKDIR "/src/AIEngineHost"
RUN dotnet publish "AIEngineHost.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 3. Final Production Runtime
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Copy Angular frontend assets from AIEngineGateway into /app/wwwroot
COPY --from=build /src/AIEngineGateway/wwwroot ./wwwroot

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "AIEngineHost.dll"]
