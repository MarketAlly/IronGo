using System.Collections.Generic;

namespace IronGo.AST;

/// <summary>
/// A visitor that walks the entire AST tree, visiting all nodes
/// </summary>
public class GoAstWalker : GoAstVisitorBase
{
    // File and declarations
    public override void VisitSourceFile(SourceFile node)
    {
        node.Package?.Accept(this);
        
        foreach (var import in node.Imports)
            import.Accept(this);
            
        foreach (var decl in node.Declarations)
            decl.Accept(this);
            
        foreach (var comment in node.Comments)
            comment.Accept(this);
    }
    
    public override void VisitImportDeclaration(ImportDeclaration node)
    {
        foreach (var spec in node.Specs)
            spec.Accept(this);
    }
    
    public override void VisitFunctionDeclaration(FunctionDeclaration node)
    {
        if (node.TypeParameters != null)
            foreach (var tp in node.TypeParameters)
                tp.Accept(this);
                
        foreach (var param in node.Parameters)
            param.Accept(this);
            
        if (node.ReturnParameters != null)
            foreach (var param in node.ReturnParameters)
                param.Accept(this);
                
        node.Body?.Accept(this);
    }
    
    public override void VisitMethodDeclaration(MethodDeclaration node)
    {
        node.Receiver.Accept(this);
        
        if (node.TypeParameters != null)
            foreach (var tp in node.TypeParameters)
                tp.Accept(this);
                
        foreach (var param in node.Parameters)
            param.Accept(this);
            
        if (node.ReturnParameters != null)
            foreach (var param in node.ReturnParameters)
                param.Accept(this);
                
        node.Body?.Accept(this);
    }
    
    public override void VisitParameter(Parameter node)
    {
        node.Type.Accept(this);
    }
    
    public override void VisitTypeParameter(TypeParameter node)
    {
        node.Constraint?.Accept(this);
    }
    
    public override void VisitTypeDeclaration(TypeDeclaration node)
    {
        foreach (var spec in node.Specs)
            spec.Accept(this);
    }
    
    public override void VisitTypeSpec(TypeSpec node)
    {
        if (node.TypeParameters != null)
            foreach (var tp in node.TypeParameters)
                tp.Accept(this);
                
        node.Type.Accept(this);
    }
    
    public override void VisitVariableDeclaration(VariableDeclaration node)
    {
        foreach (var spec in node.Specs)
            spec.Accept(this);
    }
    
    public override void VisitVariableSpec(VariableSpec node)
    {
        node.Type?.Accept(this);
        
        if (node.Values != null)
            foreach (var value in node.Values)
                value.Accept(this);
    }
    
    // Types
    public override void VisitPointerType(PointerType node)
    {
        node.ElementType.Accept(this);
    }
    
    public override void VisitArrayType(ArrayType node)
    {
        node.Length?.Accept(this);
        node.ElementType.Accept(this);
    }
    
    public override void VisitSliceType(SliceType node)
    {
        node.ElementType.Accept(this);
    }
    
    public override void VisitMapType(MapType node)
    {
        node.KeyType.Accept(this);
        node.ValueType.Accept(this);
    }
    
    public override void VisitChannelType(ChannelType node)
    {
        node.ElementType.Accept(this);
    }
    
    public override void VisitFunctionType(FunctionType node)
    {
        if (node.TypeParameters != null)
            foreach (var tp in node.TypeParameters)
                tp.Accept(this);
                
        foreach (var param in node.Parameters)
            param.Accept(this);
            
        if (node.ReturnParameters != null)
            foreach (var param in node.ReturnParameters)
                param.Accept(this);
    }
    
    public override void VisitInterfaceType(InterfaceType node)
    {
        foreach (var method in node.Methods)
            method.Accept(this);
            
        foreach (var element in node.TypeElements)
            element.Accept(this);
    }
    
    public override void VisitInterfaceMethod(InterfaceMethod node)
    {
        node.Signature.Accept(this);
    }
    
    public override void VisitInterfaceEmbedding(InterfaceEmbedding node)
    {
        node.Type.Accept(this);
    }
    
    public override void VisitStructType(StructType node)
    {
        foreach (var field in node.Fields)
            field.Accept(this);
    }
    
    public override void VisitFieldDeclaration(FieldDeclaration node)
    {
        node.Type.Accept(this);
    }
    
    public override void VisitIdentifierType(IdentifierType node)
    {
        if (node.TypeArguments != null)
            foreach (var arg in node.TypeArguments)
                arg.Accept(this);
    }
    
    public override void VisitTypeInstantiation(TypeInstantiation node)
    {
        node.BaseType.Accept(this);
        foreach (var arg in node.TypeArguments)
            arg.Accept(this);
    }
    
    public override void VisitTypeUnion(TypeUnion node)
    {
        foreach (var term in node.Terms)
            term.Accept(this);
    }
    
    public override void VisitTypeTerm(TypeTerm node)
    {
        node.Type.Accept(this);
    }
    
    public override void VisitTypeElement(TypeElement node)
    {
        node.Type.Accept(this);
    }
    
    // Statements
    public override void VisitBlockStatement(BlockStatement node)
    {
        foreach (var stmt in node.Statements)
            stmt.Accept(this);
    }
    
    public override void VisitExpressionStatement(ExpressionStatement node)
    {
        node.Expression.Accept(this);
    }
    
    public override void VisitAssignmentStatement(AssignmentStatement node)
    {
        foreach (var left in node.Left)
            left.Accept(this);
            
        foreach (var right in node.Right)
            right.Accept(this);
    }
    
    public override void VisitIfStatement(IfStatement node)
    {
        node.Init?.Accept(this);
        node.Condition.Accept(this);
        node.Then.Accept(this);
        node.Else?.Accept(this);
    }
    
    public override void VisitForStatement(ForStatement node)
    {
        node.Init?.Accept(this);
        node.Condition?.Accept(this);
        node.Post?.Accept(this);
        node.Body.Accept(this);
    }
    
    public override void VisitRangeStatement(RangeStatement node)
    {
        if (node.Key != null)
            foreach (var key in node.Key)
                key.Accept(this);
                
        if (node.Value != null)
            foreach (var value in node.Value)
                value.Accept(this);
                
        node.Range.Accept(this);
        node.Body.Accept(this);
    }
    
    public override void VisitSwitchStatement(SwitchStatement node)
    {
        node.Init?.Accept(this);
        node.Tag?.Accept(this);
        
        foreach (var @case in node.Cases)
            @case.Accept(this);
    }
    
    public override void VisitCaseClause(CaseClause node)
    {
        if (node.Expressions != null)
            foreach (var expr in node.Expressions)
                expr.Accept(this);
                
        foreach (var stmt in node.Body)
            stmt.Accept(this);
    }
    
    public override void VisitReturnStatement(ReturnStatement node)
    {
        foreach (var result in node.Results)
            result.Accept(this);
    }
    
    public override void VisitDeferStatement(DeferStatement node)
    {
        node.Call.Accept(this);
    }
    
    public override void VisitGoStatement(GoStatement node)
    {
        node.Call.Accept(this);
    }
    
    // Expressions
    public override void VisitBinaryExpression(BinaryExpression node)
    {
        node.Left.Accept(this);
        node.Right.Accept(this);
    }
    
    public override void VisitUnaryExpression(UnaryExpression node)
    {
        node.Operand.Accept(this);
    }
    
    public override void VisitCallExpression(CallExpression node)
    {
        node.Function.Accept(this);
        
        if (node.TypeArguments != null)
            foreach (var typeArg in node.TypeArguments)
                typeArg.Accept(this);
        
        foreach (var arg in node.Arguments)
            arg.Accept(this);
    }
    
    public override void VisitSelectorExpression(SelectorExpression node)
    {
        node.X.Accept(this);
    }
    
    public override void VisitIndexExpression(IndexExpression node)
    {
        node.X.Accept(this);
        node.Index.Accept(this);
    }
    
    public override void VisitSliceExpression(SliceExpression node)
    {
        node.X.Accept(this);
        node.Low?.Accept(this);
        node.High?.Accept(this);
        node.Max?.Accept(this);
    }
    
    public override void VisitTypeAssertionExpression(TypeAssertionExpression node)
    {
        node.X.Accept(this);
        node.Type?.Accept(this);
    }
    
    public override void VisitCompositeLiteral(CompositeLiteral node)
    {
        node.Type?.Accept(this);
        
        foreach (var element in node.Elements)
            element.Accept(this);
    }
    
    public override void VisitCompositeLiteralElement(CompositeLiteralElement node)
    {
        node.Key?.Accept(this);
        node.Value.Accept(this);
    }
    
    public override void VisitFunctionLiteral(FunctionLiteral node)
    {
        node.Type?.Accept(this);
        foreach (var param in node.Parameters)
            param.Accept(this);
        if (node.ReturnParameters != null)
            foreach (var param in node.ReturnParameters)
                param.Accept(this);
        node.Body.Accept(this);
    }
    
    // New visitor methods
    public override void VisitConstDeclaration(ConstDeclaration node)
    {
        foreach (var spec in node.Specs)
            spec.Accept(this);
    }
    
    public override void VisitConstSpec(ConstSpec node)
    {
        node.Type?.Accept(this);
        foreach (var value in node.Values)
            value.Accept(this);
    }
    
    
    public override void VisitForRangeStatement(ForRangeStatement node)
    {
        node.Range.Accept(this);
        node.Body.Accept(this);
    }
    
    public override void VisitTypeSwitchStatement(TypeSwitchStatement node)
    {
        node.Init?.Accept(this);
        node.Assign?.Accept(this);
        foreach (var @case in node.Cases)
            @case.Accept(this);
    }
    
    public override void VisitTypeCaseClause(TypeCaseClause node)
    {
        foreach (var type in node.Types)
            type.Accept(this);
        foreach (var stmt in node.Statements)
            stmt.Accept(this);
    }
    
    public override void VisitSelectStatement(SelectStatement node)
    {
        foreach (var @case in node.Cases)
            @case.Accept(this);
    }
    
    public override void VisitCommClause(CommClause node)
    {
        node.Comm?.Accept(this);
        foreach (var stmt in node.Statements)
            stmt.Accept(this);
    }
    
    public override void VisitSendStatement(SendStatement node)
    {
        node.Channel.Accept(this);
        node.Value.Accept(this);
    }
    
    public override void VisitIncDecStatement(IncDecStatement node)
    {
        node.Expression.Accept(this);
    }
    
    public override void VisitShortVariableDeclaration(ShortVariableDeclaration node)
    {
        foreach (var value in node.Values)
            value.Accept(this);
    }
    
    public override void VisitLabeledStatement(LabeledStatement node)
    {
        node.Statement.Accept(this);
    }
    
    public override void VisitFallthroughStatement(FallthroughStatement node)
    {
        // Nothing to visit
    }
    
    public override void VisitDeclarationStatement(DeclarationStatement node)
    {
        node.Declaration.Accept(this);
    }
    
    public override void VisitEmptyStatement(EmptyStatement node)
    {
        // Nothing to visit
    }
    
    public override void VisitKeyedElement(KeyedElement node)
    {
        node.Key?.Accept(this);
        node.Value.Accept(this);
    }
    
    public override void VisitParenthesizedExpression(ParenthesizedExpression node)
    {
        node.Expression.Accept(this);
    }
    
    public override void VisitConversionExpression(ConversionExpression node)
    {
        node.Type.Accept(this);
        node.Expression.Accept(this);
    }
    
    public override void VisitEllipsisExpression(EllipsisExpression node)
    {
        node.Type?.Accept(this);
    }
}