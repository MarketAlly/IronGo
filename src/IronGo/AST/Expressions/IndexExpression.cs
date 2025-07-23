namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents an index expression (e.g., x[i])
/// </summary>
public class IndexExpression : GoNodeBase, IExpression
{
    public IExpression X { get; }
    public IExpression Index { get; }
    
    public IndexExpression(IExpression x, IExpression index, Position start, Position end) : base(start, end)
    {
        X = x;
        Index = index;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitIndexExpression(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitIndexExpression(this);
}