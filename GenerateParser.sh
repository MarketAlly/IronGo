#!/bin/bash

echo "IronGo Parser Generator"
echo "======================"
echo ""

# Check if Java is installed
if ! command -v java &> /dev/null; then
    echo "Error: Java is not installed."
    echo ""
    echo "Please install Java first:"
    echo "  - macOS: brew install openjdk"
    echo "  - Ubuntu/Debian: sudo apt-get install default-jdk"
    echo "  - Other: Visit https://www.java.com"
    exit 1
fi

echo "Java found: $(java -version 2>&1 | head -n 1)"
echo ""

# Download ANTLR4 if not present
ANTLR_VERSION="4.13.1"
ANTLR_JAR="antlr-${ANTLR_VERSION}-complete.jar"
ANTLR_URL="https://www.antlr.org/download/${ANTLR_JAR}"

if [ ! -f "$ANTLR_JAR" ]; then
    echo "Downloading ANTLR4 ${ANTLR_VERSION}..."
    curl -O "$ANTLR_URL"
fi

# Generate parser
echo "Generating C# parser from grammar files..."
cd src/IronGo/Parser

java -jar "../../../${ANTLR_JAR}" \
    -Dlanguage=CSharp \
    -no-listener \
    -visitor \
    -package IronGo.Parser \
    GoLexer.g4 GoParser.g4

if [ $? -eq 0 ]; then
    echo ""
    echo "Parser generated successfully!"
    echo "Generated files:"
    ls -la *.cs | grep -v Base.cs
    echo ""
    echo "You can now build the project with:"
    echo "  dotnet build"
else
    echo ""
    echo "Error: Parser generation failed"
    exit 1
fi