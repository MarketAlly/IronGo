using System.Collections.Generic;

namespace IronGo.AST;

/// <summary>
/// Represents a block statement (e.g., { ... })
/// </summary>
public class BlockStatement : GoNodeBase, IStatement
{
    public IReadOnlyList<IStatement> Statements { get; }
    
    public BlockStatement(IReadOnlyList<IStatement> statements, Position start, Position end) : base(start, end)
    {
        Statements = statements;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitBlockStatement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitBlockStatement(this);
}