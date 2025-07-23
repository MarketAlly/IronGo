namespace IronGo.AST;

/// <summary>
/// Visitor interface for traversing Go AST nodes without returning a value
/// </summary>
public interface IGoAstVisitor
{
    // File and declarations
    void VisitSourceFile(SourceFile node);
    void VisitPackageDeclaration(PackageDeclaration node);
    void VisitImportDeclaration(ImportDeclaration node);
    void VisitImportSpec(ImportSpec node);
    void VisitFunctionDeclaration(FunctionDeclaration node);
    void VisitMethodDeclaration(MethodDeclaration node);
    void VisitParameter(Parameter node);
    void VisitTypeParameter(TypeParameter node);
    void VisitTypeDeclaration(TypeDeclaration node);
    void VisitTypeSpec(TypeSpec node);
    void VisitVariableDeclaration(VariableDeclaration node);
    void VisitVariableSpec(VariableSpec node);
    void VisitConstDeclaration(ConstDeclaration node);
    void VisitConstSpec(ConstSpec node);
    
    // Types
    void VisitIdentifierType(IdentifierType node);
    void VisitPointerType(PointerType node);
    void VisitArrayType(ArrayType node);
    void VisitSliceType(SliceType node);
    void VisitMapType(MapType node);
    void VisitChannelType(ChannelType node);
    void VisitFunctionType(FunctionType node);
    void VisitInterfaceType(InterfaceType node);
    void VisitInterfaceMethod(InterfaceMethod node);
    void VisitInterfaceEmbedding(InterfaceEmbedding node);
    void VisitStructType(StructType node);
    void VisitFieldDeclaration(FieldDeclaration node);
    void VisitTypeInstantiation(TypeInstantiation node);
    void VisitTypeUnion(TypeUnion node);
    void VisitTypeTerm(TypeTerm node);
    void VisitTypeElement(TypeElement node);
    
    // Statements
    void VisitBlockStatement(BlockStatement node);
    void VisitExpressionStatement(ExpressionStatement node);
    void VisitAssignmentStatement(AssignmentStatement node);
    void VisitIfStatement(IfStatement node);
    void VisitForStatement(ForStatement node);
    void VisitRangeStatement(RangeStatement node);
    void VisitForRangeStatement(ForRangeStatement node);
    void VisitSwitchStatement(SwitchStatement node);
    void VisitCaseClause(CaseClause node);
    void VisitTypeSwitchStatement(TypeSwitchStatement node);
    void VisitTypeCaseClause(TypeCaseClause node);
    void VisitSelectStatement(SelectStatement node);
    void VisitCommClause(CommClause node);
    void VisitSendStatement(SendStatement node);
    void VisitIncDecStatement(IncDecStatement node);
    void VisitShortVariableDeclaration(ShortVariableDeclaration node);
    void VisitLabeledStatement(LabeledStatement node);
    void VisitFallthroughStatement(FallthroughStatement node);
    void VisitDeclarationStatement(DeclarationStatement node);
    void VisitEmptyStatement(EmptyStatement node);
    void VisitReturnStatement(ReturnStatement node);
    void VisitBranchStatement(BranchStatement node);
    void VisitDeferStatement(DeferStatement node);
    void VisitGoStatement(GoStatement node);
    
    // Expressions
    void VisitIdentifierExpression(IdentifierExpression node);
    void VisitLiteralExpression(LiteralExpression node);
    void VisitBinaryExpression(BinaryExpression node);
    void VisitUnaryExpression(UnaryExpression node);
    void VisitCallExpression(CallExpression node);
    void VisitSelectorExpression(SelectorExpression node);
    void VisitIndexExpression(IndexExpression node);
    void VisitSliceExpression(SliceExpression node);
    void VisitTypeAssertionExpression(TypeAssertionExpression node);
    void VisitCompositeLiteral(CompositeLiteral node);
    void VisitCompositeLiteralElement(CompositeLiteralElement node);
    void VisitFunctionLiteral(FunctionLiteral node);
    void VisitKeyedElement(KeyedElement node);
    void VisitParenthesizedExpression(ParenthesizedExpression node);
    void VisitConversionExpression(ConversionExpression node);
    void VisitEllipsisExpression(EllipsisExpression node);
    
    // Other
    void VisitComment(Comment node);
}

/// <summary>
/// Generic visitor interface for traversing Go AST nodes and returning a value
/// </summary>
public interface IGoAstVisitor<T>
{
    // File and declarations
    T VisitSourceFile(SourceFile node);
    T VisitPackageDeclaration(PackageDeclaration node);
    T VisitImportDeclaration(ImportDeclaration node);
    T VisitImportSpec(ImportSpec node);
    T VisitFunctionDeclaration(FunctionDeclaration node);
    T VisitMethodDeclaration(MethodDeclaration node);
    T VisitParameter(Parameter node);
    T VisitTypeParameter(TypeParameter node);
    T VisitTypeDeclaration(TypeDeclaration node);
    T VisitTypeSpec(TypeSpec node);
    T VisitVariableDeclaration(VariableDeclaration node);
    T VisitVariableSpec(VariableSpec node);
    T VisitConstDeclaration(ConstDeclaration node);
    T VisitConstSpec(ConstSpec node);
    
    // Types
    T VisitIdentifierType(IdentifierType node);
    T VisitPointerType(PointerType node);
    T VisitArrayType(ArrayType node);
    T VisitSliceType(SliceType node);
    T VisitMapType(MapType node);
    T VisitChannelType(ChannelType node);
    T VisitFunctionType(FunctionType node);
    T VisitInterfaceType(InterfaceType node);
    T VisitInterfaceMethod(InterfaceMethod node);
    T VisitInterfaceEmbedding(InterfaceEmbedding node);
    T VisitStructType(StructType node);
    T VisitFieldDeclaration(FieldDeclaration node);
    T VisitTypeInstantiation(TypeInstantiation node);
    T VisitTypeUnion(TypeUnion node);
    T VisitTypeTerm(TypeTerm node);
    T VisitTypeElement(TypeElement node);
    
    // Statements
    T VisitBlockStatement(BlockStatement node);
    T VisitExpressionStatement(ExpressionStatement node);
    T VisitAssignmentStatement(AssignmentStatement node);
    T VisitIfStatement(IfStatement node);
    T VisitForStatement(ForStatement node);
    T VisitRangeStatement(RangeStatement node);
    T VisitForRangeStatement(ForRangeStatement node);
    T VisitSwitchStatement(SwitchStatement node);
    T VisitCaseClause(CaseClause node);
    T VisitTypeSwitchStatement(TypeSwitchStatement node);
    T VisitTypeCaseClause(TypeCaseClause node);
    T VisitSelectStatement(SelectStatement node);
    T VisitCommClause(CommClause node);
    T VisitSendStatement(SendStatement node);
    T VisitIncDecStatement(IncDecStatement node);
    T VisitShortVariableDeclaration(ShortVariableDeclaration node);
    T VisitLabeledStatement(LabeledStatement node);
    T VisitFallthroughStatement(FallthroughStatement node);
    T VisitDeclarationStatement(DeclarationStatement node);
    T VisitEmptyStatement(EmptyStatement node);
    T VisitReturnStatement(ReturnStatement node);
    T VisitBranchStatement(BranchStatement node);
    T VisitDeferStatement(DeferStatement node);
    T VisitGoStatement(GoStatement node);
    
    // Expressions
    T VisitIdentifierExpression(IdentifierExpression node);
    T VisitLiteralExpression(LiteralExpression node);
    T VisitBinaryExpression(BinaryExpression node);
    T VisitUnaryExpression(UnaryExpression node);
    T VisitCallExpression(CallExpression node);
    T VisitSelectorExpression(SelectorExpression node);
    T VisitIndexExpression(IndexExpression node);
    T VisitSliceExpression(SliceExpression node);
    T VisitTypeAssertionExpression(TypeAssertionExpression node);
    T VisitCompositeLiteral(CompositeLiteral node);
    T VisitCompositeLiteralElement(CompositeLiteralElement node);
    T VisitFunctionLiteral(FunctionLiteral node);
    T VisitKeyedElement(KeyedElement node);
    T VisitParenthesizedExpression(ParenthesizedExpression node);
    T VisitConversionExpression(ConversionExpression node);
    T VisitEllipsisExpression(EllipsisExpression node);
    
    // Other
    T VisitComment(Comment node);
}