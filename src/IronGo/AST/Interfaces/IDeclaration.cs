namespace IronGo.AST;

/// <summary>
/// Marker interface for all Go declarations
/// </summary>
public interface IDeclaration : IGoNode
{
    /// <summary>
    /// Name of the declared entity
    /// </summary>
    string? Name { get; }
}