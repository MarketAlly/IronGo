namespace IronGo.AST;

/// <summary>
/// Represents a slice expression (e.g., x[low:high:max])
/// </summary>
public class SliceExpression : GoNodeBase, IExpression
{
    public IExpression X { get; }
    public IExpression? Low { get; }
    public IExpression? High { get; }
    public IExpression? Max { get; }
    
    public SliceExpression(
        IExpression x,
        IExpression? low,
        IExpression? high,
        IExpression? max,
        Position start,
        Position end) : base(start, end)
    {
        X = x;
        Low = low;
        High = high;
        Max = max;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitSliceExpression(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitSliceExpression(this);
}