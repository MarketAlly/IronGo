using IronGo;
using IronGo.AST;
using IronGo.Serialization;
using IronGo.Utilities;
using System.Text.Json;

const string goCode = @"
package main

import ""fmt""

func main() {
    fmt.Println(""Hello, World!"")
}

func Add(a, b int) int {
    return a + b
}
";

try
{
    Console.WriteLine("=== IronGo Parser Phase 3 Demo ===\n");
    
    // 1. Basic parsing with diagnostics
    Console.WriteLine("1. Parsing with diagnostics:");
    var parseResult = IronGoParser.ParseWithDiagnostics(goCode);
    var sourceFile = parseResult.SourceFile;
    var diagnostics = parseResult.Diagnostics;
    
    Console.WriteLine($"   Parse time: {diagnostics.ParseTimeMs:F2}ms");
    Console.WriteLine($"   Token count: {diagnostics.TokenCount}");
    Console.WriteLine($"   Line count: {diagnostics.LineCount}");
    Console.WriteLine($"   File size: {diagnostics.FileSizeBytes} bytes");
    Console.WriteLine($"   Errors: {diagnostics.Errors.Count}");
    Console.WriteLine($"   Warnings: {diagnostics.Warnings.Count}");
    
    // 2. JSON serialization
    Console.WriteLine("\n2. JSON Serialization:");
    var json = sourceFile.ToJsonCompact();
    Console.WriteLine($"   Compact JSON size: {json.Length} chars");
    
    // Pretty print a portion
    var packageJson = sourceFile.Package.ToJsonPretty();
    Console.WriteLine("   Package as JSON:");
    Console.WriteLine(packageJson.Split('\n').Select(l => "     " + l).Aggregate((a, b) => a + "\n" + b));
    
    // 3. AST Utilities
    Console.WriteLine("\n3. AST Utilities:");
    var functions = sourceFile.GetFunctions().ToList();
    Console.WriteLine($"   Functions found: {functions.Count}");
    foreach (var func in functions)
    {
        Console.WriteLine($"     - {func.Name} ({func.Parameters.Count} params, {func.ReturnParameters?.Count ?? 0} returns)");
    }
    
    var mainFunc = sourceFile.FindFunction("main");
    Console.WriteLine($"   Found main function: {mainFunc != null}");
    
    var allCalls = sourceFile.GetAllCalls().ToList();
    Console.WriteLine($"   Total function calls: {allCalls.Count}");
    
    var stringLiterals = sourceFile.GetLiterals(LiteralKind.String).ToList();
    Console.WriteLine($"   String literals: {stringLiterals.Count}");
    foreach (var lit in stringLiterals)
    {
        Console.WriteLine($"     - {lit.Value}");
    }
    
    var nodeCount = sourceFile.CountNodes();
    Console.WriteLine($"   Total AST nodes: {nodeCount}");
    
    // 4. Caching demonstration
    Console.WriteLine("\n4. Parser Caching:");
    var parser = new IronGoParser(); // Uses default caching
    
    // First parse (cache miss)
    var start = DateTime.Now;
    _ = parser.ParseSource(goCode);
    var firstTime = (DateTime.Now - start).TotalMilliseconds;
    
    // Second parse (cache hit)
    start = DateTime.Now;
    _ = parser.ParseSource(goCode);
    var secondTime = (DateTime.Now - start).TotalMilliseconds;
    
    Console.WriteLine($"   First parse: {firstTime:F2}ms (cache miss)");
    Console.WriteLine($"   Second parse: {secondTime:F2}ms (cache hit)");
    Console.WriteLine($"   Speedup: {firstTime/secondTime:F1}x");
    
    var cacheStats = IronGo.Performance.ParserCache.Default.GetStatistics();
    Console.WriteLine($"   Cache entries: {cacheStats.EntryCount}");
    Console.WriteLine($"   Cache hit rate: {cacheStats.HitRate:F2}");
    
    // 5. Error handling
    Console.WriteLine("\n5. Error Handling:");
    const string invalidGo = @"
package main

func main() {
    fmt.Println(""Missing import!"")
    x := 10 +
}";
    
    if (IronGoParser.TryParse(invalidGo, out var result, out var error))
    {
        Console.WriteLine("   Parse succeeded");
    }
    else
    {
        Console.WriteLine($"   Parse failed: {error}");
    }
    
    // 6. Custom parser options
    Console.WriteLine("\n6. Custom Parser Options:");
    var options = new ParserOptions
    {
        EnableCaching = false,
        RunAnalyzer = true,
        ContinueOnError = true
    };
    
    var customParser = new IronGoParser(options);
    var emptyCode = @"
package main

func empty() {
}

func tooManyParams(a, b, c, d, e, f, g int) {
    return
}";
    
    var resultWithWarnings = customParser.ParseSourceWithDiagnostics(emptyCode);
    Console.WriteLine($"   Warnings found: {resultWithWarnings.Diagnostics.Warnings.Count}");
    foreach (var warning in resultWithWarnings.Diagnostics.Warnings)
    {
        Console.WriteLine($"     - {warning}");
    }
    
}
catch (ParseException ex)
{
    Console.WriteLine($"Parse error: {ex.Message}");
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"  {error}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}