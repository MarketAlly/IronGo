namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a binary expression
/// </summary>
public class BinaryExpression : GoNodeBase, IExpression
{
    public IExpression Left { get; }
    public BinaryOperator Operator { get; }
    public IExpression Right { get; }
    
    public BinaryExpression(IExpression left, BinaryOperator op, IExpression right, Position start, Position end) : base(start, end)
    {
        Left = left;
        Operator = op;
        Right = right;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitBinaryExpression(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitBinaryExpression(this);
}

public enum BinaryOperator
{
    // Arithmetic
    Add,        // +
    Subtract,   // -
    Multiply,   // *
    Divide,     // /
    Modulo,     // %
    
    // Bitwise
    BitwiseAnd, // &
    BitwiseOr,  // |
    BitwiseXor, // ^
    LeftShift,  // <<
    RightShift, // >>
    AndNot,     // &^
    
    // Comparison
    Equal,          // ==
    NotEqual,       // !=
    Less,           // <
    LessOrEqual,    // <=
    Greater,        // >
    GreaterOrEqual, // >=
    
    // Logical
    LogicalAnd, // &&
    LogicalOr   // ||
}