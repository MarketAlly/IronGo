using System.Collections.Generic;

namespace IronGo.AST;

/// <summary>
/// Represents an assignment statement (=, :=, +=, etc.)
/// </summary>
public class AssignmentStatement : GoNodeBase, IStatement
{
    public IReadOnlyList<IExpression> Left { get; }
    public AssignmentOperator Operator { get; }
    public IReadOnlyList<IExpression> Right { get; }
    
    public AssignmentStatement(
        IReadOnlyList<IExpression> left,
        AssignmentOperator op,
        IReadOnlyList<IExpression> right,
        Position start,
        Position end) : base(start, end)
    {
        Left = left;
        Operator = op;
        Right = right;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitAssignmentStatement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitAssignmentStatement(this);
}

public enum AssignmentOperator
{
    Assign,         // =
    DeclareAssign,  // :=
    AddAssign,      // +=
    SubAssign,      // -=
    MulAssign,      // *=
    DivAssign,      // /=
    ModAssign,      // %=
    AndAssign,      // &=
    OrAssign,       // |=
    XorAssign,      // ^=
    ShlAssign,      // <<=
    ShrAssign,      // >>=
    AndNotAssign    // &^=
}