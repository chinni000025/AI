#!/bin/bash
set -e

CONFIG="${1:-Release}"

echo "==================================================="
echo "Building Linux Installer Package ($CONFIG)"
echo "==================================================="

echo ""
echo "Step 1: Building AIEngineConnectivity..."
dotnet build AIEngineConnectivity/AIEngineConnectivity.slnx -c "$CONFIG"
mkdir -p AIEngineGateway/libs AIEngineInstaller/libs
cp -f AIEngineConnectivity/bin/"$CONFIG"/net10.0/AIEngineConnectivity.* AIEngineGateway/libs/ 2>/dev/null || true
cp -f AIEngineConnectivity/bin/"$CONFIG"/net10.0/AIEngineConnectivity.* AIEngineInstaller/libs/ 2>/dev/null || true

echo ""
echo "Step 2: Building AIEngineCore..."
dotnet build AIEngineCore/AIEngineCore.slnx -c "$CONFIG"
mkdir -p AIEngineGateway/libs AIEngineInstaller/libs
cp -f AIEngineCore/bin/"$CONFIG"/net10.0/AIEngineCore.* AIEngineGateway/libs/ 2>/dev/null || true
cp -f AIEngineCore/bin/"$CONFIG"/net10.0/AIEngineCore.* AIEngineInstaller/libs/ 2>/dev/null || true

echo ""
echo "Step 3: Building AIEngineClient (Angular Web App)..."
cd AIEngineClient
npm run build
cd ..

echo ""
echo "Step 4: Publishing AIEngineGateway for Linux (linux-x64)..."
dotnet publish AIEngineGateway/AIEngineGateway.csproj -c "$CONFIG" -r linux-x64 -o "linux-installer/AIEngineGateway"

echo ""
echo "Step 5: Publishing AIEngineInstaller for Linux (linux-x64)..."
dotnet publish AIEngineInstaller/AIEngineInstaller.csproj -c "$CONFIG" -r linux-x64 -o "linux-installer"

echo ""
echo "==================================================="
echo "Linux Installer package ready in ./linux-installer/"
echo "==================================================="
