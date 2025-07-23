namespace IronGo.AST;

/// <summary>
/// Represents a go statement (goroutine launch)
/// </summary>
public class GoStatement : GoNodeBase, IStatement
{
    public IExpression Call { get; }
    
    public GoStatement(IExpression call, Position start, Position end) : base(start, end)
    {
        Call = call;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitGoStatement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitGoStatement(this);
}