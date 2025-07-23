namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a slice type (e.g., "[]int")
/// </summary>
public class SliceType : GoNodeBase, IType
{
    public IType ElementType { get; }
    
    public SliceType(IType elementType, Position start, Position end) : base(start, end)
    {
        ElementType = elementType;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitSliceType(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitSliceType(this);
}