namespace IronGo.AST;

/// <summary>
/// Represents an empty statement
/// </summary>
public class EmptyStatement : GoNodeBase, IStatement
{
    public EmptyStatement(Position start, Position end) : base(start, end)
    {
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitEmptyStatement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitEmptyStatement(this);
}