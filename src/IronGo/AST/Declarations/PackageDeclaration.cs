namespace IronGo.AST;

/// <summary>
/// Represents a package declaration (e.g., "package main")
/// </summary>
public class PackageDeclaration : GoNodeBase, IDeclaration
{
    public string Name { get; }
    
    public PackageDeclaration(string name, Position start, Position end) : base(start, end)
    {
        Name = name;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitPackageDeclaration(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitPackageDeclaration(this);
}