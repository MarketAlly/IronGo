namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a short variable declaration (x := expr)
/// </summary>
public class ShortVariableDeclaration : GoNodeBase, IStatement
{
    public IReadOnlyList<string> Names { get; }
    public IReadOnlyList<IExpression> Values { get; }
    
    public ShortVariableDeclaration(IReadOnlyList<string> names, IReadOnlyList<IExpression> values, Position start, Position end) : base(start, end)
    {
        Names = names;
        Values = values;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitShortVariableDeclaration(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitShortVariableDeclaration(this);
}