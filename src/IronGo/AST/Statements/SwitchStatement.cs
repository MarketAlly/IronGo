using System.Collections.Generic;

namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a switch statement
/// </summary>
public class SwitchStatement : GoNodeBase, IStatement
{
    public IStatement? Init { get; }
    public IExpression? Tag { get; }
    public IReadOnlyList<CaseClause> Cases { get; }
    
    public SwitchStatement(
        IStatement? init,
        IExpression? tag,
        IReadOnlyList<CaseClause> cases,
        Position start,
        Position end) : base(start, end)
    {
        Init = init;
        Tag = tag;
        Cases = cases;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitSwitchStatement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitSwitchStatement(this);
}

/// <summary>
/// Represents a case clause in a switch statement
/// </summary>
public class CaseClause : GoNodeBase
{
    public IReadOnlyList<IExpression>? Expressions { get; }
    public IReadOnlyList<IStatement> Body { get; }
    public bool IsDefault => Expressions == null;
    
    public CaseClause(
        IReadOnlyList<IExpression>? expressions,
        IReadOnlyList<IStatement> body,
        Position start,
        Position end) : base(start, end)
    {
        Expressions = expressions;
        Body = body;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitCaseClause(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitCaseClause(this);
}