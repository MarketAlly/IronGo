# IronGo API Documentation

## Core Classes

### IronGoParser

The main entry point for parsing Go source code.

#### Static Methods

```csharp
// Parse Go source code from a string
public static SourceFile Parse(string source)

// Parse Go source code from a file
public static SourceFile ParseFile(string filePath)

// Parse Go source code from a stream
public static SourceFile Parse(Stream stream)

// Parse Go source code from a TextReader
public static SourceFile Parse(TextReader reader)

// Parse with diagnostic information
public static ParseResult ParseWithDiagnostics(string source)

// Try to parse, returns success/failure
public static bool TryParse(string source, out SourceFile? result, out string? error)
```

#### Instance Methods

```csharp
// Create parser with custom options
public IronGoParser(ParserOptions options)

// Parse source code
public SourceFile ParseSource(string source)

// Parse source file
public SourceFile ParseSourceFile(string filePath)

// Parse from stream
public SourceFile ParseStream(Stream stream)

// Parse from reader
public SourceFile ParseReader(TextReader reader)

// Parse with diagnostics
public ParseResult ParseSourceWithDiagnostics(string source)
```

### ParserOptions

Configuration options for the parser.

```csharp
public class ParserOptions
{
    // Enable caching of parse results (default: true)
    public bool EnableCaching { get; set; }
    
    // Custom cache instance (null to use default)
    public ParserCache? Cache { get; set; }
    
    // Run AST analyzer for additional diagnostics (default: true)
    public bool RunAnalyzer { get; set; }
    
    // Continue parsing even if errors are encountered (default: false)
    public bool ContinueOnError { get; set; }
    
    // Error recovery mode (default: Default)
    public ErrorRecoveryMode ErrorRecoveryMode { get; set; }
}
```

### SourceFile

The root node of the AST representing a complete Go source file.

```csharp
public class SourceFile : IGoNode
{
    // Package declaration
    public PackageDeclaration? Package { get; }
    
    // Import declarations
    public List<ImportDeclaration> Imports { get; }
    
    // Top-level declarations
    public List<IDeclaration> Declarations { get; }
    
    // Position information
    public Position Start { get; }
    public Position End { get; }
}
```

## AST Node Types

### Base Interfaces

```csharp
// Base interface for all AST nodes
public interface IGoNode
{
    Position Start { get; }
    Position End { get; }
    void Accept(IGoAstVisitor visitor);
    T Accept<T>(IGoAstVisitor<T> visitor);
}

// Base for all expressions
public interface IExpression : IGoNode { }

// Base for all statements
public interface IStatement : IGoNode { }

// Base for all declarations
public interface IDeclaration : IGoNode { }

// Base for all types
public interface IType : IGoNode { }
```

### Common Properties

All AST nodes include:
- `Start` - Starting position in source
- `End` - Ending position in source
- `Accept()` methods for visitor pattern

## Visitor Pattern

### IGoAstVisitor

Void visitor interface for AST traversal.

```csharp
public interface IGoAstVisitor
{
    void VisitSourceFile(SourceFile node);
    void VisitPackageDeclaration(PackageDeclaration node);
    void VisitImportDeclaration(ImportDeclaration node);
    void VisitFunctionDeclaration(FunctionDeclaration node);
    void VisitMethodDeclaration(MethodDeclaration node);
    void VisitTypeDeclaration(TypeDeclaration node);
    // ... methods for all node types
}
```

### IGoAstVisitor<T>

Generic visitor interface that returns values.

```csharp
public interface IGoAstVisitor<T>
{
    T VisitSourceFile(SourceFile node);
    T VisitPackageDeclaration(PackageDeclaration node);
    T VisitImportDeclaration(ImportDeclaration node);
    T VisitFunctionDeclaration(FunctionDeclaration node);
    T VisitMethodDeclaration(MethodDeclaration node);
    T VisitTypeDeclaration(TypeDeclaration node);
    // ... methods for all node types
}
```

### Base Implementations

```csharp
// Base visitor that does nothing (for selective overriding)
public abstract class GoAstVisitorBase : IGoAstVisitor

// Walker that visits all child nodes
public abstract class GoAstWalker : GoAstVisitorBase

// Generic visitor base
public abstract class GoAstVisitor<T> : IGoAstVisitor<T>
```

## Utility Extensions

### Finding Nodes

```csharp
// Find all nodes of a specific type
public static IEnumerable<T> FindNodes<T>(this IGoNode root) where T : IGoNode

// Find the first node of a specific type
public static T? FindFirstNode<T>(this IGoNode root) where T : IGoNode

// Find all function declarations
public static IEnumerable<FunctionDeclaration> GetFunctions(this SourceFile sourceFile)

// Find all method declarations
public static IEnumerable<MethodDeclaration> GetMethods(this SourceFile sourceFile)

// Find all type declarations
public static IEnumerable<TypeDeclaration> GetTypes(this SourceFile sourceFile)

// Find a function by name
public static FunctionDeclaration? FindFunction(this SourceFile sourceFile, string name)
```

### Import Management

```csharp
// Get all imported packages
public static IEnumerable<string> GetImportedPackages(this SourceFile sourceFile)

// Check if a package is imported
public static bool IsPackageImported(this SourceFile sourceFile, string packagePath)

// Get the import alias for a package
public static string? GetImportAlias(this SourceFile sourceFile, string packagePath)
```

### Expression Analysis

```csharp
// Find all identifiers in the AST
public static IEnumerable<IdentifierExpression> GetAllIdentifiers(this IGoNode root)

// Find all function calls in the AST
public static IEnumerable<CallExpression> GetAllCalls(this IGoNode root)

// Find all literal values of a specific kind
public static IEnumerable<LiteralExpression> GetLiterals(this IGoNode root, LiteralKind kind)
```

### AST Metrics

```csharp
// Count all nodes in the AST
public static int CountNodes(this IGoNode root)

// Get the depth of a node in the AST
public static int GetDepth(this IGoNode node, IGoNode root)

// Get the parent of a node
public static IGoNode? GetParent(this IGoNode node, IGoNode root)

// Get all nodes at a specific position
public static IEnumerable<IGoNode> GetNodesAtPosition(this IGoNode root, Position position)
```

## JSON Serialization

### Extension Methods

```csharp
// Serialize to JSON with default options
public static string ToJson(this IGoNode node)

// Serialize to pretty-printed JSON
public static string ToJsonPretty(this IGoNode node)

// Serialize to compact JSON
public static string ToJsonCompact(this IGoNode node)
```

### JSON Structure

The JSON output includes:
- `Type` - The node type name
- `Start` - Start position
- `End` - End position
- Node-specific properties

Example:
```json
{
  "Type": "FunctionDeclaration",
  "Name": "main",
  "Parameters": [],
  "ReturnParameters": null,
  "Body": {
    "Type": "BlockStatement",
    "Statements": []
  },
  "Start": { "Line": 3, "Column": 1, "Offset": 20 },
  "End": { "Line": 5, "Column": 2, "Offset": 40 }
}
```

## Diagnostics

### DiagnosticInfo

```csharp
public class DiagnosticInfo
{
    // Parsing performance metrics
    public double ParseTimeMs { get; }
    public int TokenCount { get; }
    public long FileSizeBytes { get; }
    public int LineCount { get; }
    
    // Errors and warnings
    public IReadOnlyList<ParseError> Errors { get; }
    public IReadOnlyList<ParseWarning> Warnings { get; }
    
    // AST metrics
    public int NodeCount { get; }
    public int MaxDepth { get; }
}
```

### ParseError

```csharp
public class ParseError
{
    public int Line { get; }
    public int Column { get; }
    public string Message { get; }
    public string? Token { get; }
}
```

### ParseWarning

```csharp
public class ParseWarning
{
    public Position Position { get; }
    public string Message { get; }
    public WarningLevel Level { get; }
}

public enum WarningLevel
{
    Error,
    Warning,
    Info,
    Suggestion
}
```

## Performance

### ParserCache

```csharp
public class ParserCache
{
    // Get the default shared cache instance
    public static ParserCache Default { get; }
    
    // Create cache with custom settings
    public ParserCache(int maxCacheSize = 100, TimeSpan? cacheExpiration = null)
    
    // Cache operations
    public bool TryGetCached(string source, out SourceFile? result)
    public void AddToCache(string source, SourceFile sourceFile)
    public void Clear()
    
    // Get cache statistics
    public CacheStatistics GetStatistics()
}
```

### CacheStatistics

```csharp
public class CacheStatistics
{
    public int EntryCount { get; }
    public long TotalHits { get; }
    public long EstimatedMemoryBytes { get; }
    public int MaxCacheSize { get; }
    public TimeSpan CacheExpiration { get; }
    public double HitRate { get; }
    public double FillRate { get; }
}
```

## Error Handling

### ParseException

```csharp
public class ParseException : Exception
{
    public IReadOnlyList<ParseError> Errors { get; }
}
```

### ErrorRecoveryMode

```csharp
public enum ErrorRecoveryMode
{
    Default,          // Default ANTLR error recovery
    Bail,             // Bail out on first error
    DefaultWithSync   // Default with synchronization
}
```

## Position Information

```csharp
public readonly struct Position
{
    public int Line { get; }      // 1-based line number
    public int Column { get; }    // 1-based column number
    public int Offset { get; }    // 0-based character offset
}
```