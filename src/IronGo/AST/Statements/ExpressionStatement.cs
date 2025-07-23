namespace IronGo.AST;

/// <summary>
/// Represents an expression used as a statement
/// </summary>
public class ExpressionStatement : GoNodeBase, IStatement
{
    public IExpression Expression { get; }
    
    public ExpressionStatement(IExpression expression, Position start, Position end) : base(start, end)
    {
        Expression = expression;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitExpressionStatement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitExpressionStatement(this);
}