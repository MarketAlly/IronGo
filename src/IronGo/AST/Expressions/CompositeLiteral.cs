using System.Collections.Generic;

namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a composite literal (e.g., []int{1, 2, 3})
/// </summary>
public class CompositeLiteral : GoNodeBase, IExpression
{
    public IType? Type { get; }
    public IReadOnlyList<IExpression> Elements { get; }
    
    public CompositeLiteral(IType? type, IReadOnlyList<IExpression> elements, Position start, Position end) : base(start, end)
    {
        Type = type;
        Elements = elements;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitCompositeLiteral(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitCompositeLiteral(this);
}

/// <summary>
/// Represents an element in a composite literal
/// </summary>
public class CompositeLiteralElement : GoNodeBase
{
    public IExpression? Key { get; }
    public IExpression Value { get; }
    
    public CompositeLiteralElement(IExpression? key, IExpression value, Position start, Position end) : base(start, end)
    {
        Key = key;
        Value = value;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitCompositeLiteralElement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitCompositeLiteralElement(this);
}