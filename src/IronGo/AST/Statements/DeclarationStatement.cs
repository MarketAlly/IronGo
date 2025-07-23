namespace IronGo.AST;

/// <summary>
/// Represents a declaration used as a statement
/// </summary>
public class DeclarationStatement : GoNodeBase, IStatement
{
    public IDeclaration Declaration { get; }
    
    public DeclarationStatement(IDeclaration declaration, Position start, Position end) : base(start, end)
    {
        Declaration = declaration;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitDeclarationStatement(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitDeclarationStatement(this);
}