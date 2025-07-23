namespace MarketAlly.IronGo.AST;

/// <summary>
/// Represents a type switch statement
/// </summary>
public class TypeSwitchStatement : GoNodeBase, IStatement
{
    public IStatement? Init { get; }
    public IStatement? Assign { get; }
    public IReadOnlyList<TypeCaseClause> Cases { get; }
    
    public TypeSwitchStatement(IStatement? init, IStatement? assign, IReadOnlyList<TypeCaseClause> cases, Position start, Position end) : base(start, end)
    {
        Init = init;
        Assign = assign;
        Cases = cases;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitTypeSwitchStatement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitTypeSwitchStatement(this);
}

/// <summary>
/// Represents a case clause in a type switch
/// </summary>
public class TypeCaseClause : GoNodeBase
{
    public IReadOnlyList<IType> Types { get; }
    public IReadOnlyList<IStatement> Statements { get; }
    public bool IsDefault { get; }
    
    public TypeCaseClause(IReadOnlyList<IType> types, IReadOnlyList<IStatement> statements, bool isDefault, Position start, Position end) : base(start, end)
    {
        Types = types;
        Statements = statements;
        IsDefault = isDefault;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitTypeCaseClause(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitTypeCaseClause(this);
}