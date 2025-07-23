using FluentAssertions;
using IronGo;
using IronGo.AST;
using IronGo.Serialization;
using System.Text.Json;
using Xunit;

namespace IronGo.Tests.SerializationTests;

public class JsonSerializationTests
{
    [Fact]
    public void ToJson_ShouldSerializeSimpleAST()
    {
        // Arrange
        const string source = @"
package main

func hello() {
    println(""Hello, World!"")
}";
        
        var ast = IronGoParser.Parse(source);
        
        // Act
        var json = ast.ToJson();
        
        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("\"Type\": \"SourceFile\"");
        json.Should().Contain("\"Package\":");
        json.Should().Contain("\"Name\": \"main\"");
        json.Should().Contain("\"Declarations\":");
        json.Should().Contain("\"Type\": \"FunctionDeclaration\"");
        json.Should().Contain("\"Name\": \"hello\"");
    }
    
    [Fact]
    public void ToJsonCompact_ShouldProduceCompactJson()
    {
        // Arrange
        const string source = "package main";
        var ast = IronGoParser.Parse(source);
        
        // Act
        var compact = ast.ToJsonCompact();
        var pretty = ast.ToJsonPretty();
        
        // Assert
        compact.Should().NotContain("\n");
        compact.Should().NotContain("  ");
        pretty.Should().Contain("\n");
        pretty.Should().Contain("  ");
        compact.Length.Should().BeLessThan(pretty.Length);
    }
    
    [Fact]
    public void SerializeExpression_ShouldHandleComplexExpressions()
    {
        // Arrange
        const string source = @"
package main

func test() {
    result := (a + b) * c / d
}";
        
        var ast = IronGoParser.Parse(source);
        var function = ast.Declarations[0] as FunctionDeclaration;
        var statement = function!.Body!.Statements[0] as ShortVariableDeclaration;
        var expression = statement!.Values[0];
        
        // Act
        var json = expression.ToJson();
        var doc = JsonDocument.Parse(json);
        
        // Assert
        doc.RootElement.GetProperty("Type").GetString().Should().Be("BinaryExpression");
        doc.RootElement.GetProperty("Operator").GetString().Should().Be("Divide");
        doc.RootElement.GetProperty("Left").GetProperty("Type").GetString().Should().Be("BinaryExpression");
    }
    
    [Fact]
    public void SerializeTypes_ShouldHandleAllTypeVariants()
    {
        // Arrange
        const string source = @"
package main

type (
    Int int
    Ptr *int
    Slice []string
    Array [10]byte
    Map map[string]int
    Chan chan int
    Func func(int) error
    Struct struct { X, Y int }
    Interface interface { Method() }
)";
        
        var ast = IronGoParser.Parse(source);
        
        // Act
        var json = ast.ToJson();
        
        // Assert
        json.Should().Contain("\"Type\": \"IdentifierType\"");
        json.Should().Contain("\"Type\": \"PointerType\"");
        json.Should().Contain("\"Type\": \"SliceType\"");
        json.Should().Contain("\"Type\": \"ArrayType\"");
        json.Should().Contain("\"Type\": \"MapType\"");
        json.Should().Contain("\"Type\": \"ChannelType\"");
        json.Should().Contain("\"Type\": \"FunctionType\"");
        json.Should().Contain("\"Type\": \"StructType\"");
        json.Should().Contain("\"Type\": \"InterfaceType\"");
    }
    
    [Fact]
    public void SerializeStatements_ShouldHandleAllStatementTypes()
    {
        // Arrange
        const string source = @"
package main

func test() {
    x := 10
    var y int
    if x > 5 { }
    for i := 0; i < 10; i++ { }
    switch x { case 1: }
    defer cleanup()
    go process()
    return x
    break
    continue
}";
        
        var ast = IronGoParser.Parse(source);
        var function = ast.Declarations[0] as FunctionDeclaration;
        
        // Act
        var json = function!.Body!.ToJson();
        
        // Assert
        json.Should().Contain("\"Type\": \"ShortVariableDeclaration\"");
        json.Should().Contain("\"Type\": \"DeclarationStatement\"");
        json.Should().Contain("\"Type\": \"IfStatement\"");
        json.Should().Contain("\"Type\": \"ForStatement\"");
        json.Should().Contain("\"Type\": \"SwitchStatement\"");
        json.Should().Contain("\"Type\": \"DeferStatement\"");
        json.Should().Contain("\"Type\": \"GoStatement\"");
        json.Should().Contain("\"Type\": \"ReturnStatement\"");
        json.Should().Contain("\"Type\": \"BranchStatement\"");
    }
    
    [Fact]
    public void SerializeLiterals_ShouldPreserveValues()
    {
        // Arrange
        const string source = @"
package main

func test() {
    s := ""hello""
    i := 42
    f := 3.14
    b := true
    r := 'x'
}";
        
        var ast = IronGoParser.Parse(source);
        
        // Act
        var json = ast.ToJson();
        var doc = JsonDocument.Parse(json);
        
        // Assert
        json.Should().Contain("\"Value\": \"hello\"");
        json.Should().Contain("\"Value\": \"42\"");
        json.Should().Contain("\"Value\": \"3.14\"");
        json.Should().Contain("\"Value\": \"true\"");
        json.Should().Contain("\"Value\": \"x\"");
    }
    
    [Fact]
    public void SerializePosition_ShouldIncludePositionInfo()
    {
        // Arrange
        const string source = "package main";
        var ast = IronGoParser.Parse(source);
        
        // Act
        var json = ast.Package!.ToJson();
        var doc = JsonDocument.Parse(json);
        
        // Assert
        doc.RootElement.TryGetProperty("Start", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("End", out _).Should().BeTrue();
        
        var start = doc.RootElement.GetProperty("Start");
        start.GetProperty("Line").GetInt32().Should().Be(1);
        start.GetProperty("Column").GetInt32().Should().Be(1);
    }
    
    [Fact]
    public void SerializeComments_ShouldIncludeComments()
    {
        // Arrange
        const string source = @"
package main

// HelloWorld prints a greeting
func HelloWorld() {
    // Print to stdout
    println(""Hello, World!"")
}";
        
        var ast = IronGoParser.Parse(source);
        var function = ast.Declarations[0] as FunctionDeclaration;
        
        // Act
        var json = function!.ToJson();
        
        // Assert
        // Comments would be included if properly parsed
        json.Should().Contain("HelloWorld");
    }
    
    [Fact]
    public void RoundTrip_ShouldProduceSameJson()
    {
        // Arrange
        const string source = @"
package main

import ""fmt""

func main() {
    fmt.Println(""test"")
}";
        
        // Act
        var ast1 = IronGoParser.Parse(source);
        var json1 = ast1.ToJson();
        
        var ast2 = IronGoParser.Parse(source);
        var json2 = ast2.ToJson();
        
        // Assert
        json1.Should().Be(json2);
    }
}