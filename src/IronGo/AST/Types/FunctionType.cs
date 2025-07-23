using System.Collections.Generic;

namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a function type (e.g., "func(int, string) error")
/// </summary>
public class FunctionType : GoNodeBase, IType
{
    public IReadOnlyList<TypeParameter>? TypeParameters { get; }
    public IReadOnlyList<Parameter> Parameters { get; }
    public IReadOnlyList<Parameter>? ReturnParameters { get; }
    
    public FunctionType(
        IReadOnlyList<TypeParameter>? typeParameters,
        IReadOnlyList<Parameter> parameters,
        IReadOnlyList<Parameter>? returnParameters,
        Position start,
        Position end) : base(start, end)
    {
        TypeParameters = typeParameters;
        Parameters = parameters;
        ReturnParameters = returnParameters;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitFunctionType(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitFunctionType(this);
}