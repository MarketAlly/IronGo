namespace IronGo.AST;

/// <summary>
/// Represents a const declaration block
/// </summary>
public class ConstDeclaration : GoNodeBase, IDeclaration
{
    public IReadOnlyList<ConstSpec> Specs { get; }
    public string? Name => null; // Const declarations don't have a single name
    
    public ConstDeclaration(IReadOnlyList<ConstSpec> specs, Position start, Position end) : base(start, end)
    {
        Specs = specs;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitConstDeclaration(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitConstDeclaration(this);
}

/// <summary>
/// Represents a single const specification within a const declaration
/// </summary>
public class ConstSpec : GoNodeBase
{
    public IReadOnlyList<string> Names { get; }
    public IType? Type { get; }
    public IReadOnlyList<IExpression> Values { get; }
    
    public ConstSpec(IReadOnlyList<string> names, IType? type, IReadOnlyList<IExpression> values, Position start, Position end) : base(start, end)
    {
        Names = names;
        Type = type;
        Values = values;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitConstSpec(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitConstSpec(this);
}