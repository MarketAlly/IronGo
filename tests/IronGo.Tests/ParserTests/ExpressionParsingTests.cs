using FluentAssertions;
using MarketAlly.IronGo;
using MarketAlly.IronGo.AST;
using Xunit;

namespace MarketAlly.IronGo.Tests.ParserTests;

public class ExpressionParsingTests
{
    [Theory]
    [InlineData("42", LiteralKind.Int, "42")]
    [InlineData("3.14", LiteralKind.Float, "3.14")]
    [InlineData("\"hello\"", LiteralKind.String, "hello")]
    [InlineData("'a'", LiteralKind.Rune, "a")]
    [InlineData("true", LiteralKind.Bool, "true")]
    [InlineData("false", LiteralKind.Bool, "false")]
    public void Parse_LiteralExpressions_ShouldParseLiterals(string source, LiteralKind expectedKind, string expectedValue)
    {
        // Arrange
        var code = $@"
package main

func test() {{
    x := {source}
}}";
        
        // Act
        var result = IronGoParser.Parse(code);
        
        // Assert
        var function = result.Declarations[0] as FunctionDeclaration;
        var shortDecl = function!.Body!.Statements[0] as ShortVariableDeclaration;
        var literal = shortDecl!.Values[0].Should().BeOfType<LiteralExpression>().Subject;
        
        literal.Kind.Should().Be(expectedKind);
        literal.Value.Should().Be(expectedValue);
    }
    
    [Fact]
    public void Parse_BinaryExpressions_ShouldParseOperatorPrecedence()
    {
        // Arrange
        const string source = @"
package main

func test() {
    a := 1 + 2 * 3
    b := (1 + 2) * 3
    c := x && y || z
    d := a == b && c != d
}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        var function = result.Declarations[0] as FunctionDeclaration;
        
        // a := 1 + 2 * 3  =>  1 + (2 * 3)
        var expr1 = (function!.Body!.Statements[0] as ShortVariableDeclaration)!.Values[0] as BinaryExpression;
        expr1!.Operator.Should().Be(BinaryOperator.Add);
        expr1.Right.Should().BeOfType<BinaryExpression>()
            .Which.Operator.Should().Be(BinaryOperator.Multiply);
        
        // b := (1 + 2) * 3
        var expr2 = (function.Body.Statements[1] as ShortVariableDeclaration)!.Values[0] as BinaryExpression;
        expr2!.Operator.Should().Be(BinaryOperator.Multiply);
        expr2.Left.Should().BeOfType<BinaryExpression>()
            .Which.Operator.Should().Be(BinaryOperator.Add);
    }
    
    [Fact]
    public void Parse_UnaryExpressions_ShouldParseUnaryOperators()
    {
        // Arrange
        const string source = @"
package main

func test() {
    a := -42
    b := !flag
    c := ^bits
    d := &value
    e := *pointer
    f := <-channel
}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        var function = result.Declarations[0] as FunctionDeclaration;
        var statements = function!.Body!.Statements;
        
        var unaryOps = statements
            .OfType<ShortVariableDeclaration>()
            .Select(s => (s.Values[0] as UnaryExpression)!.Operator)
            .ToList();
        
        unaryOps.Should().Equal(UnaryOperator.Minus, UnaryOperator.Not, UnaryOperator.Complement, 
            UnaryOperator.Address, UnaryOperator.Dereference, UnaryOperator.Receive);
    }
    
    [Fact]
    public void Parse_CallExpressions_ShouldParseFunctionCalls()
    {
        // Arrange
        const string source = @"
package main

func test() {
    fmt.Println(""Hello"", name)
    result := add(1, 2)
    go process(data)
    defer cleanup()
}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        var function = result.Declarations[0] as FunctionDeclaration;
        
        // fmt.Println("Hello", name)
        var call1 = (function!.Body!.Statements[0] as ExpressionStatement)!.Expression as CallExpression;
        call1!.Function.Should().BeOfType<SelectorExpression>();
        call1.Arguments.Should().HaveCount(2);
        
        // result := add(1, 2)
        var call2 = (function.Body.Statements[1] as ShortVariableDeclaration)!.Values[0] as CallExpression;
        call2!.Function.Should().BeOfType<IdentifierExpression>()
            .Which.Name.Should().Be("add");
        call2.Arguments.Should().HaveCount(2);
    }
    
    [Fact]
    public void Parse_IndexExpressions_ShouldParseArraySliceAccess()
    {
        // Arrange
        const string source = @"
package main

func test() {
    x := arr[0]
    y := slice[1:3]
    z := slice[:5]
    w := slice[2:]
    v := slice[:]
}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        var function = result.Declarations[0] as FunctionDeclaration;
        var statements = function!.Body!.Statements.OfType<ShortVariableDeclaration>().ToList();
        
        // arr[0]
        var index1 = statements[0].Values[0].Should().BeOfType<IndexExpression>().Subject;
        index1.Index.Should().NotBeNull();
        
        // slice[1:3]
        var slice2 = statements[1].Values[0].Should().BeOfType<SliceExpression>().Subject;
        slice2.Low.Should().NotBeNull();
        slice2.High.Should().NotBeNull();
        
        // slice[:5]
        var slice3 = statements[2].Values[0].Should().BeOfType<SliceExpression>().Subject;
        slice3.Low.Should().BeNull();
        slice3.High.Should().NotBeNull();
        
        // slice[2:]
        var slice4 = statements[3].Values[0].Should().BeOfType<SliceExpression>().Subject;
        slice4.Low.Should().NotBeNull();
        slice4.High.Should().BeNull();
    }
    
    [Fact]
    public void Parse_CompositeLiterals_ShouldParseStructSliceMapLiterals()
    {
        // Arrange
        const string source = @"
package main

func test() {
    // Struct literal
    p := Point{X: 10, Y: 20}
    
    // Slice literal
    nums := []int{1, 2, 3, 4, 5}
    
    // Map literal
    colors := map[string]int{
        ""red"": 0xFF0000,
        ""green"": 0x00FF00,
        ""blue"": 0x0000FF,
    }
    
    // Array literal
    arr := [3]string{""a"", ""b"", ""c""}
}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        var function = result.Declarations[0] as FunctionDeclaration;
        var statements = function!.Body!.Statements.OfType<ShortVariableDeclaration>().ToList();
        
        // Struct literal
        var structLit = statements[0].Values[0].Should().BeOfType<CompositeLiteral>().Subject;
        structLit.Type.Should().BeOfType<IdentifierType>()
            .Which.Name.Should().Be("Point");
        structLit.Elements.Should().HaveCount(2);
        
        // Slice literal
        var sliceLit = statements[1].Values[0].Should().BeOfType<CompositeLiteral>().Subject;
        sliceLit.Type.Should().BeOfType<SliceType>();
        sliceLit.Elements.Should().HaveCount(5);
        
        // Map literal
        var mapLit = statements[2].Values[0].Should().BeOfType<CompositeLiteral>().Subject;
        mapLit.Type.Should().BeOfType<MapType>();
        mapLit.Elements.Should().HaveCount(3);
    }
    
    [Fact]
    public void Parse_TypeAssertions_ShouldParseTypeAssertionExpressions()
    {
        // Arrange
        const string source = @"
package main

func test(i interface{}) {
    s := i.(string)
    n, ok := i.(int)
}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        var function = result.Declarations[0] as FunctionDeclaration;
        
        // s := i.(string)
        var assert1 = (function!.Body!.Statements[0] as ShortVariableDeclaration)!.Values[0];
        assert1.Should().BeOfType<TypeAssertionExpression>()
            .Which.Type.Should().BeOfType<IdentifierType>()
            .Which.Name.Should().Be("string");
        
        // n, ok := i.(int)
        var assert2 = (function.Body.Statements[1] as ShortVariableDeclaration)!.Values[0];
        assert2.Should().BeOfType<TypeAssertionExpression>();
    }
    
    [Fact]
    public void Parse_FunctionLiterals_ShouldParseAnonymousFunctions()
    {
        // Arrange
        const string source = @"
package main

func test() {
    f := func(x int) int {
        return x * 2
    }
    
    go func() {
        println(""async"")
    }()
}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        var function = result.Declarations[0] as FunctionDeclaration;
        
        // f := func(x int) int { ... }
        var funcLit1 = (function!.Body!.Statements[0] as ShortVariableDeclaration)!.Values[0];
        funcLit1.Should().BeOfType<FunctionLiteral>()
            .Which.Parameters.Should().HaveCount(1);
        
        // go func() { ... }()
        var goStmt = function.Body.Statements[1] as GoStatement;
        goStmt!.Call.Should().BeOfType<CallExpression>()
            .Which.Function.Should().BeOfType<FunctionLiteral>();
    }
}