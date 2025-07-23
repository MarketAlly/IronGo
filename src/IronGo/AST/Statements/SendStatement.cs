namespace IronGo.AST;

/// <summary>
/// Represents a send statement (channel &lt;- value)
/// </summary>
public class SendStatement : GoNodeBase, IStatement
{
    public IExpression Channel { get; }
    public IExpression Value { get; }
    
    public SendStatement(IExpression channel, IExpression value, Position start, Position end) : base(start, end)
    {
        Channel = channel;
        Value = value;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitSendStatement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitSendStatement(this);
}