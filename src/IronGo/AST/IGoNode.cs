namespace MarketAlly.IronGo.AST;

/// <summary>
/// Base interface for all Go AST nodes
/// </summary>
public interface IGoNode
{
    /// <summary>
    /// Starting position of the node in the source code
    /// </summary>
    Position Start { get; }
    
    /// <summary>
    /// Ending position of the node in the source code
    /// </summary>
    Position End { get; }
    
    /// <summary>
    /// Accept a visitor for traversing the AST
    /// </summary>
    void Accept(IGoAstVisitor visitor);
    
    /// <summary>
    /// Accept a generic visitor that returns a result
    /// </summary>
    T Accept<T>(IGoAstVisitor<T> visitor);
}

/// <summary>
/// Represents a position in source code
/// </summary>
public readonly struct Position
{
    public int Line { get; }
    public int Column { get; }
    public int Offset { get; }
    
    public Position(int line, int column, int offset)
    {
        Line = line;
        Column = column;
        Offset = offset;
    }
    
    public override string ToString() => $"{Line}:{Column}";
}

/// <summary>
/// Token information for AST nodes
/// </summary>
public class Token
{
    public string Text { get; }
    public int Type { get; }
    public Position Start { get; }
    public Position End { get; }
    
    public Token(string text, int type, Position start, Position end)
    {
        Text = text;
        Type = type;
        Start = start;
        End = end;
    }
}