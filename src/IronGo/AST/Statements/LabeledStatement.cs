namespace IronGo.AST;

/// <summary>
/// Represents a labeled statement
/// </summary>
public class LabeledStatement : GoNodeBase, IStatement
{
    public string Label { get; }
    public IStatement Statement { get; }
    
    public LabeledStatement(string label, IStatement statement, Position start, Position end) : base(start, end)
    {
        Label = label;
        Statement = statement;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitLabeledStatement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitLabeledStatement(this);
}