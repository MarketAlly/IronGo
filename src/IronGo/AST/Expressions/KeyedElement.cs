namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a keyed element in a composite literal (key: value)
/// </summary>
public class KeyedElement : GoNodeBase, IExpression
{
    public IExpression? Key { get; }
    public IExpression Value { get; }
    
    public KeyedElement(IExpression? key, IExpression value, Position start, Position end) : base(start, end)
    {
        Key = key;
        Value = value;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitKeyedElement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitKeyedElement(this);
}