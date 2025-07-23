namespace IronGo.AST;

/// <summary>
/// Represents a type conversion expression
/// </summary>
public class ConversionExpression : GoNodeBase, IExpression
{
    public IType Type { get; }
    public IExpression Expression { get; }
    
    public ConversionExpression(IType type, IExpression expression, Position start, Position end) : base(start, end)
    {
        Type = type;
        Expression = expression;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitConversionExpression(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitConversionExpression(this);
}