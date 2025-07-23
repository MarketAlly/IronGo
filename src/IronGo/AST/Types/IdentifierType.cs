using System.Collections.Generic;

namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a type referenced by identifier (e.g., "int", "string", "MyType")
/// Can optionally include type arguments for generic instantiation
/// </summary>
public class IdentifierType : GoNodeBase, IType
{
    public string Name { get; }
    public string? Package { get; }
    public IReadOnlyList<IType>? TypeArguments { get; }
    
    public IdentifierType(
        string name, 
        string? package, 
        IReadOnlyList<IType>? typeArguments,
        Position start, 
        Position end) : base(start, end)
    {
        Name = name;
        Package = package;
        TypeArguments = typeArguments;
    }
    
    // Backward compatibility constructor
    public IdentifierType(string name, string? package, Position start, Position end) 
        : this(name, package, null, start, end)
    {
    }
    
    public string FullName => Package != null ? $"{Package}.{Name}" : Name;
    
    public bool IsGenericInstantiation => TypeArguments != null && TypeArguments.Count > 0;
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitIdentifierType(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitIdentifierType(this);
}