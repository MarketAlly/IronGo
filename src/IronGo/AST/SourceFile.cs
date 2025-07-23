using System.Collections.Generic;

namespace IronGo.AST;

/// <summary>
/// Represents a complete Go source file
/// </summary>
public class SourceFile : GoNodeBase
{
    public PackageDeclaration? Package { get; }
    public IReadOnlyList<ImportDeclaration> Imports { get; }
    public IReadOnlyList<IDeclaration> Declarations { get; }
    public IReadOnlyList<Comment> Comments { get; }
    
    public SourceFile(
        PackageDeclaration? package,
        IReadOnlyList<ImportDeclaration> imports,
        IReadOnlyList<IDeclaration> declarations,
        IReadOnlyList<Comment> comments,
        Position start,
        Position end) : base(start, end)
    {
        Package = package;
        Imports = imports;
        Declarations = declarations;
        Comments = comments;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitSourceFile(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitSourceFile(this);
}