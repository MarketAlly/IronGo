namespace IronGo.AST;

/// <summary>
/// Represents a channel type (e.g., "chan int", "&lt;-chan int", "chan&lt;- int")
/// </summary>
public class ChannelType : GoNodeBase, IType
{
    public ChannelDirection Direction { get; }
    public IType ElementType { get; }
    
    public ChannelType(ChannelDirection direction, IType elementType, Position start, Position end) : base(start, end)
    {
        Direction = direction;
        ElementType = elementType;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitChannelType(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitChannelType(this);
}

public enum ChannelDirection
{
    Bidirectional,  // chan T
    SendOnly,       // chan&lt;- T
    ReceiveOnly     // &lt;-chan T
}