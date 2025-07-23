#!/bin/bash

echo "IronGo Build Script"
echo "=================="

# Check if Java is installed (needed for ANTLR4)
if ! command -v java &> /dev/null; then
    echo "Warning: Java is not installed. ANTLR4 grammar generation will be skipped."
    echo "To generate parser from grammar files, please install Java first."
    echo ""
fi

# Clean previous build
echo "Cleaning previous build..."
dotnet clean -c Release

# Restore packages
echo "Restoring NuGet packages..."
dotnet restore

# Build the solution
echo "Building solution..."
dotnet build -c Release --no-restore

# Run tests
echo "Running tests..."
dotnet test -c Release --no-build --verbosity normal

# Pack NuGet package
echo "Creating NuGet package..."
dotnet pack src/IronGo/IronGo.csproj -c Release --no-build -o ./nupkg

echo ""
echo "Build complete!"
echo "NuGet package created in ./nupkg directory"