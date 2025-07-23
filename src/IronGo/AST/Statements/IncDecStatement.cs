namespace IronGo.AST;

/// <summary>
/// Represents an increment or decrement statement (++ or --)
/// </summary>
public class IncDecStatement : GoNodeBase, IStatement
{
    public IExpression Expression { get; }
    public bool IsIncrement { get; }
    
    public IncDecStatement(IExpression expression, bool isIncrement, Position start, Position end) : base(start, end)
    {
        Expression = expression;
        IsIncrement = isIncrement;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitIncDecStatement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitIncDecStatement(this);
}