namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a defer statement
/// </summary>
public class DeferStatement : GoNodeBase, IStatement
{
    public IExpression Call { get; }
    
    public DeferStatement(IExpression call, Position start, Position end) : base(start, end)
    {
        Call = call;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitDeferStatement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitDeferStatement(this);
}