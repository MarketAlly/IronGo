using FluentAssertions;
using IronGo;
using IronGo.AST;
using IronGo.Diagnostics;
using Xunit;

namespace IronGo.Tests.DiagnosticsTests;

public class DiagnosticsTests
{
    [Fact]
    public void ParseWithDiagnostics_ShouldCollectMetrics()
    {
        // Arrange
        const string source = @"
package main

import ""fmt""

func main() {
    fmt.Println(""Hello, World!"")
}";
        
        // Act
        var result = IronGoParser.ParseWithDiagnostics(source);
        
        // Assert
        result.Should().NotBeNull();
        result.SourceFile.Should().NotBeNull();
        result.Diagnostics.Should().NotBeNull();
        
        result.Diagnostics.ParseTimeMs.Should().BeGreaterThan(0);
        result.Diagnostics.TokenCount.Should().BeGreaterThan(0);
        result.Diagnostics.LineCount.Should().Be(8); // 8 lines including the empty first line from @" literal
        result.Diagnostics.FileSizeBytes.Should().BeGreaterThan(0);
        result.Diagnostics.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public void ParseWithDiagnostics_ShouldCollectErrors()
    {
        // Arrange
        const string source = @"
package main

func broken( {
    // Missing closing paren
}";
        
        // Act & Assert
        var act = () => IronGoParser.ParseWithDiagnostics(source);
        act.Should().Throw<ParseException>()
            .Which.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public void Analyzer_ShouldDetectEmptyFunctions()
    {
        // Arrange
        const string source = @"
package main

func empty() {
}

func notEmpty() {
    println(""test"")
}";
        
        var parser = new IronGoParser(new ParserOptions { RunAnalyzer = true });
        
        // Act
        var result = parser.ParseSourceWithDiagnostics(source);
        
        // Assert
        result.Diagnostics.Warnings.Should().ContainSingle(w => 
            w.Message.Contains("empty body") && 
            w.Message.Contains("empty"));
    }
    
    [Fact]
    public void Analyzer_ShouldDetectTooManyParameters()
    {
        // Arrange
        const string source = @"
package main

func tooMany(a, b, c, d, e, f, g, h int) {
    // 8 parameters
}

func reasonable(a, b, c int) {
    // 3 parameters
}";
        
        var parser = new IronGoParser(new ParserOptions { RunAnalyzer = true });
        
        // Act
        var result = parser.ParseSourceWithDiagnostics(source);
        
        // Assert
        result.Diagnostics.Warnings.Should().ContainSingle(w => 
            w.Message.Contains("8 parameters") && 
            w.Message.Contains("consider using a struct"));
    }
    
    [Fact]
    public void Analyzer_ShouldDetectDuplicateImports()
    {
        // Arrange
        const string source = @"
package main

import ""fmt""
import ""fmt""
import ""strings""";
        
        var parser = new IronGoParser(new ParserOptions { RunAnalyzer = true });
        
        // Act
        var result = parser.ParseSourceWithDiagnostics(source);
        
        // Assert
        result.Diagnostics.Warnings.Should().ContainSingle(w => 
            w.Message.Contains("Duplicate import") && 
            w.Message.Contains("fmt"));
    }
    
    [Fact]
    public void Analyzer_ShouldDetectUnusedLookingVariables()
    {
        // Arrange
        const string source = @"
package main

func test() {
    _unused := 10
    used := 20
    _ = used
}";
        
        var parser = new IronGoParser(new ParserOptions { RunAnalyzer = true });
        
        // Act
        var result = parser.ParseSourceWithDiagnostics(source);
        
        // Assert
        result.Diagnostics.Warnings.Should().ContainSingle(w => 
            w.Message.Contains("_unused") && 
            w.Message.Contains("underscore"));
    }
    
    [Fact]
    public void Analyzer_ShouldDetectInfiniteLoops()
    {
        // Arrange
        const string source = @"
package main

func test() {
    for {
        // No break statement
        println(""infinite"")
    }
    
    for {
        if someCondition() {
            break
        }
    }
}";
        
        var parser = new IronGoParser(new ParserOptions { RunAnalyzer = true });
        
        // Act
        var result = parser.ParseSourceWithDiagnostics(source);
        
        // Assert
        result.Diagnostics.Warnings.Should().ContainSingle(w => 
            w.Message.Contains("Infinite loop") && 
            w.Message.Contains("without break"));
    }
    
    [Fact]
    public void Analyzer_ShouldDetectEmptyIfClauses()
    {
        // Arrange
        const string source = @"
package main

func test(x int) {
    if x > 0 {
        // Empty then clause
    } else {
        println(""negative"")
    }
}";
        
        var parser = new IronGoParser(new ParserOptions { RunAnalyzer = true });
        
        // Act
        var result = parser.ParseSourceWithDiagnostics(source);
        
        // Assert
        result.Diagnostics.Warnings.Should().ContainSingle(w => 
            w.Message.Contains("If statement") && 
            w.Message.Contains("empty then clause"));
    }
    
    [Fact]
    public void WarningLevels_ShouldCategorizeCorrectly()
    {
        // Arrange
        var warning1 = new ParseWarning(
            new Position(1, 1, 0),
            "This is a warning",
            WarningLevel.Warning);
            
        var warning2 = new ParseWarning(
            new Position(2, 1, 10),
            "This is a regular warning",
            WarningLevel.Warning);
            
        var warning3 = new ParseWarning(
            new Position(3, 1, 20),
            "This is an info message",
            WarningLevel.Info);
            
        var warning4 = new ParseWarning(
            new Position(4, 1, 30),
            "This is a suggestion",
            WarningLevel.Suggestion);
        
        // Assert
        warning1.Level.Should().Be(WarningLevel.Warning);
        warning2.Level.Should().Be(WarningLevel.Warning);
        warning3.Level.Should().Be(WarningLevel.Info);
        warning4.Level.Should().Be(WarningLevel.Suggestion);
    }
    
    [Fact]
    public void ContinueOnError_ShouldNotThrowOnErrors()
    {
        // Arrange
        const string source = @"
package main

func test() {
    x := 10 +
    // Missing operand
}";
        
        var parser = new IronGoParser(new ParserOptions 
        { 
            ContinueOnError = true,
            RunAnalyzer = false
        });
        
        // Act
        var result = parser.ParseSourceWithDiagnostics(source);
        
        // Assert
        // With ContinueOnError, it should not throw but should collect errors
        result.Should().NotBeNull();
        result.Diagnostics.Errors.Should().NotBeEmpty();
        result.SourceFile.Should().NotBeNull(); // Should still produce a partial AST
    }
}