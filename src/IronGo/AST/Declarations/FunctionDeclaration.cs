using System.Collections.Generic;

namespace IronGo.AST;

/// <summary>
/// Represents a function declaration
/// </summary>
public class FunctionDeclaration : GoNodeBase, IDeclaration
{
    public string Name { get; }
    public IReadOnlyList<TypeParameter>? TypeParameters { get; }
    public IReadOnlyList<Parameter> Parameters { get; }
    public IReadOnlyList<Parameter>? ReturnParameters { get; }
    public BlockStatement? Body { get; }
    
    public FunctionDeclaration(
        string name,
        IReadOnlyList<TypeParameter>? typeParameters,
        IReadOnlyList<Parameter> parameters,
        IReadOnlyList<Parameter>? returnParameters,
        BlockStatement? body,
        Position start,
        Position end) : base(start, end)
    {
        Name = name;
        TypeParameters = typeParameters;
        Parameters = parameters;
        ReturnParameters = returnParameters;
        Body = body;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitFunctionDeclaration(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitFunctionDeclaration(this);
}

/// <summary>
/// Represents a method declaration (function with receiver)
/// </summary>
public class MethodDeclaration : FunctionDeclaration
{
    public Parameter Receiver { get; }
    
    public MethodDeclaration(
        Parameter receiver,
        string name,
        IReadOnlyList<TypeParameter>? typeParameters,
        IReadOnlyList<Parameter> parameters,
        IReadOnlyList<Parameter>? returnParameters,
        BlockStatement? body,
        Position start,
        Position end) : base(name, typeParameters, parameters, returnParameters, body, start, end)
    {
        Receiver = receiver;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitMethodDeclaration(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitMethodDeclaration(this);
}

/// <summary>
/// Represents a function parameter
/// </summary>
public class Parameter : GoNodeBase
{
    public IReadOnlyList<string> Names { get; }
    public IType Type { get; }
    public bool IsVariadic { get; }
    
    public Parameter(IReadOnlyList<string> names, IType type, bool isVariadic, Position start, Position end) : base(start, end)
    {
        Names = names;
        Type = type;
        IsVariadic = isVariadic;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitParameter(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitParameter(this);
}

/// <summary>
/// Represents a type parameter for generic functions/types
/// </summary>
public class TypeParameter : GoNodeBase
{
    public string Name { get; }
    public IType? Constraint { get; }
    
    public TypeParameter(string name, IType? constraint, Position start, Position end) : base(start, end)
    {
        Name = name;
        Constraint = constraint;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitTypeParameter(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitTypeParameter(this);
}