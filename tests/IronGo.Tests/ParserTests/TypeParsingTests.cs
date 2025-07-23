using FluentAssertions;
using IronGo;
using IronGo.AST;
using Xunit;

namespace IronGo.Tests.ParserTests;

public class TypeParsingTests
{
    [Fact]
    public void Parse_StructDeclaration_ShouldParseStructType()
    {
        // Arrange
        const string source = @"
package main

type Point struct {
    X, Y int
    Name string
}

type Person struct {
    Name    string `json:""name""`
    Age     int    `json:""age""`
    Address struct {
        Street string
        City   string
    }
}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        result.Declarations.Should().HaveCount(2);
        
        var point = result.Declarations[0].Should().BeOfType<TypeDeclaration>().Subject;
        point.Name.Should().Be("Point");
        point.Specs[0].Type.Should().BeOfType<StructType>()
            .Which.Fields.Should().HaveCount(2);
        
        var person = result.Declarations[1].Should().BeOfType<TypeDeclaration>().Subject;
        person.Name.Should().Be("Person");
        var personStruct = person.Specs[0].Type.Should().BeOfType<StructType>().Subject;
        personStruct.Fields.Should().HaveCount(3);
        personStruct.Fields[0].Tag.Should().Be("`json:\"name\"`");
    }
    
    [Fact]
    public void Parse_InterfaceDeclaration_ShouldParseInterfaceType()
    {
        // Arrange
        const string source = @"
package main

type Writer interface {
    Write([]byte) (int, error)
}

type ReadWriter interface {
    Reader
    Writer
    Close() error
}

type Empty interface{}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        result.Declarations.Should().HaveCount(3);
        
        var writer = result.Declarations[0].Should().BeOfType<TypeDeclaration>().Subject;
        writer.Specs[0].Type.Should().BeOfType<InterfaceType>()
            .Which.Methods.Should().HaveCount(1);
        
        var readWriter = result.Declarations[1].Should().BeOfType<TypeDeclaration>().Subject;
        readWriter.Specs[0].Type.Should().BeOfType<InterfaceType>()
            .Which.Methods.Should().HaveCount(3); // 2 embedded + 1 method
        
        var empty = result.Declarations[2].Should().BeOfType<TypeDeclaration>().Subject;
        empty.Specs[0].Type.Should().BeOfType<InterfaceType>()
            .Which.Methods.Should().BeEmpty();
    }
    
    [Fact]
    public void Parse_TypeAlias_ShouldParseAliasDeclaration()
    {
        // Arrange
        const string source = @"
package main

type MyInt int
type StringList []string
type Handler func(string) error";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        result.Declarations.Should().HaveCount(3);
        
        var myInt = result.Declarations[0].Should().BeOfType<TypeDeclaration>().Subject;
        myInt.Name.Should().Be("MyInt");
        myInt.Specs[0].Type.Should().BeOfType<IdentifierType>()
            .Which.Name.Should().Be("int");
        
        var stringList = result.Declarations[1].Should().BeOfType<TypeDeclaration>().Subject;
        stringList.Specs[0].Type.Should().BeOfType<SliceType>()
            .Which.ElementType.Should().BeOfType<IdentifierType>()
            .Which.Name.Should().Be("string");
        
        var handler = result.Declarations[2].Should().BeOfType<TypeDeclaration>().Subject;
        handler.Specs[0].Type.Should().BeOfType<FunctionType>();
    }
    
    [Fact]
    public void Parse_PointerTypes_ShouldParsePointerType()
    {
        // Arrange
        const string source = @"
package main

type IntPtr *int
type NodePtr *struct {
    Value int
    Next  *NodePtr
}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        var intPtr = result.Declarations[0].Should().BeOfType<TypeDeclaration>().Subject;
        intPtr.Specs[0].Type.Should().BeOfType<PointerType>()
            .Which.ElementType.Should().BeOfType<IdentifierType>()
            .Which.Name.Should().Be("int");
        
        var nodePtr = result.Declarations[1].Should().BeOfType<TypeDeclaration>().Subject;
        nodePtr.Specs[0].Type.Should().BeOfType<PointerType>()
            .Which.ElementType.Should().BeOfType<StructType>();
    }
    
    [Fact]
    public void Parse_SliceAndArrayTypes_ShouldParseCorrectly()
    {
        // Arrange
        const string source = @"
package main

type IntArray [10]int
type StringSlice []string
type Matrix [3][4]float64
type ByteSlice []byte";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        var intArray = result.Declarations[0].Should().BeOfType<TypeDeclaration>().Subject;
        var arrayType = intArray.Specs[0].Type.Should().BeOfType<ArrayType>().Subject;
        arrayType.Length.Should().BeOfType<LiteralExpression>()
            .Which.Value.Should().Be("10");
        
        var stringSlice = result.Declarations[1].Should().BeOfType<TypeDeclaration>().Subject;
        stringSlice.Specs[0].Type.Should().BeOfType<SliceType>()
            .Which.ElementType.Should().BeOfType<IdentifierType>()
            .Which.Name.Should().Be("string");
        
        var matrix = result.Declarations[2].Should().BeOfType<TypeDeclaration>().Subject;
        matrix.Specs[0].Type.Should().BeOfType<ArrayType>()
            .Which.ElementType.Should().BeOfType<ArrayType>();
    }
    
    [Fact]
    public void Parse_MapTypes_ShouldParseMapType()
    {
        // Arrange
        const string source = @"
package main

type StringMap map[string]string
type Counters map[string]int
type Cache map[string]interface{}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        var stringMap = result.Declarations[0].Should().BeOfType<TypeDeclaration>().Subject;
        var mapType = stringMap.Specs[0].Type.Should().BeOfType<MapType>().Subject;
        mapType.KeyType.Should().BeOfType<IdentifierType>()
            .Which.Name.Should().Be("string");
        mapType.ValueType.Should().BeOfType<IdentifierType>()
            .Which.Name.Should().Be("string");
    }
    
    [Fact]
    public void Parse_ChannelTypes_ShouldParseChannelDirections()
    {
        // Arrange
        const string source = @"
package main

type SendOnly chan<- int
type ReceiveOnly <-chan int
type Bidirectional chan string";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        var sendOnly = result.Declarations[0].Should().BeOfType<TypeDeclaration>().Subject;
        var sendChan = sendOnly.Specs[0].Type.Should().BeOfType<ChannelType>().Subject;
        sendChan.Direction.Should().Be(ChannelDirection.SendOnly);
        
        var receiveOnly = result.Declarations[1].Should().BeOfType<TypeDeclaration>().Subject;
        var recvChan = receiveOnly.Specs[0].Type.Should().BeOfType<ChannelType>().Subject;
        recvChan.Direction.Should().Be(ChannelDirection.ReceiveOnly);
        
        var bidirectional = result.Declarations[2].Should().BeOfType<TypeDeclaration>().Subject;
        var biChan = bidirectional.Specs[0].Type.Should().BeOfType<ChannelType>().Subject;
        biChan.Direction.Should().Be(ChannelDirection.Bidirectional);
    }
    
    [Fact]
    public void Parse_FunctionTypes_ShouldParseFunctionSignatures()
    {
        // Arrange
        const string source = @"
package main

type SimpleFunc func()
type UnaryFunc func(int) int
type BinaryFunc func(int, int) int
type ErrorFunc func(string) (int, error)
type VariadicFunc func(string, ...interface{})";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        var simpleFunc = result.Declarations[0].Should().BeOfType<TypeDeclaration>().Subject;
        var funcType1 = simpleFunc.Specs[0].Type.Should().BeOfType<FunctionType>().Subject;
        funcType1.Parameters.Should().BeEmpty();
        funcType1.ReturnParameters.Should().BeNull();
        
        var errorFunc = result.Declarations[3].Should().BeOfType<TypeDeclaration>().Subject;
        var funcType2 = errorFunc.Specs[0].Type.Should().BeOfType<FunctionType>().Subject;
        funcType2.Parameters.Should().HaveCount(1);
        funcType2.ReturnParameters.Should().HaveCount(2);
        
        var variadicFunc = result.Declarations[4].Should().BeOfType<TypeDeclaration>().Subject;
        var funcType3 = variadicFunc.Specs[0].Type.Should().BeOfType<FunctionType>().Subject;
        funcType3.Parameters.Should().HaveCount(2);
        funcType3.Parameters[1].IsVariadic.Should().BeTrue();
    }
    
    [Fact]
    public void Parse_GenericTypes_ShouldParseTypeParameters()
    {
        // Arrange
        const string source = @"
package main

type List[T any] struct {
    items []T
}

type Map[K comparable, V any] struct {
    data map[K]V
}

type Constraint interface {
    ~int | ~int64 | ~float64
}

type Number[T Constraint] struct {
    value T
}";
        
        // Act
        var result = IronGoParser.Parse(source);
        
        // Assert
        var list = result.Declarations[0].Should().BeOfType<TypeDeclaration>().Subject;
        list.Specs[0].TypeParameters.Should().HaveCount(1);
        list.Specs[0].TypeParameters![0].Name.Should().Be("T");
        
        var map = result.Declarations[1].Should().BeOfType<TypeDeclaration>().Subject;
        map.Specs[0].TypeParameters.Should().HaveCount(2);
        map.Specs[0].TypeParameters![0].Name.Should().Be("K");
        map.Specs[0].TypeParameters![1].Name.Should().Be("V");
    }
}