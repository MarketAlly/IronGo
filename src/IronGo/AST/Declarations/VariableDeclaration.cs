using System.Collections.Generic;

namespace IronGo.AST;

/// <summary>
/// Represents a variable declaration (var or const)
/// </summary>
public class VariableDeclaration : GoNodeBase, IDeclaration
{
    public bool IsConstant { get; }
    public IReadOnlyList<VariableSpec> Specs { get; }
    public string? Name => Specs.Count == 1 && Specs[0].Names.Count == 1 ? Specs[0].Names[0] : null;
    
    public VariableDeclaration(bool isConstant, IReadOnlyList<VariableSpec> specs, Position start, Position end) : base(start, end)
    {
        IsConstant = isConstant;
        Specs = specs;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitVariableDeclaration(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitVariableDeclaration(this);
}

/// <summary>
/// Represents a single variable specification
/// </summary>
public class VariableSpec : GoNodeBase
{
    public IReadOnlyList<string> Names { get; }
    public IType? Type { get; }
    public IReadOnlyList<IExpression>? Values { get; }
    
    public VariableSpec(IReadOnlyList<string> names, IType? type, IReadOnlyList<IExpression>? values, Position start, Position end) : base(start, end)
    {
        Names = names;
        Type = type;
        Values = values;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitVariableSpec(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitVariableSpec(this);
}