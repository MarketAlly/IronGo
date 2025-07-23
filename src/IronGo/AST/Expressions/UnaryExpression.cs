namespace IronGo.AST;

/// <summary>
/// Represents a unary expression
/// </summary>
public class UnaryExpression : GoNodeBase, IExpression
{
    public UnaryOperator Operator { get; }
    public IExpression Operand { get; }
    
    public UnaryExpression(UnaryOperator op, IExpression operand, Position start, Position end) : base(start, end)
    {
        Operator = op;
        Operand = operand;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitUnaryExpression(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitUnaryExpression(this);
}

public enum UnaryOperator
{
    Plus,       // +
    Minus,      // -
    Not,        // !
    Complement, // ^
    Dereference,// *
    Address,    // &
    Receive     // <-
}