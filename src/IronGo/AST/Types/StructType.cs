using System.Collections.Generic;

namespace IronGo.AST;

/// <summary>
/// Represents a struct type
/// </summary>
public class StructType : GoNodeBase, IType
{
    public IReadOnlyList<FieldDeclaration> Fields { get; }
    
    public StructType(IReadOnlyList<FieldDeclaration> fields, Position start, Position end) : base(start, end)
    {
        Fields = fields;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitStructType(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitStructType(this);
}

/// <summary>
/// Represents a field declaration in a struct
/// </summary>
public class FieldDeclaration : GoNodeBase
{
    public IReadOnlyList<string> Names { get; }
    public IType Type { get; }
    public string? Tag { get; }
    public bool IsEmbedded => Names.Count == 0;
    
    public FieldDeclaration(IReadOnlyList<string> names, IType type, string? tag, Position start, Position end) : base(start, end)
    {
        Names = names;
        Type = type;
        Tag = tag;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitFieldDeclaration(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitFieldDeclaration(this);
}