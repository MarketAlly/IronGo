namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a parenthesized expression
/// </summary>
public class ParenthesizedExpression : GoNodeBase, IExpression
{
    public IExpression Expression { get; }
    
    public ParenthesizedExpression(IExpression expression, Position start, Position end) : base(start, end)
    {
        Expression = expression;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitParenthesizedExpression(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitParenthesizedExpression(this);
}