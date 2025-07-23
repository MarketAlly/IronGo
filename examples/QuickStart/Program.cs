using System;
using System.Collections.Generic;
using System.Linq;
using MarketAlly.IronGo;
using MarketAlly.IronGo.AST;
using MarketAlly.IronGo.Utilities;

namespace QuickStart;

class Program
{
    static void Main(string[] args)
    {
        // Sample Go source code
        var goSource = @"package main

import ""fmt""

type Person struct {
    Name string
    Age  int
}

func (p Person) Greet() {
    fmt.Printf(""Hello, my name is %s\n"", p.Name)
}

func main() {
    p := Person{Name: ""Alice"", Age: 30}
    p.Greet()
    
    for i := 0; i < 5; i++ {
        fmt.Println(i)
    }
}";

        Console.WriteLine("IronGo Quick Start Example");
        Console.WriteLine("=========================\n");

        try
        {
            // Parse the Go source code
            var result = IronGoParser.ParseWithDiagnostics(goSource);
            var ast = result.SourceFile;
            
            // Display basic information
            Console.WriteLine($"Package: {ast.Package.Name}");
            Console.WriteLine($"Parse time: {result.Diagnostics.ParseTimeMs:F2}ms");
            Console.WriteLine($"Token count: {result.Diagnostics.TokenCount}");
            Console.WriteLine();

            // Display imports
            Console.WriteLine("Imports:");
            foreach (var import in ast.Imports)
            {
                Console.WriteLine($"  - {import.Specs[0].Path}");
            }
            Console.WriteLine();

            // Find and display types
            Console.WriteLine("Types:");
            var typeVisitor = new TypeCollector();
            ast.Accept(typeVisitor);
            foreach (var type in typeVisitor.Types)
            {
                Console.WriteLine($"  - {type.Name} ({type.Kind})");
            }
            Console.WriteLine();

            // Find and display functions
            Console.WriteLine("Functions:");
            var funcVisitor = new FunctionCollector();
            ast.Accept(funcVisitor);
            foreach (var func in funcVisitor.Functions)
            {
                var receiver = func.Receiver != null ? $"({func.Receiver}) " : "";
                Console.WriteLine($"  - {receiver}{func.Name}({func.ParameterCount} params)");
            }
            Console.WriteLine();

            // Count various elements
            var counter = new ElementCounter();
            ast.Accept(counter);
            
            // Count total nodes using a separate visitor
            var nodeCounter = new NodeCounter();
            ast.Accept(nodeCounter);
            counter.TotalNodes = nodeCounter.Count;
            Console.WriteLine("Code Statistics:");
            Console.WriteLine($"  - Function calls: {counter.CallCount}");
            Console.WriteLine($"  - Loops: {counter.LoopCount}");
            Console.WriteLine($"  - String literals: {counter.StringLiteralCount}");
            Console.WriteLine($"  - Total nodes: {counter.TotalNodes}");
            Console.WriteLine();

            // Export to JSON (compact)
            var json = System.Text.Json.JsonSerializer.Serialize(ast, new System.Text.Json.JsonSerializerOptions
            {
                Converters = { new MarketAlly.IronGo.Serialization.AstJsonConverter() }
            });
            Console.WriteLine($"JSON representation size: {json.Length:N0} characters");
            
            // Display any warnings
            if (result.Diagnostics.Warnings.Count > 0)
            {
                Console.WriteLine("\nWarnings:");
                foreach (var warning in result.Diagnostics.Warnings)
                {
                    Console.WriteLine($"  - {warning.Message}");
                }
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
    }
}

// Custom visitor to collect type information
class TypeCollector : GoAstWalker
{
    public List<(string Name, string Kind)> Types { get; } = new();

    public override void VisitTypeDeclaration(TypeDeclaration node)
    {
        foreach (var spec in node.Specs)
        {
            var kind = spec.Type switch
            {
                StructType => "struct",
                InterfaceType => "interface",
                _ => "alias"
            };
            Types.Add((spec.Name, kind));
        }
        base.VisitTypeDeclaration(node);
    }
}

// Custom visitor to collect function information
class FunctionCollector : GoAstWalker
{
    public List<(string Name, string? Receiver, int ParameterCount)> Functions { get; } = new();

    public override void VisitFunctionDeclaration(FunctionDeclaration node)
    {
        Functions.Add((node.Name, null, node.Parameters.Count));
        base.VisitFunctionDeclaration(node);
    }

    public override void VisitMethodDeclaration(MethodDeclaration node)
    {
        var receiverType = "unknown";
        if (node.Receiver.Type is IdentifierType it)
        {
            receiverType = it.Name;
        }
        else if (node.Receiver.Type is PointerType pt && pt.ElementType is IdentifierType pit)
        {
            receiverType = $"*{pit.Name}";
        }
        Functions.Add((node.Name, receiverType, node.Parameters.Count));
        base.VisitMethodDeclaration(node);
    }
}

// Custom visitor to count various elements  
class ElementCounter : GoAstWalker
{
    public int CallCount { get; private set; }
    public int LoopCount { get; private set; }
    public int StringLiteralCount { get; private set; }
    public int TotalNodes { get; set; }

    public override void VisitCallExpression(CallExpression node)
    {
        CallCount++;
        base.VisitCallExpression(node);
    }

    public override void VisitForStatement(ForStatement node)
    {
        LoopCount++;
        base.VisitForStatement(node);
    }

    public override void VisitForRangeStatement(ForRangeStatement node)
    {
        LoopCount++;
        base.VisitForRangeStatement(node);
    }

    public override void VisitLiteralExpression(LiteralExpression node)
    {
        if (node.Kind == LiteralKind.String)
            StringLiteralCount++;
        base.VisitLiteralExpression(node);
    }
}

// Simple node counter
class NodeCounter : GoAstWalker
{
    public int Count { get; private set; }
    
    // Override one base method that all nodes go through
    public override void VisitSourceFile(SourceFile node)
    {
        Count++;
        base.VisitSourceFile(node);
    }
    
    // Count every declaration
    public override void VisitFunctionDeclaration(FunctionDeclaration node)
    {
        Count++;
        base.VisitFunctionDeclaration(node);
    }
    
    public override void VisitMethodDeclaration(MethodDeclaration node)
    {
        Count++;
        base.VisitMethodDeclaration(node);
    }
    
    public override void VisitTypeDeclaration(TypeDeclaration node)
    {
        Count++;
        base.VisitTypeDeclaration(node);
    }
    
    // Count statements
    public override void VisitBlockStatement(BlockStatement node)
    {
        Count++;
        base.VisitBlockStatement(node);
    }
    
    public override void VisitExpressionStatement(ExpressionStatement node)
    {
        Count++;
        base.VisitExpressionStatement(node);
    }
    
    // Count expressions
    public override void VisitCallExpression(CallExpression node)
    {
        Count++;
        base.VisitCallExpression(node);
    }
}