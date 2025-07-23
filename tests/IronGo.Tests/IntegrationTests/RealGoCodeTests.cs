using FluentAssertions;
using IronGo;
using IronGo.AST;
using IronGo.Serialization;
using IronGo.Utilities;
using System.IO;
using System.Linq;
using Xunit;

namespace IronGo.Tests.IntegrationTests;

public class RealGoCodeTests
{
    private string GetTestDataPath(string filename)
    {
        return Path.Combine("TestData", filename);
    }
    
    [Fact]
    public void Parse_HelloWorldGo_ShouldParseCorrectly()
    {
        // Arrange
        var path = GetTestDataPath("hello.go");
        var source = File.ReadAllText(path);
        
        // Act
        var ast = IronGoParser.Parse(source);
        
        // Assert
        ast.Should().NotBeNull();
        ast.Package!.Name.Should().Be("main");
        ast.Imports.Should().HaveCount(1);
        ast.GetImportedPackages().Should().ContainSingle("fmt");
        
        var mainFunc = ast.FindFunction("main");
        mainFunc.Should().NotBeNull();
        mainFunc!.Parameters.Should().BeEmpty();
        mainFunc.Body!.Statements.Should().HaveCount(1);
        
        var calls = ast.GetAllCalls().ToList();
        calls.Should().HaveCount(1);
        
        var printlnCall = calls[0];
        printlnCall.Function.Should().BeOfType<SelectorExpression>()
            .Which.Selector.Should().Be("Println");
    }
    
    [Fact]
    public void Parse_TypesGo_ShouldParseAllTypeDeclarations()
    {
        // Arrange
        var path = GetTestDataPath("types.go");
        var source = File.ReadAllText(path);
        
        // Act
        var result = IronGoParser.ParseWithDiagnostics(source);
        
        // Assert
        result.Diagnostics.Errors.Should().BeEmpty();
        
        var ast = result.SourceFile;
        ast.Package!.Name.Should().Be("types");
        
        // Check type declarations
        var types = ast.GetTypes().ToList();
        types.Should().Contain(t => t.Name == "ID");
        types.Should().Contain(t => t.Name == "Person");
        types.Should().Contain(t => t.Name == "Address");
        types.Should().Contain(t => t.Name == "Writer");
        types.Should().Contain(t => t.Name == "Reader");
        types.Should().Contain(t => t.Name == "ReadWriter");
        types.Should().Contain(t => t.Name == "List");
        types.Should().Contain(t => t.Name == "Map");
        
        // Check struct fields
        var personType = types.First(t => t.Name == "Person");
        var personStruct = personType.Specs[0].Type.Should().BeOfType<StructType>().Subject;
        personStruct.Fields.Should().HaveCountGreaterThan(5);
        personStruct.Fields[0].Tag.Should().Contain("json:");
        
        // Check interfaces
        var writerType = types.First(t => t.Name == "Writer");
        var writerInterface = writerType.Specs[0].Type.Should().BeOfType<InterfaceType>().Subject;
        writerInterface.Methods.Should().HaveCount(1);
        
        // Check generic types
        var listType = types.First(t => t.Name == "List");
        listType.Specs[0].TypeParameters.Should().HaveCount(1);
        listType.Specs[0].TypeParameters![0].Name.Should().Be("T");
        
        // Check methods
        var methods = ast.GetMethods().ToList();
        methods.Should().Contain(m => m.Name == "FullName");
        methods.Should().Contain(m => m.Name == "IsAdult");
        methods.Should().Contain(m => m.Name == "Add");
        methods.Should().Contain(m => m.Name == "Get");
    }
    
    [Fact]
    public void Parse_ComplexGo_ShouldHandleComplexLanguageFeatures()
    {
        // Arrange
        var path = GetTestDataPath("complex.go");
        var source = File.ReadAllText(path);
        
        // Act
        var result = IronGoParser.ParseWithDiagnostics(source);
        
        // Assert
        result.Diagnostics.Errors.Should().BeEmpty();
        result.Diagnostics.TokenCount.Should().BeGreaterThan(1000);
        
        var ast = result.SourceFile;
        
        // Check imports
        var imports = ast.GetImportedPackages().ToList();
        imports.Should().Contain("context");
        imports.Should().Contain("errors");
        imports.Should().Contain("fmt");
        imports.Should().Contain("log");
        imports.Should().Contain("sync");
        imports.Should().Contain("time");
        
        // Check constants
        var constDeclarations = ast.FindNodes<ConstDeclaration>().ToList();
        constDeclarations.Should().HaveCount(2); // Two const blocks
        var allConstants = constDeclarations.SelectMany(c => c.Specs).ToList();
        allConstants.Should().HaveCountGreaterThan(3); // More than 3 individual constants
        
        // Check global variables
        var globals = ast.Declarations.OfType<VariableDeclaration>().ToList();
        globals.Should().HaveCount(1); // One var block with multiple specs
        
        // Check complex struct types
        var serverType = ast.GetTypes().First(t => t.Name == "Server");
        var serverStruct = serverType.Specs[0].Type as StructType;
        serverStruct.Should().NotBeNull();
        serverStruct!.Fields.Should().HaveCountGreaterThan(5);
        
        // Check methods with goroutines
        var startMethod = ast.GetMethods().First(m => m.Name == "Start");
        var goStatements = startMethod.Body!.FindNodes<GoStatement>().ToList();
        goStatements.Should().HaveCountGreaterThan(1);
        
        // Check channels
        var channelTypes = ast.FindNodes<ChannelType>().ToList();
        channelTypes.Should().NotBeEmpty();
        
        // Check select statements
        var selectStatements = ast.FindNodes<SelectStatement>().ToList();
        selectStatements.Should().NotBeEmpty();
        
        // Check defer statements
        var deferStatements = ast.FindNodes<DeferStatement>().ToList();
        deferStatements.Should().HaveCountGreaterThan(3);
        
        // Check generic functions
        var genericFunctions = ast.GetFunctions()
            .Where(f => f.TypeParameters != null && f.TypeParameters.Count > 0)
            .ToList();
        genericFunctions.Should().NotBeEmpty();
        genericFunctions.Should().Contain(f => f.Name == "Map");
        genericFunctions.Should().Contain(f => f.Name == "Sum");
        
        // Check error handling
        var errorReturns = ast.GetFunctions()
            .Where(f => f.ReturnParameters?.Any(p => 
                p.Type is IdentifierType ident && ident.Name == "error") == true)
            .ToList();
        errorReturns.Should().NotBeEmpty();
        
        // Check init function
        var initFunc = ast.FindFunction("init");
        initFunc.Should().NotBeNull();
    }
    
    [Fact]
    public void JsonSerialization_ComplexGo_ShouldSerializeWithoutErrors()
    {
        // Arrange
        var path = GetTestDataPath("complex.go");
        var source = File.ReadAllText(path);
        var ast = IronGoParser.Parse(source);
        
        // Act
        var json = ast.ToJsonCompact();
        
        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Length.Should().BeGreaterThan(10000); // Complex AST should produce substantial JSON
        json.Should().Contain("\"Type\":\"SourceFile\""); // Compact JSON has no spaces
        json.Should().Contain("\"Package\":");
        json.Should().Contain("Server");
        json.Should().Contain("Client");
        // The word "goroutine" appears in comments, but JSON doesn't serialize comments
    }
    
    [Fact]
    public void Performance_ParseComplexFile_ShouldBeFast()
    {
        // Arrange
        var path = GetTestDataPath("complex.go");
        var source = File.ReadAllText(path);
        var parser = new IronGoParser();
        
        // Warm up cache
        parser.ParseSource(source);
        
        // Act
        var result = parser.ParseSourceWithDiagnostics(source);
        
        // Assert
        result.Diagnostics.ParseTimeMs.Should().BeLessThan(5); // Should be very fast from cache
    }
    
    [Fact]
    public void Visitor_ComplexGo_ShouldVisitAllNodeTypes()
    {
        // Arrange
        var path = GetTestDataPath("complex.go");
        var source = File.ReadAllText(path);
        var ast = IronGoParser.Parse(source);
        
        // Act
        var visitor = new NodeTypeCollector();
        ast.Accept(visitor);
        
        // Assert
        visitor.NodeTypes.Should().Contain(typeof(SourceFile));
        visitor.NodeTypes.Should().Contain(typeof(PackageDeclaration));
        visitor.NodeTypes.Should().Contain(typeof(ImportDeclaration));
        visitor.NodeTypes.Should().Contain(typeof(FunctionDeclaration));
        visitor.NodeTypes.Should().Contain(typeof(MethodDeclaration));
        visitor.NodeTypes.Should().Contain(typeof(TypeDeclaration));
        visitor.NodeTypes.Should().Contain(typeof(VariableDeclaration));
        visitor.NodeTypes.Should().Contain(typeof(ConstDeclaration));
        visitor.NodeTypes.Should().Contain(typeof(StructType));
        visitor.NodeTypes.Should().Contain(typeof(InterfaceType));
        visitor.NodeTypes.Should().Contain(typeof(ChannelType));
        visitor.NodeTypes.Should().Contain(typeof(MapType));
        visitor.NodeTypes.Should().Contain(typeof(SliceType));
        visitor.NodeTypes.Should().Contain(typeof(GoStatement));
        visitor.NodeTypes.Should().Contain(typeof(DeferStatement));
        visitor.NodeTypes.Should().Contain(typeof(SelectStatement));
        visitor.NodeTypes.Should().Contain(typeof(ForStatement));
        visitor.NodeTypes.Should().Contain(typeof(IfStatement));
        visitor.NodeTypes.Should().Contain(typeof(SwitchStatement));
        visitor.NodeTypes.Should().Contain(typeof(CallExpression));
        visitor.NodeTypes.Should().Contain(typeof(BinaryExpression));
        visitor.NodeTypes.Should().Contain(typeof(UnaryExpression));
    }
}

public class NodeTypeCollector : GoAstWalker
{
    public HashSet<Type> NodeTypes { get; } = new();
    
    private void CollectType(IGoNode node)
    {
        NodeTypes.Add(node.GetType());
    }
    
    public override void VisitSourceFile(SourceFile node) { CollectType(node); base.VisitSourceFile(node); }
    public override void VisitPackageDeclaration(PackageDeclaration node) { CollectType(node); base.VisitPackageDeclaration(node); }
    public override void VisitImportDeclaration(ImportDeclaration node) { CollectType(node); base.VisitImportDeclaration(node); }
    public override void VisitFunctionDeclaration(FunctionDeclaration node) { CollectType(node); base.VisitFunctionDeclaration(node); }
    public override void VisitMethodDeclaration(MethodDeclaration node) { CollectType(node); base.VisitMethodDeclaration(node); }
    public override void VisitTypeDeclaration(TypeDeclaration node) { CollectType(node); base.VisitTypeDeclaration(node); }
    public override void VisitConstDeclaration(ConstDeclaration node) { CollectType(node); base.VisitConstDeclaration(node); }
    public override void VisitVariableDeclaration(VariableDeclaration node) { CollectType(node); base.VisitVariableDeclaration(node); }
    public override void VisitStructType(StructType node) { CollectType(node); base.VisitStructType(node); }
    public override void VisitInterfaceType(InterfaceType node) { CollectType(node); base.VisitInterfaceType(node); }
    public override void VisitChannelType(ChannelType node) { CollectType(node); base.VisitChannelType(node); }
    public override void VisitMapType(MapType node) { CollectType(node); base.VisitMapType(node); }
    public override void VisitSliceType(SliceType node) { CollectType(node); base.VisitSliceType(node); }
    public override void VisitGoStatement(GoStatement node) { CollectType(node); base.VisitGoStatement(node); }
    public override void VisitDeferStatement(DeferStatement node) { CollectType(node); base.VisitDeferStatement(node); }
    public override void VisitSelectStatement(SelectStatement node) { CollectType(node); base.VisitSelectStatement(node); }
    public override void VisitForStatement(ForStatement node) { CollectType(node); base.VisitForStatement(node); }
    public override void VisitIfStatement(IfStatement node) { CollectType(node); base.VisitIfStatement(node); }
    public override void VisitSwitchStatement(SwitchStatement node) { CollectType(node); base.VisitSwitchStatement(node); }
    public override void VisitCallExpression(CallExpression node) { CollectType(node); base.VisitCallExpression(node); }
    public override void VisitBinaryExpression(BinaryExpression node) { CollectType(node); base.VisitBinaryExpression(node); }
    public override void VisitUnaryExpression(UnaryExpression node) { CollectType(node); base.VisitUnaryExpression(node); }
}