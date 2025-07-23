namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a for statement (traditional for loop)
/// </summary>
public class ForStatement : GoNodeBase, IStatement
{
    public IStatement? Init { get; }
    public IExpression? Condition { get; }
    public IStatement? Post { get; }
    public IStatement Body { get; }
    
    public ForStatement(
        IStatement? init,
        IExpression? condition,
        IStatement? post,
        IStatement body,
        Position start,
        Position end) : base(start, end)
    {
        Init = init;
        Condition = condition;
        Post = post;
        Body = body;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitForStatement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitForStatement(this);
}