namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents an identifier expression
/// </summary>
public class IdentifierExpression : GoNodeBase, IExpression
{
    public string Name { get; }
    
    public IdentifierExpression(string name, Position start, Position end) : base(start, end)
    {
        Name = name;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitIdentifierExpression(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitIdentifierExpression(this);
}