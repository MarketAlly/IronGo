namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a type element in an interface (for type sets)
/// </summary>
public class TypeElement : GoNodeBase
{
    public IType Type { get; }
    
    public TypeElement(
        IType type,
        Position start,
        Position end) : base(start, end)
    {
        Type = type;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitTypeElement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitTypeElement(this);
}