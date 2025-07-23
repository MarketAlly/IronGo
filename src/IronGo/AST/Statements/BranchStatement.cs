namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a branch statement (break, continue, goto, fallthrough)
/// </summary>
public class BranchStatement : GoNodeBase, IStatement
{
    public BranchKind Kind { get; }
    public string? Label { get; }
    
    public BranchStatement(BranchKind kind, string? label, Position start, Position end) : base(start, end)
    {
        Kind = kind;
        Label = label;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitBranchStatement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitBranchStatement(this);
}

public enum BranchKind
{
    Break,
    Continue,
    Goto,
    Fallthrough
}