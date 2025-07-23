using System.Collections.Generic;

namespace IronGo.AST;

/// <summary>
/// Represents a function literal (anonymous function)
/// </summary>
public class FunctionLiteral : GoNodeBase, IExpression
{
    public FunctionType? Type { get; }
    public IReadOnlyList<Parameter> Parameters { get; }
    public IReadOnlyList<Parameter>? ReturnParameters { get; }
    public BlockStatement Body { get; }
    
    public FunctionLiteral(FunctionType? type, IReadOnlyList<Parameter> parameters, IReadOnlyList<Parameter>? returnParameters, BlockStatement body, Position start, Position end) : base(start, end)
    {
        Type = type;
        Parameters = parameters;
        ReturnParameters = returnParameters;
        Body = body;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitFunctionLiteral(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitFunctionLiteral(this);
}