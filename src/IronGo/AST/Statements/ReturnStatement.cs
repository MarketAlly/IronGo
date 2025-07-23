using System.Collections.Generic;

namespace IronGo.AST;

/// <summary>
/// Represents a return statement
/// </summary>
public class ReturnStatement : GoNodeBase, IStatement
{
    public IReadOnlyList<IExpression> Results { get; }
    
    public ReturnStatement(IReadOnlyList<IExpression> results, Position start, Position end) : base(start, end)
    {
        Results = results;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitReturnStatement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitReturnStatement(this);
}