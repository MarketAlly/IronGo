using FluentAssertions;
using MarketAlly.IronGo;
using MarketAlly.IronGo.AST;
using System.Collections.Generic;
using Xunit;

namespace MarketAlly.IronGo.Tests.VisitorTests;

public class AstVisitorTests
{
    [Fact]
    public void Visitor_ShouldVisitAllNodes()
    {
        // Arrange
        const string source = @"
package main

import ""fmt""

type Person struct {
    Name string
    Age  int
}

func (p Person) Greet() {
    fmt.Printf(""Hello, I'm %s\n"", p.Name)
}

func main() {
    p := Person{Name: ""Alice"", Age: 30}
    p.Greet()
}";
        
        var ast = IronGoParser.Parse(source);
        var visitor = new NodeCountingVisitor();
        
        // Act
        ast.Accept(visitor);
        
        // Assert
        visitor.NodeCounts.Should().ContainKey(typeof(SourceFile));
        visitor.NodeCounts.Should().ContainKey(typeof(PackageDeclaration));
        visitor.NodeCounts.Should().ContainKey(typeof(ImportDeclaration));
        visitor.NodeCounts.Should().ContainKey(typeof(TypeDeclaration));
        visitor.NodeCounts.Should().ContainKey(typeof(FunctionDeclaration));
        visitor.NodeCounts.Should().ContainKey(typeof(MethodDeclaration));
        visitor.TotalNodes.Should().BeGreaterThan(20);
    }
    
    [Fact]
    public void GenericVisitor_ShouldReturnValues()
    {
        // Arrange
        const string source = @"
package main

func add(a, b int) int {
    return a + b
}

func multiply(x, y int) int {
    return x * y
}";
        
        var ast = IronGoParser.Parse(source);
        var visitor = new FunctionNameCollector();
        
        // Act
        var names = ast.Accept(visitor);
        
        // Assert
        names.Should().Equal("add", "multiply");
    }
    
    [Fact]
    public void Walker_ShouldWalkAllChildNodes()
    {
        // Arrange
        const string source = @"
package main

func test() {
    if true {
        for i := 0; i < 10; i++ {
            println(i)
        }
    }
}";
        
        var ast = IronGoParser.Parse(source);
        var walker = new DepthTrackingWalker();
        
        // Act
        ast.Accept(walker);
        
        // Assert
        walker.MaxDepth.Should().BeGreaterThan(5);
        walker.NodesAtDepth.Should().ContainKey(0); // SourceFile
        walker.NodesAtDepth.Should().ContainKey(1); // Package, Function
        walker.NodesAtDepth.Should().ContainKey(2); // Function body
        walker.NodesAtDepth.Should().ContainKey(3); // If statement
        walker.NodesAtDepth.Should().ContainKey(4); // For statement
    }
    
    [Fact]
    public void Visitor_ShouldHandleComplexExpressions()
    {
        // Arrange
        const string source = @"
package main

func test() {
    result := (a + b) * c / d - e
    ptr := &value
    elem := *ptr
    index := arr[1]
    slice := arr[1:5]
    call := fn(x, y, z)
    literal := []int{1, 2, 3}
}";
        
        var ast = IronGoParser.Parse(source);
        var visitor = new ExpressionTypeCollector();
        
        // Act
        ast.Accept(visitor);
        
        // Assert
        visitor.ExpressionTypes.Should().Contain(typeof(BinaryExpression));
        visitor.ExpressionTypes.Should().Contain(typeof(UnaryExpression));
        visitor.ExpressionTypes.Should().Contain(typeof(IndexExpression));
        visitor.ExpressionTypes.Should().Contain(typeof(CallExpression));
        visitor.ExpressionTypes.Should().Contain(typeof(CompositeLiteral));
    }
}

// Test visitor implementations
public class NodeCountingVisitor : GoAstWalker
{
    public Dictionary<Type, int> NodeCounts { get; } = new();
    public int TotalNodes { get; private set; }
    
    private void CountNode(IGoNode node)
    {
        var type = node.GetType();
        NodeCounts.TryGetValue(type, out var count);
        NodeCounts[type] = count + 1;
        TotalNodes++;
    }
    
    // Override all visit methods to count nodes
    public override void VisitSourceFile(SourceFile node)
    {
        CountNode(node);
        base.VisitSourceFile(node);
    }
    
    public override void VisitPackageDeclaration(PackageDeclaration node)
    {
        CountNode(node);
        base.VisitPackageDeclaration(node);
    }
    
    public override void VisitImportDeclaration(ImportDeclaration node)
    {
        CountNode(node);
        base.VisitImportDeclaration(node);
    }
    
    public override void VisitImportSpec(ImportSpec node)
    {
        CountNode(node);
        base.VisitImportSpec(node);
    }
    
    public override void VisitFunctionDeclaration(FunctionDeclaration node)
    {
        CountNode(node);
        base.VisitFunctionDeclaration(node);
    }
    
    public override void VisitMethodDeclaration(MethodDeclaration node)
    {
        CountNode(node);
        base.VisitMethodDeclaration(node);
    }
    
    public override void VisitTypeDeclaration(TypeDeclaration node)
    {
        CountNode(node);
        base.VisitTypeDeclaration(node);
    }
    
    public override void VisitTypeSpec(TypeSpec node)
    {
        CountNode(node);
        base.VisitTypeSpec(node);
    }
    
    public override void VisitBlockStatement(BlockStatement node)
    {
        CountNode(node);
        base.VisitBlockStatement(node);
    }
    
    public override void VisitExpressionStatement(ExpressionStatement node)
    {
        CountNode(node);
        base.VisitExpressionStatement(node);
    }
    
    public override void VisitCallExpression(CallExpression node)
    {
        CountNode(node);
        base.VisitCallExpression(node);
    }
    
    public override void VisitLiteralExpression(LiteralExpression node)
    {
        CountNode(node);
        base.VisitLiteralExpression(node);
    }
    
    public override void VisitIdentifierExpression(IdentifierExpression node)
    {
        CountNode(node);
        base.VisitIdentifierExpression(node);
    }
    
    public override void VisitSelectorExpression(SelectorExpression node)
    {
        CountNode(node);
        base.VisitSelectorExpression(node);
    }
    
    public override void VisitStructType(StructType node)
    {
        CountNode(node);
        base.VisitStructType(node);
    }
    
    public override void VisitFieldDeclaration(FieldDeclaration node)
    {
        CountNode(node);
        base.VisitFieldDeclaration(node);
    }
    
    public override void VisitIdentifierType(IdentifierType node)
    {
        CountNode(node);
        base.VisitIdentifierType(node);
    }
    
    public override void VisitParameter(Parameter node)
    {
        CountNode(node);
        base.VisitParameter(node);
    }
    
    public override void VisitShortVariableDeclaration(ShortVariableDeclaration node)
    {
        CountNode(node);
        base.VisitShortVariableDeclaration(node);
    }
    
    public override void VisitCompositeLiteral(CompositeLiteral node)
    {
        CountNode(node);
        base.VisitCompositeLiteral(node);
    }
    
    public override void VisitCompositeLiteralElement(CompositeLiteralElement node)
    {
        CountNode(node);
        base.VisitCompositeLiteralElement(node);
    }
}

public class FunctionNameCollector : GoAstVisitorBase<List<string>>
{
    private readonly List<string> _names = new();
    
    protected override List<string> DefaultResult => _names;
    
    public override List<string> VisitSourceFile(SourceFile node)
    {
        foreach (var decl in node.Declarations)
        {
            decl.Accept(this);
        }
        return _names;
    }
    
    public override List<string> VisitFunctionDeclaration(FunctionDeclaration node)
    {
        _names.Add(node.Name);
        return _names;
    }
    
    // Default implementations for other visit methods
    public override List<string> VisitPackageDeclaration(PackageDeclaration node) => _names;
    public override List<string> VisitImportDeclaration(ImportDeclaration node) => _names;
    public override List<string> VisitMethodDeclaration(MethodDeclaration node) => _names;
    public override List<string> VisitTypeDeclaration(TypeDeclaration node) => _names;
    public override List<string> VisitConstDeclaration(ConstDeclaration node) => _names;
    public override List<string> VisitVariableDeclaration(VariableDeclaration node) => _names;
    // ... etc
}

public class DepthTrackingWalker : GoAstWalker
{
    private int _currentDepth = 0;
    public int MaxDepth { get; private set; }
    public Dictionary<int, int> NodesAtDepth { get; } = new();
    
    private void EnterNode()
    {
        NodesAtDepth.TryGetValue(_currentDepth, out var count);
        NodesAtDepth[_currentDepth] = count + 1;
        
        if (_currentDepth > MaxDepth)
            MaxDepth = _currentDepth;
        
        _currentDepth++;
    }
    
    private void ExitNode()
    {
        _currentDepth--;
    }
    
    public override void VisitSourceFile(SourceFile node)
    {
        EnterNode();
        base.VisitSourceFile(node);
        ExitNode();
    }
    
    public override void VisitFunctionDeclaration(FunctionDeclaration node)
    {
        EnterNode();
        base.VisitFunctionDeclaration(node);
        ExitNode();
    }
    
    public override void VisitBlockStatement(BlockStatement node)
    {
        EnterNode();
        base.VisitBlockStatement(node);
        ExitNode();
    }
    
    public override void VisitIfStatement(IfStatement node)
    {
        EnterNode();
        base.VisitIfStatement(node);
        ExitNode();
    }
    
    public override void VisitForStatement(ForStatement node)
    {
        EnterNode();
        base.VisitForStatement(node);
        ExitNode();
    }
    
    // ... other visit methods would track depth similarly
}

public class ExpressionTypeCollector : GoAstWalker
{
    public HashSet<Type> ExpressionTypes { get; } = new();
    
    public override void VisitBinaryExpression(BinaryExpression node)
    {
        ExpressionTypes.Add(typeof(BinaryExpression));
        base.VisitBinaryExpression(node);
    }
    
    public override void VisitUnaryExpression(UnaryExpression node)
    {
        ExpressionTypes.Add(typeof(UnaryExpression));
        base.VisitUnaryExpression(node);
    }
    
    public override void VisitIndexExpression(IndexExpression node)
    {
        ExpressionTypes.Add(typeof(IndexExpression));
        base.VisitIndexExpression(node);
    }
    
    public override void VisitCallExpression(CallExpression node)
    {
        ExpressionTypes.Add(typeof(CallExpression));
        base.VisitCallExpression(node);
    }
    
    public override void VisitCompositeLiteral(CompositeLiteral node)
    {
        ExpressionTypes.Add(typeof(CompositeLiteral));
        base.VisitCompositeLiteral(node);
    }
}