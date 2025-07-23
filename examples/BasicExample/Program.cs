using IronGo;
using IronGo.AST;
using IronGo.Serialization;
using IronGo.Utilities;

// Example Go source code
const string goSource = @"
package main

import (
    ""fmt""
    ""strings""
)

// Person represents a person with a name and age
type Person struct {
    Name string `json:""name""`
    Age  int    `json:""age""`
}

// NewPerson creates a new person
func NewPerson(name string, age int) *Person {
    return &Person{
        Name: name,
        Age:  age,
    }
}

// Greet returns a greeting message
func (p *Person) Greet() string {
    return fmt.Sprintf(""Hello, I'm %s and I'm %d years old"", p.Name, p.Age)
}

func main() {
    p := NewPerson(""Alice"", 30)
    greeting := p.Greet()
    fmt.Println(greeting)
    
    // Process some data
    data := []string{""apple"", ""banana"", ""cherry""}
    for i, item := range data {
        processed := strings.ToUpper(item)
        fmt.Printf(""%d: %s\n"", i, processed)
    }
}
";

try
{
    Console.WriteLine("=== IronGo Basic Example ===\n");
    
    // 1. Parse the Go source code
    Console.WriteLine("1. Parsing Go source code...");
    var result = IronGoParser.ParseWithDiagnostics(goSource);
    var ast = result.SourceFile;
    
    Console.WriteLine($"   ✓ Parsed in {result.Diagnostics.ParseTimeMs:F2}ms");
    Console.WriteLine($"   ✓ {result.Diagnostics.TokenCount} tokens");
    Console.WriteLine($"   ✓ {result.Diagnostics.LineCount} lines");
    
    // 2. Basic AST Information
    Console.WriteLine("\n2. AST Information:");
    Console.WriteLine($"   Package: {ast.Package?.Name}");
    Console.WriteLine($"   Imports: {string.Join(", ", ast.GetImportedPackages())}");
    Console.WriteLine($"   Functions: {ast.GetFunctions().Count()}");
    Console.WriteLine($"   Methods: {ast.GetMethods().Count()}");
    Console.WriteLine($"   Types: {ast.GetTypes().Count()}");
    
    // 3. List all declarations
    Console.WriteLine("\n3. Declarations:");
    
    // Types
    foreach (var type in ast.GetTypes())
    {
        Console.WriteLine($"   Type: {type.Name}");
        if (type.Specs[0].Type is StructType structType)
        {
            foreach (var field in structType.Fields)
            {
                Console.WriteLine($"     - {string.Join(", ", field.Names)}: {GetTypeName(field.Type)}");
            }
        }
    }
    
    // Functions
    foreach (var func in ast.GetFunctions())
    {
        var parameters = string.Join(", ", func.Parameters.SelectMany(p => 
            p.Names.Select(n => $"{n} {GetTypeName(p.Type)}")));
        var returns = func.ReturnParameters?.Count > 0 
            ? $" {GetTypeName(func.ReturnParameters[0].Type!)}" 
            : "";
        Console.WriteLine($"   Function: {func.Name}({parameters}){returns}");
    }
    
    // Methods
    foreach (var method in ast.GetMethods())
    {
        var receiverType = GetTypeName(method.Receiver.Type);
        Console.WriteLine($"   Method: ({method.Receiver.Names[0]} {receiverType}) {method.Name}()");
    }
    
    // 4. Find specific constructs
    Console.WriteLine("\n4. Code Analysis:");
    
    // Function calls
    var calls = ast.GetAllCalls().ToList();
    Console.WriteLine($"   Function calls: {calls.Count}");
    
    var uniqueCalls = calls
        .Select(c => GetCallName(c))
        .Where(n => n != null)
        .Distinct()
        .OrderBy(n => n);
    Console.WriteLine($"   Called functions: {string.Join(", ", uniqueCalls)}");
    
    // String literals
    var strings = ast.GetLiterals(LiteralKind.String).ToList();
    Console.WriteLine($"   String literals: {strings.Count}");
    foreach (var str in strings.Take(3))
    {
        Console.WriteLine($"     - \"{str.Value}\"");
    }
    
    // 5. Export to JSON
    Console.WriteLine("\n5. JSON Export:");
    var json = ast.Package!.ToJsonCompact();
    Console.WriteLine($"   Package JSON: {json.Substring(0, Math.Min(100, json.Length))}...");
    
    // 6. Custom visitor example
    Console.WriteLine("\n6. Custom Analysis - Complexity:");
    var complexityAnalyzer = new CyclomaticComplexityVisitor();
    ast.Accept(complexityAnalyzer);
    foreach (var (name, complexity) in complexityAnalyzer.Complexity)
    {
        Console.WriteLine($"   {name}: {complexity}");
    }
    
    // 7. Find potential issues
    if (result.Diagnostics.Warnings.Count > 0)
    {
        Console.WriteLine("\n7. Warnings:");
        foreach (var warning in result.Diagnostics.Warnings)
        {
            Console.WriteLine($"   Line {warning.Line}: {warning.Message}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

// Helper functions
string GetTypeName(IType? type)
{
    return type switch
    {
        IdentifierType ident => ident.Name,
        PointerType ptr => $"*{GetTypeName(ptr.ElementType)}",
        SliceType slice => $"[]{GetTypeName(slice.ElementType)}",
        ArrayType array => $"[{array.Length}]{GetTypeName(array.ElementType)}",
        _ => "unknown"
    };
}

string? GetCallName(CallExpression call)
{
    return call.Function switch
    {
        IdentifierExpression ident => ident.Name,
        SelectorExpression selector => $"{GetExpressionName(selector.X)}.{selector.Selector}",
        _ => null
    };
}

string? GetExpressionName(IExpression expr)
{
    return expr switch
    {
        IdentifierExpression ident => ident.Name,
        _ => null
    };
}

// Custom visitor for cyclomatic complexity
class CyclomaticComplexityVisitor : GoAstWalker
{
    public Dictionary<string, int> Complexity { get; } = new();
    private string? _currentFunction;
    private int _currentComplexity;
    
    public override void VisitFunctionDeclaration(FunctionDeclaration node)
    {
        _currentFunction = node.Name;
        _currentComplexity = 1;
        base.VisitFunctionDeclaration(node);
        Complexity[node.Name] = _currentComplexity;
        _currentFunction = null;
    }
    
    public override void VisitMethodDeclaration(MethodDeclaration node)
    {
        _currentFunction = $"{node.Receiver.Type}.{node.Name}";
        _currentComplexity = 1;
        base.VisitMethodDeclaration(node);
        Complexity[_currentFunction] = _currentComplexity;
        _currentFunction = null;
    }
    
    public override void VisitIfStatement(IfStatement node)
    {
        if (_currentFunction != null) _currentComplexity++;
        base.VisitIfStatement(node);
    }
    
    public override void VisitForStatement(ForStatement node)
    {
        if (_currentFunction != null) _currentComplexity++;
        base.VisitForStatement(node);
    }
    
    public override void VisitForRangeStatement(ForRangeStatement node)
    {
        if (_currentFunction != null) _currentComplexity++;
        base.VisitForRangeStatement(node);
    }
    
    public override void VisitSwitchStatement(SwitchStatement node)
    {
        if (_currentFunction != null) _currentComplexity += Math.Max(1, node.Cases.Count - 1);
        base.VisitSwitchStatement(node);
    }
}