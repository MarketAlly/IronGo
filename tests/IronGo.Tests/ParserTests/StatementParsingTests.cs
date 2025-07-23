using FluentAssertions;
using IronGo;
using IronGo.AST;
using Xunit;

namespace IronGo.Tests.ParserTests;

public class StatementParsingTests
{
    [Fact]
    public void Parse_VariableDeclaration_ShouldParseVarStatement()
    {
        // Arrange
        const string source = @"
package main

func test() {
    var x int = 10
    var y, z string
}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        var function = result.Declarations[0] as FunctionDeclaration;
        function.Should().NotBeNull();
        function!.Body!.Statements.Should().HaveCount(2);
        
        var varDecl1 = function.Body.Statements[0].Should().BeOfType<DeclarationStatement>().Subject;
        varDecl1.Declaration.Should().BeOfType<VariableDeclaration>();
        
        var varDecl2 = function.Body.Statements[1].Should().BeOfType<DeclarationStatement>().Subject;
        varDecl2.Declaration.Should().BeOfType<VariableDeclaration>();
    }
    
    [Fact]
    public void Parse_ShortVariableDeclaration_ShouldParseColonEquals()
    {
        // Arrange
        const string source = @"
package main

func test() {
    x := 10
    y, z := ""hello"", ""world""
}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        var function = result.Declarations[0] as FunctionDeclaration;
        function.Should().NotBeNull();
        function!.Body!.Statements.Should().HaveCount(2);
        
        var shortDecl1 = function.Body.Statements[0].Should().BeOfType<ShortVariableDeclaration>().Subject;
        shortDecl1.Names.Should().ContainSingle("x");
        shortDecl1.Values.Should().HaveCount(1);
        
        var shortDecl2 = function.Body.Statements[1].Should().BeOfType<ShortVariableDeclaration>().Subject;
        shortDecl2.Names.Should().Equal("y", "z");
        shortDecl2.Values.Should().HaveCount(2);
    }
    
    [Fact]
    public void Parse_IfStatement_ShouldParseConditional()
    {
        // Arrange
        const string source = @"
package main

func test(x int) {
    if x > 0 {
        println(""positive"")
    } else if x < 0 {
        println(""negative"")
    } else {
        println(""zero"")
    }
}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        var function = result.Declarations[0] as FunctionDeclaration;
        var ifStmt = function!.Body!.Statements[0].Should().BeOfType<IfStatement>().Subject;
        
        ifStmt.Condition.Should().BeOfType<BinaryExpression>();
        ifStmt.Then.Should().BeOfType<BlockStatement>();
        ifStmt.Else.Should().BeOfType<IfStatement>(); // else if
        
        var elseIf = ifStmt.Else as IfStatement;
        elseIf!.Else.Should().BeOfType<BlockStatement>(); // final else
    }
    
    [Fact]
    public void Parse_ForLoop_ShouldParseAllForVariants()
    {
        // Arrange
        const string source = @"
package main

func test() {
    // Traditional for loop
    for i := 0; i < 10; i++ {
        println(i)
    }
    
    // While-like loop
    for x < 100 {
        x *= 2
    }
    
    // Infinite loop
    for {
        break
    }
}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        var function = result.Declarations[0] as FunctionDeclaration;
        function!.Body!.Statements.Should().HaveCount(3);
        
        var for1 = function.Body.Statements[0].Should().BeOfType<ForStatement>().Subject;
        for1.Init.Should().NotBeNull();
        for1.Condition.Should().NotBeNull();
        for1.Post.Should().NotBeNull();
        
        var for2 = function.Body.Statements[1].Should().BeOfType<ForStatement>().Subject;
        for2.Init.Should().BeNull();
        for2.Condition.Should().NotBeNull();
        for2.Post.Should().BeNull();
        
        var for3 = function.Body.Statements[2].Should().BeOfType<ForStatement>().Subject;
        for3.Init.Should().BeNull();
        for3.Condition.Should().BeNull();
        for3.Post.Should().BeNull();
    }
    
    [Fact]
    public void Parse_RangeLoop_ShouldParseForRange()
    {
        // Arrange
        const string source = @"
package main

func test(items []string) {
    for i, item := range items {
        println(i, item)
    }
    
    for _, value := range items {
        println(value)
    }
    
    for key := range items {
        println(key)
    }
}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        var function = result.Declarations[0] as FunctionDeclaration;
        function!.Body!.Statements.Should().HaveCount(3);
        
        var range1 = function.Body.Statements[0].Should().BeOfType<ForRangeStatement>().Subject;
        range1.Key.Should().Be("i");
        range1.Value.Should().Be("item");
        
        var range2 = function.Body.Statements[1].Should().BeOfType<ForRangeStatement>().Subject;
        range2.Key.Should().Be("_");
        range2.Value.Should().Be("value");
        
        var range3 = function.Body.Statements[2].Should().BeOfType<ForRangeStatement>().Subject;
        range3.Key.Should().Be("key");
        range3.Value.Should().BeNull();
    }
    
    [Fact]
    public void Parse_SwitchStatement_ShouldParseSwitchCases()
    {
        // Arrange
        const string source = @"
package main

func test(x int) {
    switch x {
    case 1:
        println(""one"")
    case 2, 3:
        println(""two or three"")
    default:
        println(""other"")
    }
}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        var function = result.Declarations[0] as FunctionDeclaration;
        var switchStmt = function!.Body!.Statements[0].Should().BeOfType<SwitchStatement>().Subject;
        
        switchStmt.Tag.Should().BeOfType<IdentifierExpression>();
        switchStmt.Cases.Should().HaveCount(3);
        
        switchStmt.Cases[0].Expressions.Should().HaveCount(1);
        switchStmt.Cases[1].Expressions.Should().HaveCount(2);
        switchStmt.Cases[2].IsDefault.Should().BeTrue();
    }
    
    [Fact]
    public void Parse_DeferStatement_ShouldParseDefer()
    {
        // Arrange
        const string source = @"
package main

func test() {
    defer fmt.Println(""cleanup"")
    defer func() {
        recover()
    }()
}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        var function = result.Declarations[0] as FunctionDeclaration;
        function!.Body!.Statements.Should().HaveCount(2);
        
        var defer1 = function.Body.Statements[0].Should().BeOfType<DeferStatement>().Subject;
        defer1.Call.Should().BeOfType<CallExpression>();
        
        var defer2 = function.Body.Statements[1].Should().BeOfType<DeferStatement>().Subject;
        defer2.Call.Should().BeOfType<CallExpression>();
    }
    
    [Fact]
    public void Parse_GoStatement_ShouldParseGoroutine()
    {
        // Arrange
        const string source = @"
package main

func test() {
    go processData()
    go func(x int) {
        println(x)
    }(42)
}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        var function = result.Declarations[0] as FunctionDeclaration;
        function!.Body!.Statements.Should().HaveCount(2);
        
        function.Body.Statements[0].Should().BeOfType<GoStatement>();
        function.Body.Statements[1].Should().BeOfType<GoStatement>();
    }
    
    [Fact]
    public void Parse_ReturnStatement_ShouldParseReturnValues()
    {
        // Arrange
        const string source = @"
package main

func test() (int, error) {
    return 42, nil
}

func empty() {
    return
}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        result.Declarations.Should().HaveCount(2);
        
        var func1 = result.Declarations[0] as FunctionDeclaration;
        var return1 = func1!.Body!.Statements[0].Should().BeOfType<ReturnStatement>().Subject;
        return1.Results.Should().HaveCount(2);
        
        var func2 = result.Declarations[1] as FunctionDeclaration;
        var return2 = func2!.Body!.Statements[0].Should().BeOfType<ReturnStatement>().Subject;
        return2.Results.Should().BeEmpty();
    }
}