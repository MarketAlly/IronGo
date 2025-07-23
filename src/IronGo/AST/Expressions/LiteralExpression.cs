namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a literal expression
/// </summary>
public class LiteralExpression : GoNodeBase, IExpression
{
    public LiteralKind Kind { get; }
    public string Value { get; }
    
    public LiteralExpression(LiteralKind kind, string value, Position start, Position end) : base(start, end)
    {
        Kind = kind;
        Value = value;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitLiteralExpression(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitLiteralExpression(this);
}

public enum LiteralKind
{
    Int,
    Float,
    Imaginary,
    Rune,
    String,
    Bool,
    Nil
}