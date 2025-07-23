namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a pointer type (e.g., "*int")
/// </summary>
public class PointerType : GoNodeBase, IType
{
    public IType ElementType { get; }
    
    public PointerType(IType elementType, Position start, Position end) : base(start, end)
    {
        ElementType = elementType;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitPointerType(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitPointerType(this);
}