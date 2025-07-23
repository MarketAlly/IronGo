namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents an if statement
/// </summary>
public class IfStatement : GoNodeBase, IStatement
{
    public IStatement? Init { get; }
    public IExpression Condition { get; }
    public IStatement Then { get; }
    public IStatement? Else { get; }
    
    public IfStatement(
        IStatement? init,
        IExpression condition,
        IStatement then,
        IStatement? @else,
        Position start,
        Position end) : base(start, end)
    {
        Init = init;
        Condition = condition;
        Then = then;
        Else = @else;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitIfStatement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitIfStatement(this);
}