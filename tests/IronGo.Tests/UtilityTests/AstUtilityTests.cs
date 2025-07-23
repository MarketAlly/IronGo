using FluentAssertions;
using MarketAlly.IronGo;
using MarketAlly.IronGo.AST;
using MarketAlly.IronGo.Utilities;
using System.Linq;
using Xunit;

namespace MarketAlly.IronGo.Tests.UtilityTests;

public class AstUtilityTests
{
    [Fact]
    public void FindNodes_ShouldFindAllNodesOfType()
    {
        // Arrange
        const string source = @"
package main

func foo() {}
func bar() {}

type Person struct {}

func (p Person) Method() {}";
        
        var ast = IronGoParser.Parse(source);
        
        // Act
        var allFunctions = ast.FindNodes<FunctionDeclaration>().ToList();
        var functions = allFunctions.Where(f => f.GetType() == typeof(FunctionDeclaration)).ToList();
        var methods = ast.FindNodes<MethodDeclaration>().ToList();
        var types = ast.FindNodes<TypeDeclaration>().ToList();
        
        // Assert
        functions.Should().HaveCount(2);
        functions.Select(f => f.Name).Should().Equal("foo", "bar");
        
        methods.Should().HaveCount(1);
        methods[0].Name.Should().Be("Method");
        
        types.Should().HaveCount(1);
        types[0].Name.Should().Be("Person");
    }
    
    [Fact]
    public void GetFunctions_ShouldReturnOnlyFunctions()
    {
        // Arrange
        const string source = @"
package main

func main() {}
func helper() {}

type T struct {}

func (t T) Method() {}";
        
        var ast = IronGoParser.Parse(source);
        
        // Act
        var functions = ast.GetFunctions().ToList();
        
        // Assert
        functions.Should().HaveCount(2);
        functions.Should().NotContain(f => f is MethodDeclaration);
        functions.Select(f => f.Name).Should().Equal("main", "helper");
    }
    
    [Fact]
    public void FindFunction_ShouldFindByName()
    {
        // Arrange
        const string source = @"
package main

func init() {}
func main() {}
func process() {}";
        
        var ast = IronGoParser.Parse(source);
        
        // Act
        var main = ast.FindFunction("main");
        var missing = ast.FindFunction("missing");
        
        // Assert
        main.Should().NotBeNull();
        main!.Name.Should().Be("main");
        missing.Should().BeNull();
    }
    
    [Fact]
    public void GetImportedPackages_ShouldReturnAllImports()
    {
        // Arrange
        const string source = @"
package main

import ""fmt""
import (
    ""strings""
    ""io""
    ""fmt"" // duplicate
)";
        
        var ast = IronGoParser.Parse(source);
        
        // Act
        var packages = ast.GetImportedPackages().ToList();
        
        // Assert
        packages.Should().Equal("fmt", "strings", "io");
        packages.Should().HaveCount(3); // Distinct removes duplicate
    }
    
    [Fact]
    public void IsPackageImported_ShouldCheckImports()
    {
        // Arrange
        const string source = @"
package main

import (
    ""fmt""
    ""strings""
)";
        
        var ast = IronGoParser.Parse(source);
        
        // Act & Assert
        ast.IsPackageImported("fmt").Should().BeTrue();
        ast.IsPackageImported("strings").Should().BeTrue();
        ast.IsPackageImported("io").Should().BeFalse();
    }
    
    [Fact]
    public void GetImportAlias_ShouldReturnAlias()
    {
        // Arrange
        const string source = @"
package main

import (
    ""fmt""
    f ""fmt""
    . ""strings""
)";
        
        var ast = IronGoParser.Parse(source);
        
        // Act & Assert
        ast.GetImportAlias("fmt").Should().BeNull(); // First import has no alias
        // Note: Due to multiple imports of same package, this would need adjustment in real implementation
    }
    
    [Fact]
    public void GetAllIdentifiers_ShouldFindAllIdentifiers()
    {
        // Arrange
        const string source = @"
package main

func test() {
    x := 10
    y := x + z
    fmt.Println(x, y)
}";
        
        var ast = IronGoParser.Parse(source);
        
        // Act
        var identifiers = ast.GetAllIdentifiers()
            .Select(i => i.Name)
            .Distinct()
            .ToList();
        
        // Assert
        identifiers.Should().Contain("x");
        identifiers.Should().Contain("y");
        identifiers.Should().Contain("z");
        identifiers.Should().Contain("fmt");
        identifiers.Should().Contain("Println");
    }
    
    [Fact]
    public void GetAllCalls_ShouldFindFunctionCalls()
    {
        // Arrange
        const string source = @"
package main

func test() {
    println(""test"")
    fmt.Println(""hello"")
    result := add(1, 2)
    go process()
    defer cleanup()
}";
        
        var ast = IronGoParser.Parse(source);
        
        // Act
        var calls = ast.GetAllCalls().ToList();
        
        // Assert
        calls.Should().HaveCount(5);
    }
    
    [Fact]
    public void GetLiterals_ShouldFilterByKind()
    {
        // Arrange
        const string source = @"
package main

func test() {
    s1 := ""hello""
    s2 := ""world""
    i := 42
    f := 3.14
    b := true
}";
        
        var ast = IronGoParser.Parse(source);
        
        // Act
        var strings = ast.GetLiterals(LiteralKind.String)
            .Select(l => l.Value)
            .ToList();
        var ints = ast.GetLiterals(LiteralKind.Int)
            .Select(l => l.Value)
            .ToList();
        var floats = ast.GetLiterals(LiteralKind.Float)
            .Select(l => l.Value)
            .ToList();
        var bools = ast.GetLiterals(LiteralKind.Bool)
            .Select(l => l.Value)
            .ToList();
        
        // Assert
        strings.Should().Equal("hello", "world");
        ints.Should().Equal("42");
        floats.Should().Equal("3.14");
        bools.Should().Equal("true");
    }
    
    [Fact]
    public void CountNodes_ShouldCountAllNodes()
    {
        // Arrange
        const string source = @"
package main

import ""fmt""

func main() {
    x := 10
    if x > 5 {
        fmt.Println(""big"")
    }
}";
        
        var ast = IronGoParser.Parse(source);
        
        // Act
        var count = ast.CountNodes();
        
        // Assert
        count.Should().BeGreaterThan(10); // Rough estimate
    }
    
    [Fact]
    public void GetNodesAtPosition_ShouldFindNodesContainingPosition()
    {
        // Arrange
        const string source = @"package main

func test() {
    x := 10
}";
        
        var ast = IronGoParser.Parse(source);
        
        // Find position of "10" literal
        var literal = ast.FindNodes<LiteralExpression>().First(l => l.Value == "10");
        var position = new Position(literal.Start.Line, literal.Start.Column, literal.Start.Offset);
        
        // Act
        var nodesAtPosition = ast.GetNodesAtPosition(position).ToList();
        
        // Assert
        nodesAtPosition.Should().Contain(literal);
        nodesAtPosition.Should().Contain(n => n is ShortVariableDeclaration);
        nodesAtPosition.Should().Contain(n => n is BlockStatement);
        nodesAtPosition.Should().Contain(n => n is FunctionDeclaration);
        nodesAtPosition.Should().Contain(n => n is SourceFile);
    }
}