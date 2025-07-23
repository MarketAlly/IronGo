using System.Collections.Generic;

namespace IronGo.AST;

/// <summary>
/// Represents an import declaration
/// </summary>
public class ImportDeclaration : GoNodeBase, IDeclaration
{
    public IReadOnlyList<ImportSpec> Specs { get; }
    public string? Name => null; // Import declarations don't have a single name
    
    public ImportDeclaration(IReadOnlyList<ImportSpec> specs, Position start, Position end) : base(start, end)
    {
        Specs = specs;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitImportDeclaration(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitImportDeclaration(this);
}

/// <summary>
/// Represents a single import specification
/// </summary>
public class ImportSpec : GoNodeBase
{
    public string? Alias { get; }
    public string Path { get; }
    
    public ImportSpec(string? alias, string path, Position start, Position end) : base(start, end)
    {
        Alias = alias;
        Path = path;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitImportSpec(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitImportSpec(this);
}