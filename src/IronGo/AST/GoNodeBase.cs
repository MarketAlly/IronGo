namespace MarketAlly.IronGo.AST;

/// <summary>
/// Base class for all Go AST nodes
/// </summary>
public abstract class GoNodeBase : IGoNode
{
    public Position Start { get; protected set; }
    public Position End { get; protected set; }
    
    protected GoNodeBase(Position start, Position end)
    {
        Start = start;
        End = end;
    }
    
    public abstract void Accept(IGoAstVisitor visitor);
    public abstract T Accept<T>(IGoAstVisitor<T> visitor);
}