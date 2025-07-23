using System.Collections.Generic;

namespace IronGo.AST;

/// <summary>
/// Represents a generic type instantiation (e.g., List[int], Map[string, User])
/// </summary>
public class TypeInstantiation : GoNodeBase, IType
{
    public IType BaseType { get; }
    public IReadOnlyList<IType> TypeArguments { get; }
    
    public TypeInstantiation(
        IType baseType,
        IReadOnlyList<IType> typeArguments,
        Position start,
        Position end) : base(start, end)
    {
        BaseType = baseType;
        TypeArguments = typeArguments;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitTypeInstantiation(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitTypeInstantiation(this);
}