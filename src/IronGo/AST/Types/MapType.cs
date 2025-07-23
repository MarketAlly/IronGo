namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a map type (e.g., "map[string]int")
/// </summary>
public class MapType : GoNodeBase, IType
{
    public IType KeyType { get; }
    public IType ValueType { get; }
    
    public MapType(IType keyType, IType valueType, Position start, Position end) : base(start, end)
    {
        KeyType = keyType;
        ValueType = valueType;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitMapType(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitMapType(this);
}