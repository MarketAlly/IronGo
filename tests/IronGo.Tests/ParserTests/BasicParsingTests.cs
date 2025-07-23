using FluentAssertions;
using IronGo;
using IronGo.AST;
using Xunit;

namespace IronGo.Tests.ParserTests;

public class BasicParsingTests
{
    [Fact]
    public void Parse_EmptySource_ShouldReturnEmptySourceFile()
    {
        // Arrange
        const string source = "";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        result.Should().NotBeNull();
        result.Package.Should().BeNull();
        result.Imports.Should().BeEmpty();
        result.Declarations.Should().BeEmpty();
    }
    
    [Fact]
    public void Parse_PackageOnly_ShouldParsePackageDeclaration()
    {
        // Arrange
        const string source = "package main\n";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        result.Should().NotBeNull();
        result.Package.Should().NotBeNull();
        result.Package!.Name.Should().Be("main");
    }
    
    [Fact]
    public void Parse_SimpleImport_ShouldParseImportDeclaration()
    {
        // Arrange
        const string source = @"
package main

import ""fmt""";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        result.Should().NotBeNull();
        result.Imports.Should().HaveCount(1);
        result.Imports[0].Specs.Should().HaveCount(1);
        result.Imports[0].Specs[0].Path.Should().Be("fmt");
    }
    
    [Fact]
    public void Parse_MultipleImports_ShouldParseAllImports()
    {
        // Arrange
        const string source = @"
package main

import (
    ""fmt""
    ""strings""
    ""io""
)";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        result.Should().NotBeNull();
        result.Imports.Should().HaveCount(1);
        result.Imports[0].Specs.Should().HaveCount(3);
        result.Imports[0].Specs[0].Path.Should().Be("fmt");
        result.Imports[0].Specs[1].Path.Should().Be("strings");
        result.Imports[0].Specs[2].Path.Should().Be("io");
    }
    
    [Fact]
    public void Parse_ImportWithAlias_ShouldParseAliasedImport()
    {
        // Arrange
        const string source = @"
package main

import f ""fmt""";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        result.Should().NotBeNull();
        result.Imports.Should().HaveCount(1);
        result.Imports[0].Specs[0].Alias.Should().Be("f");
        result.Imports[0].Specs[0].Path.Should().Be("fmt");
    }
    
    [Fact]
    public void Parse_SimpleFunctionDeclaration_ShouldParseFunctionWithoutParameters()
    {
        // Arrange
        const string source = @"
package main

func hello() {
    println(""Hello, World!"")
}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        result.Should().NotBeNull();
        result.Declarations.Should().HaveCount(1);
        
        var function = result.Declarations[0].Should().BeOfType<FunctionDeclaration>().Subject;
        function.Name.Should().Be("hello");
        function.Parameters.Should().BeEmpty();
        function.ReturnParameters.Should().BeNull();
        function.Body.Should().NotBeNull();
        function.Body!.Statements.Should().HaveCount(1);
    }
    
    [Fact]
    public void Parse_FunctionWithParameters_ShouldParseParameterList()
    {
        // Arrange
        const string source = @"
package main

func add(a int, b int) int {
    return a + b
}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        result.Should().NotBeNull();
        var function = result.Declarations[0].Should().BeOfType<FunctionDeclaration>().Subject;
        
        function.Parameters.Should().HaveCount(2);
        function.Parameters[0].Names.Should().ContainSingle("a");
        function.Parameters[0].Type.Should().BeOfType<IdentifierType>()
            .Which.Name.Should().Be("int");
        function.Parameters[1].Names.Should().ContainSingle("b");
        function.Parameters[1].Type.Should().BeOfType<IdentifierType>()
            .Which.Name.Should().Be("int");
        
        function.ReturnParameters.Should().NotBeNull();
        function.ReturnParameters!.Should().HaveCount(1);
        function.ReturnParameters![0].Type.Should().BeOfType<IdentifierType>()
            .Which.Name.Should().Be("int");
    }
    
    [Fact]
    public void Parse_InvalidSyntax_ShouldThrowParseException()
    {
        // Arrange
        const string source = @"
package main

func broken {
    // Missing parentheses
}";
        
        // Act & Assert
        var act = () => IronGoParser.Parse(source);
        act.Should().Throw<ParseException>()
            .WithMessage("*syntax error*");
    }
    
    [Fact]
    public void TryParse_ValidSource_ShouldReturnTrue()
    {
        // Arrange
        const string source = "package main";
        
        // Act
        var success = IronGoParser.TryParse(source, out var result, out var error);
        
        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        error.Should().BeNull();
    }
    
    [Fact]
    public void TryParse_InvalidSource_ShouldReturnFalse()
    {
        // Arrange
        const string source = "invalid go code {{{";
        
        // Act
        var success = IronGoParser.TryParse(source, out var result, out var error);
        
        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }
}