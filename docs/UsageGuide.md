# IronGo Usage Guide

This guide provides practical examples of using IronGo to parse and analyze Go source code.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Parsing Go Code](#parsing-go-code)
3. [Traversing the AST](#traversing-the-ast)
4. [Finding Specific Nodes](#finding-specific-nodes)
5. [Code Analysis](#code-analysis)
6. [JSON Export](#json-export)
7. [Error Handling](#error-handling)
8. [Performance Optimization](#performance-optimization)
9. [Advanced Scenarios](#advanced-scenarios)

## Getting Started

First, install IronGo via NuGet:

```bash
dotnet add package IronGo
```

Then import the namespace:

```csharp
using IronGo;
using IronGo.AST;
using IronGo.Utilities;
```

## Parsing Go Code

### Basic Parsing

```csharp
// Parse from string
var source = @"
package main

import ""fmt""

func main() {
    fmt.Println(""Hello, World!"")
}";

var ast = IronGoParser.Parse(source);
```

### Parse from File

```csharp
// Parse from file
var ast = IronGoParser.ParseFile("path/to/file.go");
```

### Parse with Options

```csharp
// Configure parser options
var options = new ParserOptions
{
    EnableCaching = true,      // Cache results for repeated parsing
    RunAnalyzer = true,        // Run code analysis
    ContinueOnError = false    // Stop on first error
};

var parser = new IronGoParser(options);
var ast = parser.ParseSource(source);
```

## Traversing the AST

### Using the Visitor Pattern

Create a custom visitor to traverse the AST:

```csharp
public class FunctionPrinter : GoAstWalker
{
    public override void VisitFunctionDeclaration(FunctionDeclaration node)
    {
        Console.WriteLine($"Function: {node.Name}");
        Console.WriteLine($"  Parameters: {node.Parameters.Count}");
        Console.WriteLine($"  Returns: {node.ReturnParameters?.Count ?? 0}");
        
        // Continue visiting child nodes
        base.VisitFunctionDeclaration(node);
    }
    
    public override void VisitMethodDeclaration(MethodDeclaration node)
    {
        Console.WriteLine($"Method: {node.Name}");
        Console.WriteLine($"  Receiver: {node.Receiver.Type}");
        
        base.VisitMethodDeclaration(node);
    }
}

// Use the visitor
var visitor = new FunctionPrinter();
ast.Accept(visitor);
```

### Collecting Information

Use a generic visitor to collect data:

```csharp
public class ImportCollector : GoAstVisitor<List<string>>
{
    private readonly List<string> _imports = new();
    
    public override List<string> VisitImportDeclaration(ImportDeclaration node)
    {
        foreach (var spec in node.Specs)
        {
            _imports.Add(spec.Path);
        }
        return _imports;
    }
    
    public override List<string> VisitSourceFile(SourceFile node)
    {
        foreach (var import in node.Imports)
        {
            import.Accept(this);
        }
        return _imports;
    }
    
    // Default implementation for other nodes
    protected override List<string> DefaultVisit(IGoNode node) => _imports;
}

var collector = new ImportCollector();
var imports = ast.Accept(collector);
```

## Finding Specific Nodes

### Find Functions

```csharp
// Get all functions
var functions = ast.GetFunctions();
foreach (var func in functions)
{
    Console.WriteLine($"Function: {func.Name}");
}

// Find specific function
var mainFunc = ast.FindFunction("main");
if (mainFunc != null)
{
    Console.WriteLine("Found main function");
}
```

### Find Types

```csharp
// Get all type declarations
var types = ast.GetTypes();
foreach (var type in types)
{
    Console.WriteLine($"Type: {type.Name}");
    
    if (type.Type is StructType structType)
    {
        Console.WriteLine($"  Struct with {structType.Fields.Count} fields");
    }
    else if (type.Type is InterfaceType interfaceType)
    {
        Console.WriteLine($"  Interface with {interfaceType.Methods.Count} methods");
    }
}
```

### Find All Nodes of a Type

```csharp
// Find all if statements
var ifStatements = ast.FindNodes<IfStatement>();
foreach (var ifStmt in ifStatements)
{
    Console.WriteLine("Found if statement");
}

// Find all function calls
var calls = ast.GetAllCalls();
foreach (var call in calls)
{
    if (call.Function is IdentifierExpression ident)
    {
        Console.WriteLine($"Call to: {ident.Name}");
    }
    else if (call.Function is SelectorExpression selector)
    {
        Console.WriteLine($"Method call: {selector.Member}");
    }
}
```

### Find Literals

```csharp
// Find all string literals
var strings = ast.GetLiterals(LiteralKind.String);
foreach (var str in strings)
{
    Console.WriteLine($"String literal: {str.Value}");
}

// Find all numeric literals
var numbers = ast.GetLiterals(LiteralKind.Int)
    .Concat(ast.GetLiterals(LiteralKind.Float));
foreach (var num in numbers)
{
    Console.WriteLine($"Number: {num.Value}");
}
```

## Code Analysis

### Analyze Function Complexity

```csharp
public class ComplexityAnalyzer : GoAstWalker
{
    private int _currentComplexity;
    private readonly Dictionary<string, int> _functionComplexity = new();
    private string? _currentFunction;
    
    public override void VisitFunctionDeclaration(FunctionDeclaration node)
    {
        _currentFunction = node.Name;
        _currentComplexity = 1; // Base complexity
        
        base.VisitFunctionDeclaration(node);
        
        _functionComplexity[node.Name] = _currentComplexity;
        _currentFunction = null;
    }
    
    public override void VisitIfStatement(IfStatement node)
    {
        _currentComplexity++;
        base.VisitIfStatement(node);
    }
    
    public override void VisitForStatement(ForStatement node)
    {
        _currentComplexity++;
        base.VisitForStatement(node);
    }
    
    public override void VisitSwitchStatement(SwitchStatement node)
    {
        _currentComplexity += node.Cases.Count;
        base.VisitSwitchStatement(node);
    }
    
    public void PrintReport()
    {
        Console.WriteLine("Cyclomatic Complexity Report:");
        foreach (var (func, complexity) in _functionComplexity)
        {
            Console.WriteLine($"  {func}: {complexity}");
        }
    }
}

var analyzer = new ComplexityAnalyzer();
ast.Accept(analyzer);
analyzer.PrintReport();
```

### Find Unused Imports

```csharp
public class UnusedImportFinder : GoAstWalker
{
    private readonly HashSet<string> _imports = new();
    private readonly HashSet<string> _usedPackages = new();
    
    public override void VisitImportDeclaration(ImportDeclaration node)
    {
        foreach (var spec in node.Specs)
        {
            var pkgName = spec.Alias ?? Path.GetFileName(spec.Path.Trim('"'));
            _imports.Add(pkgName);
        }
    }
    
    public override void VisitSelectorExpression(SelectorExpression node)
    {
        if (node.Object is IdentifierExpression ident)
        {
            _usedPackages.Add(ident.Name);
        }
        base.VisitSelectorExpression(node);
    }
    
    public IEnumerable<string> GetUnusedImports()
    {
        return _imports.Except(_usedPackages);
    }
}

var finder = new UnusedImportFinder();
ast.Accept(finder);
var unused = finder.GetUnusedImports();
Console.WriteLine($"Unused imports: {string.Join(", ", unused)}");
```

## JSON Export

### Basic JSON Export

```csharp
// Pretty-printed JSON
var prettyJson = ast.ToJsonPretty();
File.WriteAllText("ast-pretty.json", prettyJson);

// Compact JSON
var compactJson = ast.ToJsonCompact();
File.WriteAllText("ast-compact.json", compactJson);
```

### Selective JSON Export

```csharp
// Export only functions
var functions = ast.GetFunctions();
var functionsJson = JsonSerializer.Serialize(functions, new JsonSerializerOptions
{
    WriteIndented = true,
    Converters = { new AstJsonConverter() }
});
```

### Custom JSON Processing

```csharp
// Process JSON data
var json = ast.ToJson();
using var doc = JsonDocument.Parse(json);

// Navigate JSON
var root = doc.RootElement;
if (root.GetProperty("Type").GetString() == "SourceFile")
{
    var package = root.GetProperty("Package");
    var packageName = package.GetProperty("Name").GetString();
    Console.WriteLine($"Package: {packageName}");
}
```

## Error Handling

### Try-Parse Pattern

```csharp
if (IronGoParser.TryParse(source, out var ast, out var error))
{
    // Success - process AST
    Console.WriteLine($"Parsed successfully: {ast.Package?.Name}");
}
else
{
    // Failure - handle error
    Console.WriteLine($"Parse error: {error}");
}
```

### Exception Handling

```csharp
try
{
    var ast = IronGoParser.Parse(source);
}
catch (ParseException ex)
{
    Console.WriteLine($"Parse failed with {ex.Errors.Count} errors:");
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"  Line {error.Line}, Col {error.Column}: {error.Message}");
    }
}
```

### Parsing with Diagnostics

```csharp
var result = IronGoParser.ParseWithDiagnostics(source);

Console.WriteLine($"Parse time: {result.Diagnostics.ParseTimeMs}ms");
Console.WriteLine($"Errors: {result.Diagnostics.Errors.Count}");
Console.WriteLine($"Warnings: {result.Diagnostics.Warnings.Count}");

foreach (var warning in result.Diagnostics.Warnings)
{
    Console.WriteLine($"{warning.Level}: {warning.Message}");
}
```

## Performance Optimization

### Enable Caching

```csharp
// Caching is enabled by default
var parser = new IronGoParser();

// Parse same source multiple times
for (int i = 0; i < 100; i++)
{
    var ast = parser.ParseSource(source); // Very fast after first parse
}

// Check cache statistics
var stats = ParserCache.Default.GetStatistics();
Console.WriteLine($"Cache hits: {stats.TotalHits}");
Console.WriteLine($"Hit rate: {stats.HitRate:P}");
```

### Custom Cache Configuration

```csharp
// Create custom cache
var cache = new ParserCache(
    maxCacheSize: 50,
    cacheExpiration: TimeSpan.FromMinutes(10)
);

var options = new ParserOptions
{
    EnableCaching = true,
    Cache = cache
};

var parser = new IronGoParser(options);
```

### Disable Caching for Dynamic Code

```csharp
// Disable caching when parsing frequently changing code
var options = new ParserOptions
{
    EnableCaching = false
};

var parser = new IronGoParser(options);
```

## Advanced Scenarios

### Building a Code Formatter

```csharp
public class CodeFormatter : GoAstVisitor<string>
{
    private readonly StringBuilder _output = new();
    private int _indentLevel = 0;
    
    private void Write(string text) => _output.Append(text);
    private void WriteLine(string text = "") => _output.AppendLine(text);
    private void Indent() => _output.Append(new string(' ', _indentLevel * 4));
    
    public override string VisitSourceFile(SourceFile node)
    {
        if (node.Package != null)
        {
            node.Package.Accept(this);
            WriteLine();
        }
        
        foreach (var import in node.Imports)
        {
            import.Accept(this);
        }
        if (node.Imports.Count > 0) WriteLine();
        
        foreach (var decl in node.Declarations)
        {
            decl.Accept(this);
            WriteLine();
        }
        
        return _output.ToString();
    }
    
    public override string VisitPackageDeclaration(PackageDeclaration node)
    {
        Write($"package {node.Name}");
        return "";
    }
    
    // ... implement other visit methods
}
```

### Finding Dead Code

```csharp
public class DeadCodeFinder : GoAstWalker
{
    private readonly HashSet<string> _declaredFunctions = new();
    private readonly HashSet<string> _calledFunctions = new();
    
    public override void VisitFunctionDeclaration(FunctionDeclaration node)
    {
        _declaredFunctions.Add(node.Name);
        base.VisitFunctionDeclaration(node);
    }
    
    public override void VisitCallExpression(CallExpression node)
    {
        if (node.Function is IdentifierExpression ident)
        {
            _calledFunctions.Add(ident.Name);
        }
        base.VisitCallExpression(node);
    }
    
    public IEnumerable<string> GetUnusedFunctions()
    {
        // Exclude main and init as they're called by runtime
        return _declaredFunctions
            .Except(_calledFunctions)
            .Where(f => f != "main" && f != "init");
    }
}
```

### Building a Dependency Graph

```csharp
public class DependencyGraphBuilder : GoAstWalker
{
    public Dictionary<string, HashSet<string>> Dependencies { get; } = new();
    private string? _currentFunction;
    
    public override void VisitFunctionDeclaration(FunctionDeclaration node)
    {
        _currentFunction = node.Name;
        Dependencies[_currentFunction] = new HashSet<string>();
        base.VisitFunctionDeclaration(node);
        _currentFunction = null;
    }
    
    public override void VisitCallExpression(CallExpression node)
    {
        if (_currentFunction != null && node.Function is IdentifierExpression ident)
        {
            Dependencies[_currentFunction].Add(ident.Name);
        }
        base.VisitCallExpression(node);
    }
}

var builder = new DependencyGraphBuilder();
ast.Accept(builder);

foreach (var (func, deps) in builder.Dependencies)
{
    Console.WriteLine($"{func} calls: {string.Join(", ", deps)}");
}
```