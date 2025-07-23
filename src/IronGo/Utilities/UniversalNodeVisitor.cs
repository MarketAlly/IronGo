using IronGo.AST;

namespace IronGo.Utilities;

/// <summary>
/// Base class that provides a universal node checking mechanism for all visitor methods
/// </summary>
internal abstract class UniversalNodeVisitor : GoAstWalker
{
    protected abstract void ProcessNode(IGoNode node);
    
    public override void VisitSourceFile(SourceFile node) { ProcessNode(node); base.VisitSourceFile(node); }
    public override void VisitPackageDeclaration(PackageDeclaration node) { ProcessNode(node); base.VisitPackageDeclaration(node); }
    public override void VisitImportDeclaration(ImportDeclaration node) { ProcessNode(node); base.VisitImportDeclaration(node); }
    public override void VisitImportSpec(ImportSpec node) { ProcessNode(node); base.VisitImportSpec(node); }
    public override void VisitFunctionDeclaration(FunctionDeclaration node) { ProcessNode(node); base.VisitFunctionDeclaration(node); }
    public override void VisitMethodDeclaration(MethodDeclaration node) { ProcessNode(node); base.VisitMethodDeclaration(node); }
    public override void VisitFieldDeclaration(FieldDeclaration node) { ProcessNode(node); base.VisitFieldDeclaration(node); }
    public override void VisitTypeDeclaration(TypeDeclaration node) { ProcessNode(node); base.VisitTypeDeclaration(node); }
    public override void VisitConstDeclaration(ConstDeclaration node) { ProcessNode(node); base.VisitConstDeclaration(node); }
    public override void VisitVariableDeclaration(VariableDeclaration node) { ProcessNode(node); base.VisitVariableDeclaration(node); }
    public override void VisitVariableSpec(VariableSpec node) { ProcessNode(node); base.VisitVariableSpec(node); }
    public override void VisitConstSpec(ConstSpec node) { ProcessNode(node); base.VisitConstSpec(node); }
    public override void VisitParameter(Parameter node) { ProcessNode(node); base.VisitParameter(node); }
    public override void VisitTypeSpec(TypeSpec node) { ProcessNode(node); base.VisitTypeSpec(node); }
    
    // Statements
    public override void VisitBlockStatement(BlockStatement node) { ProcessNode(node); base.VisitBlockStatement(node); }
    public override void VisitExpressionStatement(ExpressionStatement node) { ProcessNode(node); base.VisitExpressionStatement(node); }
    public override void VisitSendStatement(SendStatement node) { ProcessNode(node); base.VisitSendStatement(node); }
    public override void VisitIncDecStatement(IncDecStatement node) { ProcessNode(node); base.VisitIncDecStatement(node); }
    public override void VisitAssignmentStatement(AssignmentStatement node) { ProcessNode(node); base.VisitAssignmentStatement(node); }
    public override void VisitShortVariableDeclaration(ShortVariableDeclaration node) { ProcessNode(node); base.VisitShortVariableDeclaration(node); }
    public override void VisitIfStatement(IfStatement node) { ProcessNode(node); base.VisitIfStatement(node); }
    public override void VisitSwitchStatement(SwitchStatement node) { ProcessNode(node); base.VisitSwitchStatement(node); }
    public override void VisitCaseClause(CaseClause node) { ProcessNode(node); base.VisitCaseClause(node); }
    public override void VisitTypeSwitchStatement(TypeSwitchStatement node) { ProcessNode(node); base.VisitTypeSwitchStatement(node); }
    public override void VisitTypeCaseClause(TypeCaseClause node) { ProcessNode(node); base.VisitTypeCaseClause(node); }
    public override void VisitSelectStatement(SelectStatement node) { ProcessNode(node); base.VisitSelectStatement(node); }
    public override void VisitCommClause(CommClause node) { ProcessNode(node); base.VisitCommClause(node); }
    public override void VisitForStatement(ForStatement node) { ProcessNode(node); base.VisitForStatement(node); }
    public override void VisitForRangeStatement(ForRangeStatement node) { ProcessNode(node); base.VisitForRangeStatement(node); }
    public override void VisitReturnStatement(ReturnStatement node) { ProcessNode(node); base.VisitReturnStatement(node); }
    public override void VisitBranchStatement(BranchStatement node) { ProcessNode(node); base.VisitBranchStatement(node); }
    public override void VisitLabeledStatement(LabeledStatement node) { ProcessNode(node); base.VisitLabeledStatement(node); }
    public override void VisitGoStatement(GoStatement node) { ProcessNode(node); base.VisitGoStatement(node); }
    public override void VisitDeferStatement(DeferStatement node) { ProcessNode(node); base.VisitDeferStatement(node); }
    public override void VisitFallthroughStatement(FallthroughStatement node) { ProcessNode(node); base.VisitFallthroughStatement(node); }
    public override void VisitDeclarationStatement(DeclarationStatement node) { ProcessNode(node); base.VisitDeclarationStatement(node); }
    public override void VisitEmptyStatement(EmptyStatement node) { ProcessNode(node); base.VisitEmptyStatement(node); }
    public override void VisitRangeStatement(RangeStatement node) { ProcessNode(node); base.VisitRangeStatement(node); }
    
    // Expressions
    public override void VisitIdentifierExpression(IdentifierExpression node) { ProcessNode(node); base.VisitIdentifierExpression(node); }
    public override void VisitLiteralExpression(LiteralExpression node) { ProcessNode(node); base.VisitLiteralExpression(node); }
    public override void VisitFunctionLiteral(FunctionLiteral node) { ProcessNode(node); base.VisitFunctionLiteral(node); }
    public override void VisitCompositeLiteral(CompositeLiteral node) { ProcessNode(node); base.VisitCompositeLiteral(node); }
    public override void VisitCompositeLiteralElement(CompositeLiteralElement node) { ProcessNode(node); base.VisitCompositeLiteralElement(node); }
    public override void VisitKeyedElement(KeyedElement node) { ProcessNode(node); base.VisitKeyedElement(node); }
    public override void VisitBinaryExpression(BinaryExpression node) { ProcessNode(node); base.VisitBinaryExpression(node); }
    public override void VisitUnaryExpression(UnaryExpression node) { ProcessNode(node); base.VisitUnaryExpression(node); }
    public override void VisitCallExpression(CallExpression node) { ProcessNode(node); base.VisitCallExpression(node); }
    public override void VisitIndexExpression(IndexExpression node) { ProcessNode(node); base.VisitIndexExpression(node); }
    public override void VisitSliceExpression(SliceExpression node) { ProcessNode(node); base.VisitSliceExpression(node); }
    public override void VisitSelectorExpression(SelectorExpression node) { ProcessNode(node); base.VisitSelectorExpression(node); }
    public override void VisitTypeAssertionExpression(TypeAssertionExpression node) { ProcessNode(node); base.VisitTypeAssertionExpression(node); }
    public override void VisitParenthesizedExpression(ParenthesizedExpression node) { ProcessNode(node); base.VisitParenthesizedExpression(node); }
    public override void VisitConversionExpression(ConversionExpression node) { ProcessNode(node); base.VisitConversionExpression(node); }
    public override void VisitEllipsisExpression(EllipsisExpression node) { ProcessNode(node); base.VisitEllipsisExpression(node); }
    
    // Types
    public override void VisitIdentifierType(IdentifierType node) { ProcessNode(node); base.VisitIdentifierType(node); }
    public override void VisitArrayType(ArrayType node) { ProcessNode(node); base.VisitArrayType(node); }
    public override void VisitSliceType(SliceType node) { ProcessNode(node); base.VisitSliceType(node); }
    public override void VisitStructType(StructType node) { ProcessNode(node); base.VisitStructType(node); }
    public override void VisitPointerType(PointerType node) { ProcessNode(node); base.VisitPointerType(node); }
    public override void VisitFunctionType(FunctionType node) { ProcessNode(node); base.VisitFunctionType(node); }
    public override void VisitInterfaceType(InterfaceType node) { ProcessNode(node); base.VisitInterfaceType(node); }
    public override void VisitInterfaceMethod(InterfaceMethod node) { ProcessNode(node); base.VisitInterfaceMethod(node); }
    public override void VisitInterfaceEmbedding(InterfaceEmbedding node) { ProcessNode(node); base.VisitInterfaceEmbedding(node); }
    public override void VisitMapType(MapType node) { ProcessNode(node); base.VisitMapType(node); }
    public override void VisitChannelType(ChannelType node) { ProcessNode(node); base.VisitChannelType(node); }
    public override void VisitTypeParameter(TypeParameter node) { ProcessNode(node); base.VisitTypeParameter(node); }
    
    // Other
    public override void VisitComment(Comment node) { ProcessNode(node); base.VisitComment(node); }
}