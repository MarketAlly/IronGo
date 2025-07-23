using System.Collections.Generic;

namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents an interface type
/// </summary>
public class InterfaceType : GoNodeBase, IType
{
    public IReadOnlyList<IDeclaration> Methods { get; }
    public IReadOnlyList<TypeElement> TypeElements { get; }
    
    public InterfaceType(
        IReadOnlyList<IDeclaration> methods, 
        IReadOnlyList<TypeElement>? typeElements,
        Position start, 
        Position end) : base(start, end)
    {
        Methods = methods;
        TypeElements = typeElements ?? new List<TypeElement>();
    }
    
    // Backward compatibility constructor
    public InterfaceType(IReadOnlyList<IDeclaration> methods, Position start, Position end) 
        : this(methods, null, start, end)
    {
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitInterfaceType(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitInterfaceType(this);
}

/// <summary>
/// Represents a method in an interface
/// </summary>
public class InterfaceMethod : GoNodeBase, IDeclaration
{
    public string Name { get; }
    public FunctionType Signature { get; }
    
    public InterfaceMethod(string name, FunctionType signature, Position start, Position end) : base(start, end)
    {
        Name = name;
        Signature = signature;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitInterfaceMethod(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitInterfaceMethod(this);
}

/// <summary>
/// Represents an embedded type in an interface
/// </summary>
public class InterfaceEmbedding : GoNodeBase, IDeclaration
{
    public IType Type { get; }
    public string? Name => null; // Embedded types don't have a name
    
    public InterfaceEmbedding(IType type, Position start, Position end) : base(start, end)
    {
        Type = type;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitInterfaceEmbedding(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitInterfaceEmbedding(this);
}