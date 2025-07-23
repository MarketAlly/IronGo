namespace MarketAlly.IronGo.AST;

/// <summary>
/// Base implementation of visitor pattern that does nothing by default
/// </summary>
public abstract class GoAstVisitorBase : IGoAstVisitor
{
    // File and declarations
    public virtual void VisitSourceFile(SourceFile node) { }
    public virtual void VisitPackageDeclaration(PackageDeclaration node) { }
    public virtual void VisitImportDeclaration(ImportDeclaration node) { }
    public virtual void VisitImportSpec(ImportSpec node) { }
    public virtual void VisitFunctionDeclaration(FunctionDeclaration node) { }
    public virtual void VisitMethodDeclaration(MethodDeclaration node) { }
    public virtual void VisitParameter(Parameter node) { }
    public virtual void VisitTypeParameter(TypeParameter node) { }
    public virtual void VisitTypeDeclaration(TypeDeclaration node) { }
    public virtual void VisitTypeSpec(TypeSpec node) { }
    public virtual void VisitVariableDeclaration(VariableDeclaration node) { }
    public virtual void VisitVariableSpec(VariableSpec node) { }
    public virtual void VisitConstDeclaration(ConstDeclaration node) { }
    public virtual void VisitConstSpec(ConstSpec node) { }
    
    // Types
    public virtual void VisitIdentifierType(IdentifierType node) { }
    public virtual void VisitPointerType(PointerType node) { }
    public virtual void VisitArrayType(ArrayType node) { }
    public virtual void VisitSliceType(SliceType node) { }
    public virtual void VisitMapType(MapType node) { }
    public virtual void VisitChannelType(ChannelType node) { }
    public virtual void VisitFunctionType(FunctionType node) { }
    public virtual void VisitInterfaceType(InterfaceType node) { }
    public virtual void VisitInterfaceMethod(InterfaceMethod node) { }
    public virtual void VisitInterfaceEmbedding(InterfaceEmbedding node) { }
    public virtual void VisitStructType(StructType node) { }
    public virtual void VisitFieldDeclaration(FieldDeclaration node) { }
    public virtual void VisitTypeInstantiation(TypeInstantiation node) { }
    public virtual void VisitTypeUnion(TypeUnion node) { }
    public virtual void VisitTypeTerm(TypeTerm node) { }
    public virtual void VisitTypeElement(TypeElement node) { }
    
    // Statements
    public virtual void VisitBlockStatement(BlockStatement node) { }
    public virtual void VisitExpressionStatement(ExpressionStatement node) { }
    public virtual void VisitAssignmentStatement(AssignmentStatement node) { }
    public virtual void VisitIfStatement(IfStatement node) { }
    public virtual void VisitForStatement(ForStatement node) { }
    public virtual void VisitRangeStatement(RangeStatement node) { }
    public virtual void VisitForRangeStatement(ForRangeStatement node) { }
    public virtual void VisitSwitchStatement(SwitchStatement node) { }
    public virtual void VisitCaseClause(CaseClause node) { }
    public virtual void VisitTypeSwitchStatement(TypeSwitchStatement node) { }
    public virtual void VisitTypeCaseClause(TypeCaseClause node) { }
    public virtual void VisitSelectStatement(SelectStatement node) { }
    public virtual void VisitCommClause(CommClause node) { }
    public virtual void VisitSendStatement(SendStatement node) { }
    public virtual void VisitIncDecStatement(IncDecStatement node) { }
    public virtual void VisitShortVariableDeclaration(ShortVariableDeclaration node) { }
    public virtual void VisitLabeledStatement(LabeledStatement node) { }
    public virtual void VisitFallthroughStatement(FallthroughStatement node) { }
    public virtual void VisitDeclarationStatement(DeclarationStatement node) { }
    public virtual void VisitEmptyStatement(EmptyStatement node) { }
    public virtual void VisitReturnStatement(ReturnStatement node) { }
    public virtual void VisitBranchStatement(BranchStatement node) { }
    public virtual void VisitDeferStatement(DeferStatement node) { }
    public virtual void VisitGoStatement(GoStatement node) { }
    
    // Expressions
    public virtual void VisitIdentifierExpression(IdentifierExpression node) { }
    public virtual void VisitLiteralExpression(LiteralExpression node) { }
    public virtual void VisitBinaryExpression(BinaryExpression node) { }
    public virtual void VisitUnaryExpression(UnaryExpression node) { }
    public virtual void VisitCallExpression(CallExpression node) { }
    public virtual void VisitSelectorExpression(SelectorExpression node) { }
    public virtual void VisitIndexExpression(IndexExpression node) { }
    public virtual void VisitSliceExpression(SliceExpression node) { }
    public virtual void VisitTypeAssertionExpression(TypeAssertionExpression node) { }
    public virtual void VisitCompositeLiteral(CompositeLiteral node) { }
    public virtual void VisitCompositeLiteralElement(CompositeLiteralElement node) { }
    public virtual void VisitFunctionLiteral(FunctionLiteral node) { }
    public virtual void VisitKeyedElement(KeyedElement node) { }
    public virtual void VisitParenthesizedExpression(ParenthesizedExpression node) { }
    public virtual void VisitConversionExpression(ConversionExpression node) { }
    public virtual void VisitEllipsisExpression(EllipsisExpression node) { }
    
    // Other
    public virtual void VisitComment(Comment node) { }
}

/// <summary>
/// Base implementation of generic visitor pattern that returns default values
/// </summary>
public abstract class GoAstVisitorBase<T> : IGoAstVisitor<T>
{
    protected abstract T DefaultResult { get; }
    
    // File and declarations
    public virtual T VisitSourceFile(SourceFile node) => DefaultResult;
    public virtual T VisitPackageDeclaration(PackageDeclaration node) => DefaultResult;
    public virtual T VisitImportDeclaration(ImportDeclaration node) => DefaultResult;
    public virtual T VisitImportSpec(ImportSpec node) => DefaultResult;
    public virtual T VisitFunctionDeclaration(FunctionDeclaration node) => DefaultResult;
    public virtual T VisitMethodDeclaration(MethodDeclaration node) => DefaultResult;
    public virtual T VisitParameter(Parameter node) => DefaultResult;
    public virtual T VisitTypeParameter(TypeParameter node) => DefaultResult;
    public virtual T VisitTypeDeclaration(TypeDeclaration node) => DefaultResult;
    public virtual T VisitTypeSpec(TypeSpec node) => DefaultResult;
    public virtual T VisitVariableDeclaration(VariableDeclaration node) => DefaultResult;
    public virtual T VisitVariableSpec(VariableSpec node) => DefaultResult;
    public virtual T VisitConstDeclaration(ConstDeclaration node) => DefaultResult;
    public virtual T VisitConstSpec(ConstSpec node) => DefaultResult;
    
    // Types
    public virtual T VisitIdentifierType(IdentifierType node) => DefaultResult;
    public virtual T VisitPointerType(PointerType node) => DefaultResult;
    public virtual T VisitArrayType(ArrayType node) => DefaultResult;
    public virtual T VisitSliceType(SliceType node) => DefaultResult;
    public virtual T VisitMapType(MapType node) => DefaultResult;
    public virtual T VisitChannelType(ChannelType node) => DefaultResult;
    public virtual T VisitFunctionType(FunctionType node) => DefaultResult;
    public virtual T VisitInterfaceType(InterfaceType node) => DefaultResult;
    public virtual T VisitInterfaceMethod(InterfaceMethod node) => DefaultResult;
    public virtual T VisitInterfaceEmbedding(InterfaceEmbedding node) => DefaultResult;
    public virtual T VisitStructType(StructType node) => DefaultResult;
    public virtual T VisitFieldDeclaration(FieldDeclaration node) => DefaultResult;
    public virtual T VisitTypeInstantiation(TypeInstantiation node) => DefaultResult;
    public virtual T VisitTypeUnion(TypeUnion node) => DefaultResult;
    public virtual T VisitTypeTerm(TypeTerm node) => DefaultResult;
    public virtual T VisitTypeElement(TypeElement node) => DefaultResult;
    
    // Statements
    public virtual T VisitBlockStatement(BlockStatement node) => DefaultResult;
    public virtual T VisitExpressionStatement(ExpressionStatement node) => DefaultResult;
    public virtual T VisitAssignmentStatement(AssignmentStatement node) => DefaultResult;
    public virtual T VisitIfStatement(IfStatement node) => DefaultResult;
    public virtual T VisitForStatement(ForStatement node) => DefaultResult;
    public virtual T VisitRangeStatement(RangeStatement node) => DefaultResult;
    public virtual T VisitForRangeStatement(ForRangeStatement node) => DefaultResult;
    public virtual T VisitSwitchStatement(SwitchStatement node) => DefaultResult;
    public virtual T VisitCaseClause(CaseClause node) => DefaultResult;
    public virtual T VisitTypeSwitchStatement(TypeSwitchStatement node) => DefaultResult;
    public virtual T VisitTypeCaseClause(TypeCaseClause node) => DefaultResult;
    public virtual T VisitSelectStatement(SelectStatement node) => DefaultResult;
    public virtual T VisitCommClause(CommClause node) => DefaultResult;
    public virtual T VisitSendStatement(SendStatement node) => DefaultResult;
    public virtual T VisitIncDecStatement(IncDecStatement node) => DefaultResult;
    public virtual T VisitShortVariableDeclaration(ShortVariableDeclaration node) => DefaultResult;
    public virtual T VisitLabeledStatement(LabeledStatement node) => DefaultResult;
    public virtual T VisitFallthroughStatement(FallthroughStatement node) => DefaultResult;
    public virtual T VisitDeclarationStatement(DeclarationStatement node) => DefaultResult;
    public virtual T VisitEmptyStatement(EmptyStatement node) => DefaultResult;
    public virtual T VisitReturnStatement(ReturnStatement node) => DefaultResult;
    public virtual T VisitBranchStatement(BranchStatement node) => DefaultResult;
    public virtual T VisitDeferStatement(DeferStatement node) => DefaultResult;
    public virtual T VisitGoStatement(GoStatement node) => DefaultResult;
    
    // Expressions
    public virtual T VisitIdentifierExpression(IdentifierExpression node) => DefaultResult;
    public virtual T VisitLiteralExpression(LiteralExpression node) => DefaultResult;
    public virtual T VisitBinaryExpression(BinaryExpression node) => DefaultResult;
    public virtual T VisitUnaryExpression(UnaryExpression node) => DefaultResult;
    public virtual T VisitCallExpression(CallExpression node) => DefaultResult;
    public virtual T VisitSelectorExpression(SelectorExpression node) => DefaultResult;
    public virtual T VisitIndexExpression(IndexExpression node) => DefaultResult;
    public virtual T VisitSliceExpression(SliceExpression node) => DefaultResult;
    public virtual T VisitTypeAssertionExpression(TypeAssertionExpression node) => DefaultResult;
    public virtual T VisitCompositeLiteral(CompositeLiteral node) => DefaultResult;
    public virtual T VisitCompositeLiteralElement(CompositeLiteralElement node) => DefaultResult;
    public virtual T VisitFunctionLiteral(FunctionLiteral node) => DefaultResult;
    public virtual T VisitKeyedElement(KeyedElement node) => DefaultResult;
    public virtual T VisitParenthesizedExpression(ParenthesizedExpression node) => DefaultResult;
    public virtual T VisitConversionExpression(ConversionExpression node) => DefaultResult;
    public virtual T VisitEllipsisExpression(EllipsisExpression node) => DefaultResult;
    
    // Other
    public virtual T VisitComment(Comment node) => DefaultResult;
}