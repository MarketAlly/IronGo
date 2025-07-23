using System;
using System.Collections.Generic;
using System.Linq;
using IronGo.AST;

namespace IronGo.Utilities;

/// <summary>
/// Utility methods for working with Go AST
/// </summary>
public static class AstUtilities
{
    /// <summary>
    /// Find all nodes of a specific type in the AST
    /// </summary>
    public static IEnumerable<T> FindNodes<T>(this IGoNode root) where T : IGoNode
    {
        var finder = new NodeFinder<T>();
        root.Accept(finder);
        return finder.FoundNodes;
    }
    
    /// <summary>
    /// Find the first node of a specific type in the AST
    /// </summary>
    public static T? FindFirstNode<T>(this IGoNode root) where T : IGoNode
    {
        return root.FindNodes<T>().FirstOrDefault();
    }
    
    /// <summary>
    /// Find all function declarations in the source file
    /// </summary>
    public static IEnumerable<FunctionDeclaration> GetFunctions(this SourceFile sourceFile)
    {
        return sourceFile.Declarations.OfType<FunctionDeclaration>()
            .Where(f => f.GetType() == typeof(FunctionDeclaration));
    }
    
    /// <summary>
    /// Find all method declarations in the source file
    /// </summary>
    public static IEnumerable<MethodDeclaration> GetMethods(this SourceFile sourceFile)
    {
        return sourceFile.Declarations.OfType<MethodDeclaration>();
    }
    
    /// <summary>
    /// Find all type declarations in the source file
    /// </summary>
    public static IEnumerable<TypeDeclaration> GetTypes(this SourceFile sourceFile)
    {
        return sourceFile.Declarations.OfType<TypeDeclaration>();
    }
    
    /// <summary>
    /// Find a function by name
    /// </summary>
    public static FunctionDeclaration? FindFunction(this SourceFile sourceFile, string name)
    {
        return sourceFile.GetFunctions().FirstOrDefault(f => f.Name == name);
    }
    
    /// <summary>
    /// Get all imported packages
    /// </summary>
    public static IEnumerable<string> GetImportedPackages(this SourceFile sourceFile)
    {
        return sourceFile.Imports
            .SelectMany(i => i.Specs)
            .Select(s => s.Path)
            .Distinct();
    }
    
    /// <summary>
    /// Check if a package is imported
    /// </summary>
    public static bool IsPackageImported(this SourceFile sourceFile, string packagePath)
    {
        return sourceFile.GetImportedPackages().Contains(packagePath);
    }
    
    /// <summary>
    /// Get the import alias for a package
    /// </summary>
    public static string? GetImportAlias(this SourceFile sourceFile, string packagePath)
    {
        return sourceFile.Imports
            .SelectMany(i => i.Specs)
            .FirstOrDefault(s => s.Path == packagePath)?.Alias;
    }
    
    /// <summary>
    /// Find all identifiers in the AST
    /// </summary>
    public static IEnumerable<IdentifierExpression> GetAllIdentifiers(this IGoNode root)
    {
        var identifiers = new List<IdentifierExpression>();
        identifiers.AddRange(root.FindNodes<IdentifierExpression>());
        
        // Also find identifiers in selector expressions
        var selectors = root.FindNodes<SelectorExpression>();
        foreach (var selector in selectors)
        {
            // Create a synthetic IdentifierExpression for the selector
            identifiers.Add(new IdentifierExpression(selector.Selector, selector.Start, selector.End));
        }
        
        return identifiers;
    }
    
    /// <summary>
    /// Find all function calls in the AST
    /// </summary>
    public static IEnumerable<CallExpression> GetAllCalls(this IGoNode root)
    {
        return root.FindNodes<CallExpression>();
    }
    
    /// <summary>
    /// Find all literal values of a specific kind
    /// </summary>
    public static IEnumerable<LiteralExpression> GetLiterals(this IGoNode root, LiteralKind kind)
    {
        return root.FindNodes<LiteralExpression>().Where(l => l.Kind == kind);
    }
    
    /// <summary>
    /// Get the depth of a node in the AST
    /// </summary>
    public static int GetDepth(this IGoNode node, IGoNode root)
    {
        var pathFinder = new PathFinder(node);
        root.Accept(pathFinder);
        return pathFinder.Path?.Count ?? -1;
    }
    
    /// <summary>
    /// Get the parent of a node
    /// </summary>
    public static IGoNode? GetParent(this IGoNode node, IGoNode root)
    {
        var parentFinder = new ParentFinder(node);
        root.Accept(parentFinder);
        return parentFinder.Parent;
    }
    
    /// <summary>
    /// Count all nodes in the AST
    /// </summary>
    public static int CountNodes(this IGoNode root)
    {
        var counter = new NodeCounter();
        root.Accept(counter);
        return counter.Count;
    }
    
    /// <summary>
    /// Get all nodes at a specific position
    /// </summary>
    public static IEnumerable<IGoNode> GetNodesAtPosition(this IGoNode root, Position position)
    {
        var finder = new PositionNodeFinder(position);
        root.Accept(finder);
        return finder.FoundNodes;
    }
    
    /// <summary>
    /// Clone an AST node (deep copy)
    /// </summary>
    public static T Clone<T>(this T node) where T : IGoNode
    {
        return AstCloner.Clone(node);
    }
    
    private class NodeFinder<T> : UniversalNodeVisitor where T : IGoNode
    {
        public List<T> FoundNodes { get; } = new();
        
        protected override void ProcessNode(IGoNode node)
        {
            if (node is T t) 
                FoundNodes.Add(t);
        }
    }
    
    private class NodeCounter : UniversalNodeVisitor
    {
        public int Count { get; private set; }
        
        protected override void ProcessNode(IGoNode node)
        {
            Count++;
        }
    }
    
    private class PathFinder : GoAstWalker
    {
        private readonly IGoNode _target;
        private readonly Stack<IGoNode> _currentPath = new();
        private bool _found;
        
        public List<IGoNode>? Path { get; private set; }
        
        public PathFinder(IGoNode target)
        {
            _target = target;
        }
        
        private void EnterNode(IGoNode node)
        {
            if (_found) return;
            
            _currentPath.Push(node);
            
            if (ReferenceEquals(node, _target))
            {
                _found = true;
                Path = _currentPath.Reverse().ToList();
            }
        }
        
        private void ExitNode()
        {
            if (!_found && _currentPath.Count > 0)
                _currentPath.Pop();
        }
        
        public override void VisitSourceFile(SourceFile node) { EnterNode(node); base.VisitSourceFile(node); ExitNode(); }
        public override void VisitPackageDeclaration(PackageDeclaration node) { EnterNode(node); base.VisitPackageDeclaration(node); ExitNode(); }
        public override void VisitImportDeclaration(ImportDeclaration node) { EnterNode(node); base.VisitImportDeclaration(node); ExitNode(); }
        public override void VisitFunctionDeclaration(FunctionDeclaration node) { EnterNode(node); base.VisitFunctionDeclaration(node); ExitNode(); }
        public override void VisitMethodDeclaration(MethodDeclaration node) { EnterNode(node); base.VisitMethodDeclaration(node); ExitNode(); }
        public override void VisitTypeDeclaration(TypeDeclaration node) { EnterNode(node); base.VisitTypeDeclaration(node); ExitNode(); }
        public override void VisitBlockStatement(BlockStatement node) { EnterNode(node); base.VisitBlockStatement(node); ExitNode(); }
        public override void VisitIfStatement(IfStatement node) { EnterNode(node); base.VisitIfStatement(node); ExitNode(); }
        public override void VisitForStatement(ForStatement node) { EnterNode(node); base.VisitForStatement(node); ExitNode(); }
        public override void VisitBinaryExpression(BinaryExpression node) { EnterNode(node); base.VisitBinaryExpression(node); ExitNode(); }
        public override void VisitCallExpression(CallExpression node) { EnterNode(node); base.VisitCallExpression(node); ExitNode(); }
        public override void VisitIdentifierExpression(IdentifierExpression node) { EnterNode(node); base.VisitIdentifierExpression(node); ExitNode(); }
    }
    
    private class ParentFinder : GoAstWalker
    {
        private readonly IGoNode _target;
        private readonly Stack<IGoNode> _parentStack = new();
        private bool _found;
        
        public IGoNode? Parent { get; private set; }
        
        public ParentFinder(IGoNode target)
        {
            _target = target;
        }
        
        private void EnterNode(IGoNode node)
        {
            if (_found) return;
            
            if (ReferenceEquals(node, _target) && _parentStack.Count > 0)
            {
                Parent = _parentStack.Peek();
                _found = true;
                return;
            }
            
            _parentStack.Push(node);
        }
        
        private void ExitNode()
        {
            if (!_found && _parentStack.Count > 0)
                _parentStack.Pop();
        }
        
        public override void VisitSourceFile(SourceFile node) { EnterNode(node); base.VisitSourceFile(node); ExitNode(); }
        public override void VisitPackageDeclaration(PackageDeclaration node) { EnterNode(node); base.VisitPackageDeclaration(node); ExitNode(); }
        public override void VisitImportDeclaration(ImportDeclaration node) { EnterNode(node); base.VisitImportDeclaration(node); ExitNode(); }
        public override void VisitFunctionDeclaration(FunctionDeclaration node) { EnterNode(node); base.VisitFunctionDeclaration(node); ExitNode(); }
        public override void VisitMethodDeclaration(MethodDeclaration node) { EnterNode(node); base.VisitMethodDeclaration(node); ExitNode(); }
        public override void VisitTypeDeclaration(TypeDeclaration node) { EnterNode(node); base.VisitTypeDeclaration(node); ExitNode(); }
        public override void VisitBlockStatement(BlockStatement node) { EnterNode(node); base.VisitBlockStatement(node); ExitNode(); }
        public override void VisitIfStatement(IfStatement node) { EnterNode(node); base.VisitIfStatement(node); ExitNode(); }
        public override void VisitForStatement(ForStatement node) { EnterNode(node); base.VisitForStatement(node); ExitNode(); }
        public override void VisitBinaryExpression(BinaryExpression node) { EnterNode(node); base.VisitBinaryExpression(node); ExitNode(); }
        public override void VisitCallExpression(CallExpression node) { EnterNode(node); base.VisitCallExpression(node); ExitNode(); }
        public override void VisitIdentifierExpression(IdentifierExpression node) { EnterNode(node); base.VisitIdentifierExpression(node); ExitNode(); }
    }
    
    private class PositionNodeFinder : GoAstWalker
    {
        private readonly Position _position;
        public List<IGoNode> FoundNodes { get; } = new();
        
        public PositionNodeFinder(Position position)
        {
            _position = position;
        }
        
        private void CheckNode(IGoNode node)
        {
            if (node.Start.Offset <= _position.Offset && node.End.Offset >= _position.Offset)
            {
                FoundNodes.Add(node);
            }
        }
        
        public override void VisitSourceFile(SourceFile node)
        {
            CheckNode(node);
            base.VisitSourceFile(node);
        }
        
        public override void VisitPackageDeclaration(PackageDeclaration node) { CheckNode(node); base.VisitPackageDeclaration(node); }
        public override void VisitImportDeclaration(ImportDeclaration node) { CheckNode(node); base.VisitImportDeclaration(node); }
        public override void VisitFunctionDeclaration(FunctionDeclaration node) { CheckNode(node); base.VisitFunctionDeclaration(node); }
        public override void VisitMethodDeclaration(MethodDeclaration node) { CheckNode(node); base.VisitMethodDeclaration(node); }
        public override void VisitTypeDeclaration(TypeDeclaration node) { CheckNode(node); base.VisitTypeDeclaration(node); }
        public override void VisitBlockStatement(BlockStatement node) { CheckNode(node); base.VisitBlockStatement(node); }
        public override void VisitIfStatement(IfStatement node) { CheckNode(node); base.VisitIfStatement(node); }
        public override void VisitForStatement(ForStatement node) { CheckNode(node); base.VisitForStatement(node); }
        public override void VisitSwitchStatement(SwitchStatement node) { CheckNode(node); base.VisitSwitchStatement(node); }
        public override void VisitReturnStatement(ReturnStatement node) { CheckNode(node); base.VisitReturnStatement(node); }
        public override void VisitExpressionStatement(ExpressionStatement node) { CheckNode(node); base.VisitExpressionStatement(node); }
        public override void VisitBinaryExpression(BinaryExpression node) { CheckNode(node); base.VisitBinaryExpression(node); }
        public override void VisitUnaryExpression(UnaryExpression node) { CheckNode(node); base.VisitUnaryExpression(node); }
        public override void VisitCallExpression(CallExpression node) { CheckNode(node); base.VisitCallExpression(node); }
        public override void VisitIdentifierExpression(IdentifierExpression node) { CheckNode(node); base.VisitIdentifierExpression(node); }
        public override void VisitLiteralExpression(LiteralExpression node) { CheckNode(node); base.VisitLiteralExpression(node); }
        public override void VisitShortVariableDeclaration(ShortVariableDeclaration node) { CheckNode(node); base.VisitShortVariableDeclaration(node); }
    }
}