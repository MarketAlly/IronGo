using System.Collections.Generic;

namespace IronGo.AST;

/// <summary>
/// Represents a union of types in constraints (e.g., int | string | float64)
/// </summary>
public class TypeUnion : GoNodeBase, IType
{
    public IReadOnlyList<TypeTerm> Terms { get; }
    
    public TypeUnion(
        IReadOnlyList<TypeTerm> terms,
        Position start,
        Position end) : base(start, end)
    {
        Terms = terms;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitTypeUnion(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitTypeUnion(this);
}

/// <summary>
/// Represents a single term in a type union, with optional underlying type operator (~)
/// </summary>
public class TypeTerm : GoNodeBase
{
    public bool IsUnderlying { get; }
    public IType Type { get; }
    
    public TypeTerm(
        bool isUnderlying,
        IType type,
        Position start,
        Position end) : base(start, end)
    {
        IsUnderlying = isUnderlying;
        Type = type;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitTypeTerm(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitTypeTerm(this);
}