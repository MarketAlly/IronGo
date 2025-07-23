namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a for-range statement
/// </summary>
public class ForRangeStatement : GoNodeBase, IStatement
{
    public string? Key { get; }
    public string? Value { get; }
    public bool IsShortDeclaration { get; }
    public IExpression Range { get; }
    public IStatement Body { get; }
    
    public ForRangeStatement(string? key, string? value, bool isShortDeclaration, IExpression range, IStatement body, Position start, Position end) : base(start, end)
    {
        Key = key;
        Value = value;
        IsShortDeclaration = isShortDeclaration;
        Range = range;
        Body = body;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitForRangeStatement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitForRangeStatement(this);
}