# IronGo

[![NuGet](https://img.shields.io/nuget/v/IronGo.svg)](https://www.nuget.org/packages/IronGo/)
[![License](https://img.shields.io/github/license/MarketAlly/IronGo.svg)](LICENSE)
[![Build Status](https://img.shields.io/github/actions/workflow/status/MarketAlly/IronGo/build.yml)](https://github.com/MarketAlly/IronGo/actions)

IronGo is a native .NET library for parsing Go source code. It provides a complete Abstract Syntax Tree (AST) representation of Go programs with comprehensive support for Go 1.21+ syntax, including generics.

## Features

- **Complete Go Parser**: Full support for Go 1.21 syntax including generics
- **Strongly-Typed AST**: Native .NET classes for all Go language constructs
- **Visitor Pattern**: Both void and generic visitor interfaces for AST traversal
- **JSON Serialization**: Export AST to JSON for tooling integration
- **Performance Optimized**: Built-in caching and efficient parsing
- **Comprehensive Diagnostics**: Detailed error reporting and code analysis
- **Cross-Platform**: Works on Windows, Linux, and macOS
- **Container-Ready**: Optimized for cloud and containerized environments

## Installation

Install IronGo via NuGet:

```bash
dotnet add package IronGo
```

Or via Package Manager Console:

```powershell
Install-Package IronGo
```

## Quick Start

### Basic Parsing

```csharp
using IronGo;

// Parse Go source code
var source = @"
package main

import ""fmt""

func main() {
    fmt.Println(""Hello, World!"")
}";

var ast = IronGoParser.Parse(source);

// Access AST nodes
Console.WriteLine($"Package: {ast.Package.Name}");
Console.WriteLine($"Imports: {string.Join(", ", ast.GetImportedPackages())}");
Console.WriteLine($"Functions: {ast.GetFunctions().Count()}");
```

### Parsing with Diagnostics

```csharp
// Get detailed parsing information
var result = IronGoParser.ParseWithDiagnostics(source);

Console.WriteLine($"Parse time: {result.Diagnostics.ParseTimeMs}ms");
Console.WriteLine($"Token count: {result.Diagnostics.TokenCount}");
Console.WriteLine($"Errors: {result.Diagnostics.Errors.Count}");
Console.WriteLine($"Warnings: {result.Diagnostics.Warnings.Count}");
```

### Using the Visitor Pattern

```csharp
// Count all function calls in the code
public class CallCounter : GoAstWalker
{
    public int CallCount { get; private set; }
    
    public override void VisitCallExpression(CallExpression node)
    {
        CallCount++;
        base.VisitCallExpression(node);
    }
}

var counter = new CallCounter();
ast.Accept(counter);
Console.WriteLine($"Function calls: {counter.CallCount}");
```

### JSON Serialization

```csharp
// Export AST to JSON
var json = ast.ToJsonPretty();
File.WriteAllText("ast.json", json);

// Compact JSON for transmission
var compactJson = ast.ToJsonCompact();
```

### Advanced Usage

```csharp
// Custom parser options
var options = new ParserOptions
{
    EnableCaching = true,
    RunAnalyzer = true,
    ContinueOnError = false,
    ErrorRecoveryMode = ErrorRecoveryMode.Default
};

var parser = new IronGoParser(options);
var ast = parser.ParseSource(source);

// Find specific nodes
var mainFunc = ast.FindFunction("main");
var allCalls = ast.GetAllCalls();
var stringLiterals = ast.GetLiterals(LiteralKind.String);

// Check imports
if (ast.IsPackageImported("fmt"))
{
    Console.WriteLine("fmt package is imported");
}
```

## AST Structure

IronGo provides a complete AST representation with the following key types:

### Declarations
- `FunctionDeclaration` - Regular functions
- `MethodDeclaration` - Methods with receivers
- `TypeDeclaration` - Type definitions
- `VariableDeclaration` - Variable declarations
- `ConstDeclaration` - Constant declarations

### Types
- `StructType` - Struct definitions
- `InterfaceType` - Interface definitions
- `SliceType` - Slice types
- `ArrayType` - Array types
- `MapType` - Map types
- `ChannelType` - Channel types
- `FunctionType` - Function signatures
- `PointerType` - Pointer types

### Statements
- `BlockStatement` - Statement blocks
- `IfStatement` - If/else statements
- `ForStatement` - For loops
- `ForRangeStatement` - Range loops
- `SwitchStatement` - Switch statements
- `SelectStatement` - Select statements
- `DeferStatement` - Defer statements
- `GoStatement` - Goroutine launches
- `ReturnStatement` - Return statements

### Expressions
- `BinaryExpression` - Binary operations
- `UnaryExpression` - Unary operations
- `CallExpression` - Function calls
- `IndexExpression` - Array/slice indexing
- `SelectorExpression` - Field/method selection
- `TypeAssertionExpression` - Type assertions
- `CompositeLiteral` - Composite literals
- `FunctionLiteral` - Anonymous functions
- `LiteralExpression` - Literals (string, int, float, etc.)

## Utility Methods

IronGo includes many utility extension methods:

```csharp
// Find all functions
var functions = ast.GetFunctions();

// Find all method declarations
var methods = ast.GetMethods();

// Find all type declarations
var types = ast.GetTypes();

// Find specific function by name
var mainFunc = ast.FindFunction("main");

// Get all imported packages
var imports = ast.GetImportedPackages();

// Find all identifiers
var identifiers = ast.GetAllIdentifiers();

// Find all function calls
var calls = ast.GetAllCalls();

// Get literals by type
var strings = ast.GetLiterals(LiteralKind.String);
var numbers = ast.GetLiterals(LiteralKind.Int);

// Count total nodes
var nodeCount = ast.CountNodes();
```

## Performance

IronGo includes built-in performance optimizations:

- **Parser Caching**: Automatically caches parsed results
- **Efficient Grammar**: Optimized ANTLR4 grammar
- **Minimal Allocations**: Designed to reduce GC pressure

```csharp
// Caching is enabled by default
var parser = new IronGoParser();

// First parse - cache miss
var ast1 = parser.ParseSource(source);

// Second parse - cache hit (very fast)
var ast2 = parser.ParseSource(source);

// Get cache statistics
var stats = ParserCache.Default.GetStatistics();
Console.WriteLine($"Cache hit rate: {stats.HitRate:P}");
```

## Error Handling

IronGo provides comprehensive error handling:

```csharp
// Try parse pattern
if (IronGoParser.TryParse(source, out var ast, out var error))
{
    // Success - use ast
}
else
{
    Console.WriteLine($"Parse error: {error}");
}

// Exception-based parsing
try
{
    var ast = IronGoParser.Parse(source);
}
catch (ParseException ex)
{
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"{error.Line}:{error.Column}: {error.Message}");
    }
}
```

## Code Analysis

IronGo includes a built-in code analyzer:

```csharp
var options = new ParserOptions { RunAnalyzer = true };
var parser = new IronGoParser(options);
var result = parser.ParseSourceWithDiagnostics(source);

foreach (var warning in result.Diagnostics.Warnings)
{
    Console.WriteLine($"{warning.Level}: {warning.Message} at {warning.Position}");
}
```

The analyzer detects:
- Empty function bodies
- Functions with too many parameters
- Duplicate imports
- Unused-looking variables (starting with underscore)
- Infinite loops without break statements
- Empty if-statement clauses

## Requirements

- .NET 9.0 or later
- No external dependencies beyond ANTLR4 runtime

## Building from Source

```bash
# Clone the repository
git clone https://github.com/MarketAlly/IronGo.git
cd IronGo

# Build the solution
dotnet build

# Run tests
dotnet test

# Create NuGet package
dotnet pack src/IronGo/IronGo.csproj -c Release
```

## Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built using [ANTLR4](https://www.antlr.org/) parser generator
- Go grammar adapted from [antlr/grammars-v4](https://github.com/antlr/grammars-v4)
- Inspired by the official Go AST package

## Support

- **Documentation**: See the [Wiki](https://github.com/MarketAlly/IronGo/wiki)
- **Issues**: Report bugs on [GitHub Issues](https://github.com/MarketAlly/IronGo/issues)
- **Discussions**: Join our [GitHub Discussions](https://github.com/MarketAlly/IronGo/discussions)

## Roadmap

- [ ] Support for Go 1.22+ features
- [ ] Language Server Protocol (LSP) implementation
- [ ] Code generation capabilities
- [ ] More advanced code analysis rules
- [ ] Integration with Roslyn analyzers