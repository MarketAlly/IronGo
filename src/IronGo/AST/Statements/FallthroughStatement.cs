namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a fallthrough statement in a switch case
/// </summary>
public class FallthroughStatement : GoNodeBase, IStatement
{
    public FallthroughStatement(Position start, Position end) : base(start, end)
    {
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitFallthroughStatement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitFallthroughStatement(this);
}