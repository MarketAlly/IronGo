using System.Collections.Generic;

namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a function call expression
/// </summary>
public class CallExpression : GoNodeBase, IExpression
{
    public IExpression Function { get; }
    public IReadOnlyList<IExpression> Arguments { get; }
    public IReadOnlyList<IType>? TypeArguments { get; }
    public bool HasEllipsis { get; }
    
    public CallExpression(
        IExpression function,
        IReadOnlyList<IExpression> arguments,
        IReadOnlyList<IType>? typeArguments,
        bool hasEllipsis,
        Position start,
        Position end) : base(start, end)
    {
        Function = function;
        Arguments = arguments;
        TypeArguments = typeArguments;
        HasEllipsis = hasEllipsis;
    }
    
    // Backward compatibility constructor
    public CallExpression(
        IExpression function,
        IReadOnlyList<IExpression> arguments,
        bool hasEllipsis,
        Position start,
        Position end) : this(function, arguments, null, hasEllipsis, start, end)
    {
    }
    
    public bool IsGenericCall => TypeArguments != null && TypeArguments.Count > 0;
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitCallExpression(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitCallExpression(this);
}