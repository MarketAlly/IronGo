Write-Host "IronGo Parser Generator" -ForegroundColor Green
Write-Host "======================" -ForegroundColor Green
Write-Host ""

# Check if Java is installed
try {
    $javaVersion = java -version 2>&1 | Select-String "version"
    Write-Host "Java found: $javaVersion"
    Write-Host ""
} catch {
    Write-Host "Error: Java is not installed." -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install Java first:"
    Write-Host "  - Download from: https://www.java.com"
    Write-Host "  - Or use: winget install Oracle.JavaRuntimeEnvironment"
    exit 1
}

# Download ANTLR4 if not present
$antlrVersion = "4.13.1"
$antlrJar = "antlr-$antlrVersion-complete.jar"
$antlrUrl = "https://www.antlr.org/download/$antlrJar"

if (!(Test-Path $antlrJar)) {
    Write-Host "Downloading ANTLR4 $antlrVersion..."
    Invoke-WebRequest -Uri $antlrUrl -OutFile $antlrJar
}

# Generate parser
Write-Host "Generating C# parser from grammar files..."
Push-Location src\IronGo\Parser

$result = java -jar "..\..\..\$antlrJar" `
    -Dlanguage=CSharp `
    -no-listener `
    -visitor `
    -package IronGo.Parser `
    GoLexer.g4 GoParser.g4

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Parser generated successfully!" -ForegroundColor Green
    Write-Host "Generated files:"
    Get-ChildItem *.cs | Where-Object { $_.Name -notlike "*Base.cs" } | ForEach-Object { Write-Host "  $_" }
    Write-Host ""
    Write-Host "You can now build the project with:"
    Write-Host "  dotnet build" -ForegroundColor Cyan
} else {
    Write-Host ""
    Write-Host "Error: Parser generation failed" -ForegroundColor Red
    Pop-Location
    exit 1
}

Pop-Location