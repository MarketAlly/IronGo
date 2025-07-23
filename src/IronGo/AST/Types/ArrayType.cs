namespace IronGo.AST;

/// <summary>
/// Represents an array type (e.g., "[10]int")
/// </summary>
public class ArrayType : GoNodeBase, IType
{
    public IExpression? Length { get; }
    public IType ElementType { get; }
    
    public ArrayType(IExpression? length, IType elementType, Position start, Position end) : base(start, end)
    {
        Length = length;
        ElementType = elementType;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitArrayType(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitArrayType(this);
}