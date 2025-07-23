using System;
using System.Collections.Generic;
using System.Linq;
using IronGo.AST;

namespace IronGo.Utilities;

/// <summary>
/// Provides deep cloning functionality for AST nodes
/// </summary>
public class AstCloner : IGoAstVisitor<IGoNode>
{
    private readonly Dictionary<IGoNode, IGoNode> _cloneMap = new();
    
    /// <summary>
    /// Clone any AST node
    /// </summary>
    public static T Clone<T>(T node) where T : IGoNode
    {
        var cloner = new AstCloner();
        return (T)node.Accept(cloner);
    }
    
    public IGoNode VisitSourceFile(SourceFile node)
    {
        if (_cloneMap.TryGetValue(node, out var existing))
            return existing;
            
        var clone = new SourceFile(
            node.Package != null ? (PackageDeclaration)node.Package.Accept(this) : null,
            node.Imports.Select(i => (ImportDeclaration)i.Accept(this)).ToList(),
            node.Declarations.Select(d => (IDeclaration)d.Accept(this)).ToList(),
            node.Comments.Select(c => (Comment)c.Accept(this)).ToList(),
            node.Start,
            node.End
        );
        
        _cloneMap[node] = clone;
        return clone;
    }
    
    public IGoNode VisitPackageDeclaration(PackageDeclaration node)
    {
        return new PackageDeclaration(node.Name, node.Start, node.End);
    }
    
    public IGoNode VisitImportDeclaration(ImportDeclaration node)
    {
        return new ImportDeclaration(
            node.Specs.Select(s => (ImportSpec)s.Accept(this)).ToList(),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitImportSpec(ImportSpec node)
    {
        return new ImportSpec(node.Alias, node.Path, node.Start, node.End);
    }
    
    public IGoNode VisitFunctionDeclaration(FunctionDeclaration node)
    {
        return new FunctionDeclaration(
            node.Name,
            node.TypeParameters?.Select(p => (TypeParameter)p.Accept(this)).ToList(),
            node.Parameters.Select(p => (Parameter)p.Accept(this)).ToList(),
            node.ReturnParameters?.Select(p => (Parameter)p.Accept(this)).ToList(),
            node.Body != null ? (BlockStatement)node.Body.Accept(this) : null,
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitMethodDeclaration(MethodDeclaration node)
    {
        return new MethodDeclaration(
            (Parameter)node.Receiver.Accept(this),
            node.Name,
            node.TypeParameters?.Select(p => (TypeParameter)p.Accept(this)).ToList(),
            node.Parameters.Select(p => (Parameter)p.Accept(this)).ToList(),
            node.ReturnParameters?.Select(p => (Parameter)p.Accept(this)).ToList(),
            node.Body != null ? (BlockStatement)node.Body.Accept(this) : null,
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitFieldDeclaration(FieldDeclaration node)
    {
        return new FieldDeclaration(
            node.Names.ToList(),
            node.Type != null ? (IType)node.Type.Accept(this) : null!,
            node.Tag,
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitTypeDeclaration(TypeDeclaration node)
    {
        return new TypeDeclaration(
            node.Specs.Select(s => (TypeSpec)s.Accept(this)).ToList(),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitConstDeclaration(ConstDeclaration node)
    {
        return new ConstDeclaration(
            node.Specs.Select(s => (ConstSpec)s.Accept(this)).ToList(),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitVariableDeclaration(VariableDeclaration node)
    {
        return new VariableDeclaration(
            node.IsConstant,
            node.Specs.Select(s => (VariableSpec)s.Accept(this)).ToList(),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitVariableSpec(VariableSpec node)
    {
        return new VariableSpec(
            node.Names.ToList(),
            node.Type != null ? (IType)node.Type.Accept(this) : null,
            node.Values.Select(v => (IExpression)v.Accept(this)).ToList(),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitConstSpec(ConstSpec node)
    {
        return new ConstSpec(
            node.Names.ToList(),
            node.Type != null ? (IType)node.Type.Accept(this) : null,
            node.Values.Select(v => (IExpression)v.Accept(this)).ToList(),
            node.Start,
            node.End
        );
    }
    
    
    // Statements
    public IGoNode VisitBlockStatement(BlockStatement node)
    {
        return new BlockStatement(
            node.Statements.Select(s => (IStatement)s.Accept(this)).ToList(),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitExpressionStatement(ExpressionStatement node)
    {
        return new ExpressionStatement(
            (IExpression)node.Expression.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitSendStatement(SendStatement node)
    {
        return new SendStatement(
            (IExpression)node.Channel.Accept(this),
            (IExpression)node.Value.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitIncDecStatement(IncDecStatement node)
    {
        return new IncDecStatement(
            (IExpression)node.Expression.Accept(this),
            node.IsIncrement,
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitAssignmentStatement(AssignmentStatement node)
    {
        return new AssignmentStatement(
            node.Left.Select(e => (IExpression)e.Accept(this)).ToList(),
            node.Operator,
            node.Right.Select(e => (IExpression)e.Accept(this)).ToList(),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitShortVariableDeclaration(ShortVariableDeclaration node)
    {
        return new ShortVariableDeclaration(
            node.Names.ToList(),
            node.Values.Select(v => (IExpression)v.Accept(this)).ToList(),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitIfStatement(IfStatement node)
    {
        return new IfStatement(
            node.Init != null ? (IStatement)node.Init.Accept(this) : null,
            (IExpression)node.Condition.Accept(this),
            (IStatement)node.Then.Accept(this),
            node.Else != null ? (IStatement)node.Else.Accept(this) : null,
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitSwitchStatement(SwitchStatement node)
    {
        return new SwitchStatement(
            node.Init != null ? (IStatement)node.Init.Accept(this) : null,
            node.Tag != null ? (IExpression)node.Tag.Accept(this) : null,
            node.Cases.Select(c => (CaseClause)c.Accept(this)).ToList(),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitCaseClause(CaseClause node)
    {
        return new CaseClause(
            node.Expressions?.Select(e => (IExpression)e.Accept(this)).ToList(),
            node.Body.Select(s => (IStatement)s.Accept(this)).ToList(),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitTypeSwitchStatement(TypeSwitchStatement node)
    {
        return new TypeSwitchStatement(
            node.Init != null ? (IStatement)node.Init.Accept(this) : null,
            node.Assign != null ? (IStatement)node.Assign.Accept(this) : null,
            node.Cases.Select(c => (TypeCaseClause)c.Accept(this)).ToList(),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitTypeCaseClause(TypeCaseClause node)
    {
        return new TypeCaseClause(
            node.Types.Select(t => (IType)t.Accept(this)).ToList(),
            node.Statements.Select(s => (IStatement)s.Accept(this)).ToList(),
            node.IsDefault,
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitSelectStatement(SelectStatement node)
    {
        return new SelectStatement(
            node.Cases.Select(c => (CommClause)c.Accept(this)).ToList(),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitCommClause(CommClause node)
    {
        return new CommClause(
            node.Comm != null ? (IStatement)node.Comm.Accept(this) : null,
            node.Statements.Select(s => (IStatement)s.Accept(this)).ToList(),
            node.IsDefault,
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitForStatement(ForStatement node)
    {
        return new ForStatement(
            node.Init != null ? (IStatement)node.Init.Accept(this) : null,
            node.Condition != null ? (IExpression)node.Condition.Accept(this) : null,
            node.Post != null ? (IStatement)node.Post.Accept(this) : null,
            (IStatement)node.Body.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitForRangeStatement(ForRangeStatement node)
    {
        return new ForRangeStatement(
            node.Key,
            node.Value,
            node.IsShortDeclaration,
            (IExpression)node.Range.Accept(this),
            (IStatement)node.Body.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitReturnStatement(ReturnStatement node)
    {
        return new ReturnStatement(
            node.Results.Select(r => (IExpression)r.Accept(this)).ToList(),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitBranchStatement(BranchStatement node)
    {
        return new BranchStatement(node.Kind, node.Label, node.Start, node.End);
    }
    
    public IGoNode VisitLabeledStatement(LabeledStatement node)
    {
        return new LabeledStatement(
            node.Label,
            (IStatement)node.Statement.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitGoStatement(GoStatement node)
    {
        return new GoStatement(
            (CallExpression)node.Call.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitDeferStatement(DeferStatement node)
    {
        return new DeferStatement(
            (CallExpression)node.Call.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitFallthroughStatement(FallthroughStatement node)
    {
        return new FallthroughStatement(node.Start, node.End);
    }
    
    public IGoNode VisitDeclarationStatement(DeclarationStatement node)
    {
        return new DeclarationStatement(
            (IDeclaration)node.Declaration.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitEmptyStatement(EmptyStatement node)
    {
        return new EmptyStatement(node.Start, node.End);
    }
    
    // Expressions
    public IGoNode VisitIdentifierExpression(IdentifierExpression node)
    {
        return new IdentifierExpression(node.Name, node.Start, node.End);
    }
    
    public IGoNode VisitLiteralExpression(LiteralExpression node)
    {
        return new LiteralExpression(node.Kind, node.Value, node.Start, node.End);
    }
    
    public IGoNode VisitFunctionLiteral(FunctionLiteral node)
    {
        return new FunctionLiteral(
            node.Type != null ? (FunctionType)node.Type.Accept(this) : null,
            node.Parameters.Select(p => (Parameter)p.Accept(this)).ToList(),
            node.ReturnParameters?.Select(p => (Parameter)p.Accept(this)).ToList(),
            (BlockStatement)node.Body.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitCompositeLiteral(CompositeLiteral node)
    {
        return new CompositeLiteral(
            node.Type != null ? (IType)node.Type.Accept(this) : null,
            node.Elements.Select(e => (IExpression)e.Accept(this)).ToList(),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitKeyedElement(KeyedElement node)
    {
        return new KeyedElement(
            node.Key != null ? (IExpression)node.Key.Accept(this) : null,
            (IExpression)node.Value.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitBinaryExpression(BinaryExpression node)
    {
        return new BinaryExpression(
            (IExpression)node.Left.Accept(this),
            node.Operator,
            (IExpression)node.Right.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitUnaryExpression(UnaryExpression node)
    {
        return new UnaryExpression(
            node.Operator,
            (IExpression)node.Operand.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitCallExpression(CallExpression node)
    {
        return new CallExpression(
            (IExpression)node.Function.Accept(this),
            node.Arguments.Select(a => (IExpression)a.Accept(this)).ToList(),
            node.TypeArguments?.Select(t => (IType)t.Accept(this)).ToList(),
            node.HasEllipsis,
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitIndexExpression(IndexExpression node)
    {
        return new IndexExpression(
            (IExpression)node.X.Accept(this),
            (IExpression)node.Index.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitSliceExpression(SliceExpression node)
    {
        return new SliceExpression(
            (IExpression)node.X.Accept(this),
            node.Low != null ? (IExpression)node.Low.Accept(this) : null,
            node.High != null ? (IExpression)node.High.Accept(this) : null,
            node.Max != null ? (IExpression)node.Max.Accept(this) : null,
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitSelectorExpression(SelectorExpression node)
    {
        return new SelectorExpression(
            (IExpression)node.X.Accept(this),
            node.Selector,
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitTypeAssertionExpression(TypeAssertionExpression node)
    {
        return new TypeAssertionExpression(
            (IExpression)node.X.Accept(this),
            node.Type != null ? (IType)node.Type.Accept(this) : null,
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitParenthesizedExpression(ParenthesizedExpression node)
    {
        return new ParenthesizedExpression(
            (IExpression)node.Expression.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitConversionExpression(ConversionExpression node)
    {
        return new ConversionExpression(
            (IType)node.Type.Accept(this),
            (IExpression)node.Expression.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitEllipsisExpression(EllipsisExpression node)
    {
        return new EllipsisExpression(
            node.Type != null ? (IType)node.Type.Accept(this) : null,
            node.Start,
            node.End
        );
    }
    
    // Types
    public IGoNode VisitIdentifierType(IdentifierType node)
    {
        return new IdentifierType(
            node.Name,
            node.Package,
            node.TypeArguments?.Select(t => (IType)t.Accept(this)).ToList(),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitArrayType(ArrayType node)
    {
        return new ArrayType(
            (IExpression)node.Length.Accept(this),
            (IType)node.ElementType.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitSliceType(SliceType node)
    {
        return new SliceType(
            (IType)node.ElementType.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitStructType(StructType node)
    {
        return new StructType(
            node.Fields.Select(f => (FieldDeclaration)f.Accept(this)).ToList(),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitPointerType(PointerType node)
    {
        return new PointerType(
            (IType)node.ElementType.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitFunctionType(FunctionType node)
    {
        return new FunctionType(
            node.TypeParameters?.Select(tp => (TypeParameter)tp.Accept(this)).ToList(),
            node.Parameters.Select(p => (Parameter)p.Accept(this)).ToList(),
            node.ReturnParameters?.Select(p => (Parameter)p.Accept(this)).ToList(),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitInterfaceType(InterfaceType node)
    {
        return new InterfaceType(
            node.Methods.Select(m => (IDeclaration)m.Accept(this)).ToList(),
            node.TypeElements.Select(e => (TypeElement)e.Accept(this)).ToList(),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitMapType(MapType node)
    {
        return new MapType(
            (IType)node.KeyType.Accept(this),
            (IType)node.ValueType.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitChannelType(ChannelType node)
    {
        return new ChannelType(
            node.Direction,
            (IType)node.ElementType.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitTypeParameter(TypeParameter node)
    {
        return new TypeParameter(
            node.Name,
            node.Constraint != null ? (IType)node.Constraint.Accept(this) : null,
            node.Start,
            node.End
        );
    }
    
    // Additional required methods
    public IGoNode VisitParameter(Parameter node)
    {
        return new Parameter(
            node.Names.ToList(),
            (IType)node.Type.Accept(this),
            node.IsVariadic,
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitTypeSpec(TypeSpec node)
    {
        return new TypeSpec(
            node.Name,
            node.TypeParameters?.Select(p => (TypeParameter)p.Accept(this)).ToList(),
            (IType)node.Type.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitInterfaceMethod(InterfaceMethod node)
    {
        return new InterfaceMethod(
            node.Name,
            (FunctionType)node.Signature.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitInterfaceEmbedding(InterfaceEmbedding node)
    {
        return new InterfaceEmbedding(
            (IType)node.Type.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitRangeStatement(RangeStatement node)
    {
        return new RangeStatement(
            node.Key?.Select(k => (IExpression)k.Accept(this)).ToList(),
            node.Value?.Select(v => (IExpression)v.Accept(this)).ToList(),
            node.IsDeclaration,
            (IExpression)node.Range.Accept(this),
            (IStatement)node.Body.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitCompositeLiteralElement(CompositeLiteralElement node)
    {
        return new CompositeLiteralElement(
            node.Key != null ? (IExpression)node.Key.Accept(this) : null,
            (IExpression)node.Value.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitComment(Comment node)
    {
        return new Comment(
            node.Text,
            node.IsLineComment,
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitTypeInstantiation(TypeInstantiation node)
    {
        return new TypeInstantiation(
            (IType)node.BaseType.Accept(this),
            node.TypeArguments.Select(t => (IType)t.Accept(this)).ToList(),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitTypeUnion(TypeUnion node)
    {
        return new TypeUnion(
            node.Terms.Select(t => (TypeTerm)t.Accept(this)).ToList(),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitTypeTerm(TypeTerm node)
    {
        return new TypeTerm(
            node.IsUnderlying,
            (IType)node.Type.Accept(this),
            node.Start,
            node.End
        );
    }
    
    public IGoNode VisitTypeElement(TypeElement node)
    {
        return new TypeElement(
            (IType)node.Type.Accept(this),
            node.Start,
            node.End
        );
    }
}