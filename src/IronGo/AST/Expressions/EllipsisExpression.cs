namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents an ellipsis expression (...)
/// </summary>
public class EllipsisExpression : GoNodeBase, IExpression
{
    public IType? Type { get; }
    
    public EllipsisExpression(IType? type, Position start, Position end) : base(start, end)
    {
        Type = type;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitEllipsisExpression(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitEllipsisExpression(this);
}