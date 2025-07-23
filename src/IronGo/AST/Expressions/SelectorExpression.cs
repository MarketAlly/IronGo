namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a selector expression (e.g., x.field)
/// </summary>
public class SelectorExpression : GoNodeBase, IExpression
{
    public IExpression X { get; }
    public string Selector { get; }
    
    public SelectorExpression(IExpression x, string selector, Position start, Position end) : base(start, end)
    {
        X = x;
        Selector = selector;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitSelectorExpression(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitSelectorExpression(this);
}