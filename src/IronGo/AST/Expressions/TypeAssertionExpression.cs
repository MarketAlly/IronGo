namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a type assertion expression (e.g., x.(T))
/// </summary>
public class TypeAssertionExpression : GoNodeBase, IExpression
{
    public IExpression X { get; }
    public IType? Type { get; }
    
    public TypeAssertionExpression(IExpression x, IType? type, Position start, Position end) : base(start, end)
    {
        X = x;
        Type = type;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitTypeAssertionExpression(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitTypeAssertionExpression(this);
}