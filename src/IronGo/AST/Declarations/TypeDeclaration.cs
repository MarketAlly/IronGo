using System.Collections.Generic;

namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a type declaration (e.g., "type MyType int")
/// </summary>
public class TypeDeclaration : GoNodeBase, IDeclaration
{
    public IReadOnlyList<TypeSpec> Specs { get; }
    public string? Name => Specs.Count == 1 ? Specs[0].Name : null;
    
    public TypeDeclaration(IReadOnlyList<TypeSpec> specs, Position start, Position end) : base(start, end)
    {
        Specs = specs;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitTypeDeclaration(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitTypeDeclaration(this);
}

/// <summary>
/// Represents a single type specification
/// </summary>
public class TypeSpec : GoNodeBase
{
    public string Name { get; }
    public IReadOnlyList<TypeParameter>? TypeParameters { get; }
    public IType Type { get; }
    
    public TypeSpec(string name, IReadOnlyList<TypeParameter>? typeParameters, IType type, Position start, Position end) : base(start, end)
    {
        Name = name;
        TypeParameters = typeParameters;
        Type = type;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitTypeSpec(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitTypeSpec(this);
}