using System.Collections.Generic;

namespace IronGo.AST;

/// <summary>
/// Represents a range-based for statement
/// </summary>
public class RangeStatement : GoNodeBase, IStatement
{
    public IReadOnlyList<IExpression>? Key { get; }
    public IReadOnlyList<IExpression>? Value { get; }
    public bool IsDeclaration { get; }
    public IExpression Range { get; }
    public IStatement Body { get; }
    
    public RangeStatement(
        IReadOnlyList<IExpression>? key,
        IReadOnlyList<IExpression>? value,
        bool isDeclaration,
        IExpression range,
        IStatement body,
        Position start,
        Position end) : base(start, end)
    {
        Key = key;
        Value = value;
        IsDeclaration = isDeclaration;
        Range = range;
        Body = body;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitRangeStatement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitRangeStatement(this);
}