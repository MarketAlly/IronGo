namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a select statement
/// </summary>
public class SelectStatement : GoNodeBase, IStatement
{
    public IReadOnlyList<CommClause> Cases { get; }
    
    public SelectStatement(IReadOnlyList<CommClause> cases, Position start, Position end) : base(start, end)
    {
        Cases = cases;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitSelectStatement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitSelectStatement(this);
}

/// <summary>
/// Represents a communication clause in a select statement
/// </summary>
public class CommClause : GoNodeBase
{
    public IStatement? Comm { get; }
    public IReadOnlyList<IStatement> Statements { get; }
    public bool IsDefault { get; }
    
    public CommClause(IStatement? comm, IReadOnlyList<IStatement> statements, bool isDefault, Position start, Position end) : base(start, end)
    {
        Comm = comm;
        Statements = statements;
        IsDefault = isDefault;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitCommClause(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitCommClause(this);
}