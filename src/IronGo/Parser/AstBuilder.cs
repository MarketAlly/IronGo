using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using MarketAlly.IronGo.AST;

namespace MarketAlly.IronGo.Parser;

/// <summary>
/// Builds an IronGo AST from an ANTLR parse tree
/// </summary>
internal class AstBuilder : GoParserBaseVisitor<IGoNode?>
{
    private readonly List<Comment> _comments = new();
    
    public SourceFile BuildSourceFile(GoParser.SourceFileContext context)
    {
        var result = Visit(context);
        if (result is SourceFile sourceFile)
            return sourceFile;
            
        throw new InvalidOperationException("Failed to build source file AST");
    }
    
    public override IGoNode? VisitSourceFile(GoParser.SourceFileContext context)
    {
        var package = VisitPackageClause(context.packageClause()) as PackageDeclaration
            ?? throw new InvalidOperationException("Missing package declaration");
            
        var imports = new List<ImportDeclaration>();
        foreach (var importDecl in context.importDecl())
        {
            if (Visit(importDecl) is ImportDeclaration import)
                imports.Add(import);
        }
        
        var declarations = new List<IDeclaration>();
        foreach (var child in context.children)
        {
            if (child is GoParser.FunctionDeclContext funcDecl)
            {
                if (Visit(funcDecl) is IDeclaration decl)
                    declarations.Add(decl);
            }
            else if (child is GoParser.MethodDeclContext methodDecl)
            {
                if (Visit(methodDecl) is IDeclaration decl)
                    declarations.Add(decl);
            }
            else if (child is GoParser.DeclarationContext decl)
            {
                if (Visit(decl) is IDeclaration declaration)
                    declarations.Add(declaration);
            }
        }
        
        return new SourceFile(
            package,
            imports,
            declarations,
            _comments,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitPackageClause(GoParser.PackageClauseContext context)
    {
        var name = context.packageName().identifier().GetText();
        return new PackageDeclaration(
            name,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitImportDecl(GoParser.ImportDeclContext context)
    {
        var specs = new List<ImportSpec>();
        foreach (var spec in context.importSpec())
        {
            if (Visit(spec) is ImportSpec importSpec)
                specs.Add(importSpec);
        }
        
        return new ImportDeclaration(
            specs,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitImportSpec(GoParser.ImportSpecContext context)
    {
        string? alias = null;
        
        // Check for dot import
        if (context.DOT() != null)
        {
            alias = ".";
        }
        // Check for package alias
        else if (context.packageName() != null)
        {
            alias = context.packageName().identifier().GetText();
        }
        
        var path = context.importPath().string_().GetText().Trim('"', '`');
        
        return new ImportSpec(
            alias,
            path,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitFunctionDecl(GoParser.FunctionDeclContext context)
    {
        var name = context.IDENTIFIER().GetText();
        var signature = context.signature();
        
        // Parse type parameters if present
        var typeParameters = context.typeParameters() != null 
            ? ParseTypeParameters(context.typeParameters()) 
            : null;
        
        var parameters = ParseParameters(signature.parameters());
        var returnParams = signature.result() != null ? ParseResult(signature.result()) : null;
        var body = context.block() != null ? Visit(context.block()) as BlockStatement : null;
        
        return new FunctionDeclaration(
            name,
            typeParameters,
            parameters,
            returnParams,
            body,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitMethodDecl(GoParser.MethodDeclContext context)
    {
        var receiver = ParseReceiver(context.receiver());
        var name = context.IDENTIFIER().GetText();
        var signature = context.signature();
        
        var parameters = ParseParameters(signature.parameters());
        var returnParams = signature.result() != null ? ParseResult(signature.result()) : null;
        var body = context.block() != null ? Visit(context.block()) as BlockStatement : null;
        
        return new MethodDeclaration(
            receiver,
            name,
            null, // Type parameters not in current grammar
            parameters,
            returnParams,
            body,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    private Parameter ParseReceiver(GoParser.ReceiverContext context)
    {
        var paramList = context.parameters();
        if (paramList.parameterDecl().Length != 1)
            throw new InvalidOperationException("Receiver must have exactly one parameter");
            
        var param = paramList.parameterDecl()[0];
        var names = param.identifierList()?.IDENTIFIER().Select(id => id.GetText()).ToList() 
            ?? new List<string>();
        var type = Visit(param.type_()) as IType 
            ?? throw new InvalidOperationException("Invalid receiver type");
            
        return new Parameter(
            names,
            type,
            false,
            GetPosition(param.Start),
            GetPosition(param.Stop ?? param.Start));
    }
    
    private IReadOnlyList<Parameter> ParseParameters(GoParser.ParametersContext context)
    {
        var result = new List<Parameter>();
        
        foreach (var paramDecl in context.parameterDecl())
        {
            var names = paramDecl.identifierList()?.IDENTIFIER().Select(id => id.GetText()).ToList() 
                ?? new List<string>();
            var type = Visit(paramDecl.type_()) as IType 
                ?? throw new InvalidOperationException("Invalid parameter type");
            var isVariadic = paramDecl.ELLIPSIS() != null;
            
            result.Add(new Parameter(
                names,
                type,
                isVariadic,
                GetPosition(paramDecl.Start),
                GetPosition(paramDecl.Stop ?? paramDecl.Start)));
        }
        
        return result;
    }
    
    private IReadOnlyList<Parameter>? ParseResult(GoParser.ResultContext context)
    {
        if (context.parameters() != null)
            return ParseParameters(context.parameters());
            
        if (context.type_() != null)
        {
            var type = Visit(context.type_()) as IType 
                ?? throw new InvalidOperationException("Invalid return type");
                
            return new List<Parameter>
            {
                new Parameter(
                    new List<string>(),
                    type,
                    false,
                    GetPosition(context.Start),
                    GetPosition(context.Stop ?? context.Start))
            };
        }
        
        return null;
    }
    
    // Type visitors
    public override IGoNode? VisitType_(GoParser.Type_Context context)
    {
        if (context.typeName() != null)
        {
            var baseType = Visit(context.typeName()) as IType;
            if (baseType != null && context.typeArgs() != null)
            {
                var typeArgs = VisitTypeArgs(context.typeArgs());
                if (typeArgs != null && typeArgs.Count > 0)
                {
                    // If it's an IdentifierType, update it with type arguments
                    if (baseType is IdentifierType identType)
                    {
                        return new IdentifierType(
                            identType.Name,
                            identType.Package,
                            typeArgs,
                            identType.Start,
                            GetPosition(context.Stop ?? context.Start));
                    }
                    // Otherwise create a TypeInstantiation
                    return new TypeInstantiation(
                        baseType,
                        typeArgs,
                        baseType.Start,
                        GetPosition(context.Stop ?? context.Start));
                }
            }
            return baseType;
        }
        if (context.typeLit() != null)
            return Visit(context.typeLit());
        if (context.L_PAREN() != null)
            return Visit(context.type_());
            
        return null;
    }
    
    public override IGoNode? VisitTypeName(GoParser.TypeNameContext context)
    {
        if (context.qualifiedIdent() != null)
        {
            var qualIdent = context.qualifiedIdent();
            var parts = qualIdent.GetText().Split('.');
            
            if (parts.Length > 1)
            {
                return new IdentifierType(
                    parts[1],
                    parts[0],
                    GetPosition(context.Start),
                    GetPosition(context.Stop ?? context.Start));
            }
            else
            {
                return new IdentifierType(
                    parts[0],
                    null,
                    GetPosition(context.Start),
                    GetPosition(context.Stop ?? context.Start));
            }
        }
        
        return new IdentifierType(
            context.IDENTIFIER().GetText(),
            null,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitTypeLit(GoParser.TypeLitContext context)
    {
        if (context.arrayType() != null)
            return Visit(context.arrayType());
        if (context.structType() != null)
            return Visit(context.structType());
        if (context.pointerType() != null)
            return Visit(context.pointerType());
        if (context.functionType() != null)
            return Visit(context.functionType());
        if (context.interfaceType() != null)
            return Visit(context.interfaceType());
        if (context.sliceType() != null)
            return Visit(context.sliceType());
        if (context.mapType() != null)
            return Visit(context.mapType());
        if (context.channelType() != null)
            return Visit(context.channelType());
            
        return null;
    }
    
    public override IGoNode? VisitArrayType(GoParser.ArrayTypeContext context)
    {
        var length = Visit(context.arrayLength().expression()) as IExpression
            ?? throw new InvalidOperationException("Invalid array length");
        var elementType = Visit(context.elementType().type_()) as IType
            ?? throw new InvalidOperationException("Invalid element type");
            
        return new ArrayType(
            length,
            elementType,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitSliceType(GoParser.SliceTypeContext context)
    {
        var elementType = Visit(context.elementType().type_()) as IType
            ?? throw new InvalidOperationException("Invalid element type");
            
        return new SliceType(
            elementType,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitStructType(GoParser.StructTypeContext context)
    {
        var fields = new List<FieldDeclaration>();
        
        foreach (var fieldDecl in context.fieldDecl())
        {
            if (Visit(fieldDecl) is FieldDeclaration field)
                fields.Add(field);
        }
        
        return new StructType(
            fields,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitFieldDecl(GoParser.FieldDeclContext context)
    {
        var names = context.identifierList()?.IDENTIFIER().Select(id => id.GetText()).ToList() 
            ?? new List<string>();
        var type = context.type_() != null ? Visit(context.type_()) as IType : null;
        var tag = context.string_()?.GetText();
        
        // Handle embedded field
        if (names.Count == 0 && context.embeddedField() != null)
        {
            var embedded = context.embeddedField();
            if (embedded.typeName() != null)
            {
                type = Visit(embedded.typeName()) as IType;
            }
        }
        
        return new FieldDeclaration(
            names,
            type,
            tag,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitPointerType(GoParser.PointerTypeContext context)
    {
        var elementType = Visit(context.type_()) as IType
            ?? throw new InvalidOperationException("Invalid pointer element type");
            
        return new PointerType(
            elementType,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitFunctionType(GoParser.FunctionTypeContext context)
    {
        var parameters = ParseParameters(context.signature().parameters());
        var returnParams = context.signature().result() != null 
            ? ParseResult(context.signature().result()) 
            : null;
            
        return new FunctionType(
            null, // No type parameters for function type literals
            parameters,
            returnParams,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitInterfaceType(GoParser.InterfaceTypeContext context)
    {
        var methods = new List<IDeclaration>();
        var typeElements = new List<TypeElement>();
        
        // The grammar is: ((methodSpec | typeElement) eos)*
        // We need to access the specific child rules properly
        var methodSpecs = context.methodSpec();
        if (methodSpecs != null)
        {
            foreach (var methodSpec in methodSpecs)
            {
                if (Visit(methodSpec) is IDeclaration method)
                    methods.Add(method);
            }
        }
        
        var typeElementContexts = context.typeElement();
        if (typeElementContexts != null)
        {
            foreach (var typeElementContext in typeElementContexts)
            {
                var node = Visit(typeElementContext);
                if (node is InterfaceEmbedding embedding)
                    methods.Add(embedding); // Embedded interfaces go in methods
                else if (node is TypeElement typeElement)
                    typeElements.Add(typeElement); // Type constraints go in typeElements
                else if (node is IDeclaration decl)
                    methods.Add(decl); // For backward compatibility
            }
        }
        
        return new InterfaceType(
            methods,
            typeElements,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitMethodSpec(GoParser.MethodSpecContext context)
    {
        var name = context.IDENTIFIER().GetText();
        var parameters = ParseParameters(context.parameters());
        var returnParams = context.result() != null 
            ? ParseResult(context.result()) 
            : null;
            
        // Create a function type for the method signature
        var signature = new FunctionType(
            null, // No type parameters
            parameters,
            returnParams,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
            
        return new InterfaceMethod(
            name,
            signature,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitTypeElement(GoParser.TypeElementContext context)
    {
        // A typeElement is: typeTerm (OR typeTerm)*
        // In an interface context, if it's a single type name, it's an embedded interface
        var typeTerms = context.typeTerm();
        if (typeTerms != null && typeTerms.Length > 0)
        {
            // Check if we're in an interface context
            bool isInInterface = context.Parent is GoParser.InterfaceTypeContext;
            
            if (typeTerms.Length == 1 && isInInterface)
            {
                // Single type term in interface - could be embedded interface
                var typeTerm = typeTerms[0];
                if (typeTerm.UNDERLYING() == null && typeTerm.type_() != null)
                {
                    var type = Visit(typeTerm.type_()) as IType;
                    if (type is IdentifierType)
                    {
                        // This is an embedded interface
                        return new InterfaceEmbedding(
                            type,
                            GetPosition(context.Start),
                            GetPosition(context.Stop ?? context.Start));
                    }
                }
            }
            
            // For generics/type constraints, create TypeElement
            if (typeTerms.Length == 1)
            {
                // Single type term - could be a simple type or underlying type
                var term = VisitTypeTerm(typeTerms[0]) as IType;
                if (term != null)
                {
                    return new TypeElement(
                        term,
                        GetPosition(context.Start),
                        GetPosition(context.Stop ?? context.Start));
                }
            }
            else
            {
                // Multiple type terms - create a type union
                var terms = new List<TypeTerm>();
                foreach (var typeTerm in typeTerms)
                {
                    if (VisitTypeTerm(typeTerm) is TypeTerm term)
                        terms.Add(term);
                }
                
                if (terms.Count > 0)
                {
                    var typeUnion = new TypeUnion(
                        terms,
                        GetPosition(context.Start),
                        GetPosition(context.Stop ?? context.Start));
                    
                    return new TypeElement(
                        typeUnion,
                        GetPosition(context.Start),
                        GetPosition(context.Stop ?? context.Start));
                }
            }
        }
        
        return null;
    }
    
    public override IGoNode? VisitTypeTerm(GoParser.TypeTermContext context)
    {
        // A typeTerm is: UNDERLYING? type_
        bool isUnderlying = context.UNDERLYING() != null;
        var type = Visit(context.type_()) as IType;
        
        if (type != null)
        {
            if (context.Parent is GoParser.TypeElementContext parentElement && 
                parentElement.typeTerm().Length > 1)
            {
                // This is part of a union, so return a TypeTerm
                return new TypeTerm(
                    isUnderlying,
                    type,
                    GetPosition(context.Start),
                    GetPosition(context.Stop ?? context.Start));
            }
            else
            {
                // Single type term, return the type directly
                // (possibly wrapped with underlying marker if needed)
                return type;
            }
        }
        
        return null;
    }
    
    private List<IType>? VisitTypeArgs(GoParser.TypeArgsContext context)
    {
        // typeArgs: L_BRACKET typeList COMMA? R_BRACKET
        var typeList = context.typeList();
        if (typeList != null)
        {
            return VisitTypeList(typeList);
        }
        return null;
    }
    
    private List<IType>? VisitTypeList(GoParser.TypeListContext context)
    {
        // typeList: type_ (COMMA type_)*
        var types = new List<IType>();
        var typeContexts = context.type_();
        
        if (typeContexts != null)
        {
            foreach (var typeContext in typeContexts)
            {
                if (Visit(typeContext) is IType type)
                    types.Add(type);
            }
        }
        
        return types;
    }
    
    public override IGoNode? VisitMapType(GoParser.MapTypeContext context)
    {
        var keyType = Visit(context.type_()) as IType
            ?? throw new InvalidOperationException("Invalid map key type");
        var valueType = Visit(context.elementType().type_()) as IType
            ?? throw new InvalidOperationException("Invalid map value type");
            
        return new MapType(
            keyType,
            valueType,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitChannelType(GoParser.ChannelTypeContext context)
    {
        var direction = ChannelDirection.Bidirectional;
        
        if (context.RECEIVE() != null)
        {
            if (context.CHAN() != null && context.RECEIVE().Symbol.TokenIndex < context.CHAN().Symbol.TokenIndex)
            {
                direction = ChannelDirection.ReceiveOnly;
            }
            else
            {
                direction = ChannelDirection.SendOnly;
            }
        }
        
        var elementType = Visit(context.elementType().type_()) as IType
            ?? throw new InvalidOperationException("Invalid channel element type");
            
        return new ChannelType(
            direction,
            elementType,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    // Statement visitors
    public override IGoNode? VisitBlock(GoParser.BlockContext context)
    {
        var statements = new List<IStatement>();
        
        if (context.statementList() != null)
        {
            foreach (var stmt in context.statementList().statement())
            {
                if (Visit(stmt) is IStatement statement)
                    statements.Add(statement);
            }
        }
        
        return new BlockStatement(
            statements,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitStatement(GoParser.StatementContext context)
    {
        if (context.simpleStmt() != null)
            return Visit(context.simpleStmt());
            
        if (context.returnStmt() != null)
            return Visit(context.returnStmt());
            
        if (context.block() != null)
            return Visit(context.block());
            
        if (context.ifStmt() != null)
            return Visit(context.ifStmt());
            
        if (context.forStmt() != null)
            return Visit(context.forStmt());
            
        if (context.switchStmt() != null)
            return Visit(context.switchStmt());
            
        if (context.selectStmt() != null)
            return Visit(context.selectStmt());
            
        if (context.deferStmt() != null)
            return Visit(context.deferStmt());
            
        if (context.goStmt() != null)
            return Visit(context.goStmt());
            
        if (context.breakStmt() != null)
            return new BranchStatement(
                BranchKind.Break,
                context.breakStmt().IDENTIFIER()?.GetText(),
                GetPosition(context.Start),
                GetPosition(context.Stop ?? context.Start));
                
        if (context.continueStmt() != null)
            return new BranchStatement(
                BranchKind.Continue,
                context.continueStmt().IDENTIFIER()?.GetText(),
                GetPosition(context.Start),
                GetPosition(context.Stop ?? context.Start));
                
        if (context.gotoStmt() != null)
            return new BranchStatement(
                BranchKind.Goto,
                context.gotoStmt().IDENTIFIER().GetText(),
                GetPosition(context.Start),
                GetPosition(context.Stop ?? context.Start));
                
        if (context.fallthroughStmt() != null)
            return new FallthroughStatement(
                GetPosition(context.Start),
                GetPosition(context.Stop ?? context.Start));
                
        if (context.labeledStmt() != null)
            return Visit(context.labeledStmt());
            
        if (context.declaration() != null)
        {
            var decl = Visit(context.declaration()) as IDeclaration;
            if (decl != null)
                return new DeclarationStatement(
                    decl,
                    GetPosition(context.Start),
                    GetPosition(context.Stop ?? context.Start));
        }
            
        return null;
    }
    
    public override IGoNode? VisitSimpleStmt(GoParser.SimpleStmtContext context)
    {
        if (context.expressionStmt() != null)
            return Visit(context.expressionStmt());
            
        if (context.assignment() != null)
            return Visit(context.assignment());
            
        if (context.shortVarDecl() != null)
            return Visit(context.shortVarDecl());
            
        if (context.incDecStmt() != null)
            return Visit(context.incDecStmt());
            
        if (context.sendStmt() != null)
            return Visit(context.sendStmt());
            
        return null;
    }
    
    public override IGoNode? VisitLabeledStmt(GoParser.LabeledStmtContext context)
    {
        var label = context.IDENTIFIER().GetText();
        var statement = Visit(context.statement()) as IStatement
            ?? throw new InvalidOperationException("Invalid labeled statement");
            
        return new LabeledStatement(
            label,
            statement,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitRecvStmt(GoParser.RecvStmtContext context)
    {
        // The receive expression is stored in recvExpr
        var recvExpr = Visit(context.recvExpr) as IExpression
            ?? throw new InvalidOperationException("Invalid receive expression");
            
        // Handle receive assignments
        if (context.identifierList() != null)
        {
            var names = context.identifierList().IDENTIFIER()
                .Select(id => id.GetText())
                .ToList();
                
            if (context.DECLARE_ASSIGN() != null)
            {
                // Short variable declaration: x := <-ch
                return new ShortVariableDeclaration(
                    names,
                    new[] { recvExpr },
                    GetPosition(context.Start),
                    GetPosition(context.Stop ?? context.Start));
            }
            else
            {
                // Should not happen based on grammar
                throw new InvalidOperationException("Identifier list without := in receive statement");
            }
        }
        else if (context.expressionList() != null)
        {
            // Regular assignment: x = <-ch
            var left = new List<IExpression>();
            foreach (var expr in context.expressionList().expression())
            {
                if (Visit(expr) is IExpression e)
                    left.Add(e);
            }
            
            return new AssignmentStatement(
                left,
                AssignmentOperator.Assign,
                new[] { recvExpr },
                GetPosition(context.Start),
                GetPosition(context.Stop ?? context.Start));
        }
        else
        {
            // Just receive expression as statement
            return new ExpressionStatement(
                recvExpr,
                GetPosition(context.Start),
                GetPosition(context.Stop ?? context.Start));
        }
    }
    
    public override IGoNode? VisitExpressionStmt(GoParser.ExpressionStmtContext context)
    {
        var expr = Visit(context.expression()) as IExpression
            ?? throw new InvalidOperationException("Invalid expression");
            
        return new ExpressionStatement(
            expr,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitReturnStmt(GoParser.ReturnStmtContext context)
    {
        var values = new List<IExpression>();
        
        if (context.expressionList() != null)
        {
            foreach (var expr in context.expressionList().expression())
            {
                if (Visit(expr) is IExpression value)
                    values.Add(value);
            }
        }
        
        return new ReturnStatement(
            values,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitIfStmt(GoParser.IfStmtContext context)
    {
        IStatement? init = null;
        IExpression? condition = null;
        
        // Check for simple statement initialization
        if (context.simpleStmt() != null)
        {
            init = Visit(context.simpleStmt()) as IStatement;
        }
        
        // Parse the condition
        var expr = context.expression();
        if (expr != null)
        {
            condition = Visit(expr) as IExpression;
        }
        
        if (condition == null)
            throw new InvalidOperationException("If statement must have a condition");
            
        var then = Visit(context.block(0)) as IStatement
            ?? throw new InvalidOperationException("Invalid then block");
            
        IStatement? elseStmt = null;
        if (context.ELSE() != null)
        {
            if (context.ifStmt() != null)
                elseStmt = Visit(context.ifStmt()) as IStatement;
            else if (context.block().Length > 1)
                elseStmt = Visit(context.block(1)) as IStatement;
        }
        
        return new IfStatement(
            init,
            condition,
            then,
            elseStmt,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitForStmt(GoParser.ForStmtContext context)
    {
        var body = Visit(context.block()) as IStatement
            ?? throw new InvalidOperationException("Invalid for loop body");
            
        if (context.rangeClause() != null)
        {
            return VisitRangeClause(context.rangeClause(), body);
        }
        
        if (context.forClause() != null)
        {
            var forClause = context.forClause();
            IStatement? init = null;
            IExpression? condition = null;
            IStatement? post = null;
            
            if (forClause.initStmt != null)
                init = Visit(forClause.initStmt) as IStatement;
            
            if (forClause.expression() != null)
                condition = Visit(forClause.expression()) as IExpression;
                
            if (forClause.postStmt != null)
                post = Visit(forClause.postStmt) as IStatement;
                
            return new ForStatement(
                init,
                condition,
                post,
                body,
                GetPosition(context.Start),
                GetPosition(context.Stop ?? context.Start));
        }
        
        // Simple condition or infinite loop
        IExpression? cond = null;
        if (context.condition() != null && context.condition().expression() != null)
        {
            cond = Visit(context.condition().expression()) as IExpression;
        }
        
        return new ForStatement(
            null,
            cond,
            null,
            body,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    private IStatement VisitRangeClause(GoParser.RangeClauseContext context, IStatement body)
    {
        string? key = null;
        string? value = null;
        var isShortDecl = false;
        
        if (context.identifierList() != null)
        {
            var ids = context.identifierList().IDENTIFIER();
            if (ids.Length > 0)
                key = ids[0].GetText();
            if (ids.Length > 1)
                value = ids[1].GetText();
        }
        
        if (context.DECLARE_ASSIGN() != null)
            isShortDecl = true;
            
        var range = Visit(context.expression()) as IExpression
            ?? throw new InvalidOperationException("Invalid range expression");
            
        return new ForRangeStatement(
            key,
            value,
            isShortDecl,
            range,
            body,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitSwitchStmt(GoParser.SwitchStmtContext context)
    {
        if (context.exprSwitchStmt() != null)
            return Visit(context.exprSwitchStmt());
        if (context.typeSwitchStmt() != null)
            return Visit(context.typeSwitchStmt());
            
        return null;
    }
    
    public override IGoNode? VisitExprSwitchStmt(GoParser.ExprSwitchStmtContext context)
    {
        IStatement? init = null;
        IExpression? expr = null;
        
        if (context.simpleStmt() != null)
            init = Visit(context.simpleStmt()) as IStatement;
            
        if (context.expression() != null)
            expr = Visit(context.expression()) as IExpression;
            
        var cases = new List<CaseClause>();
        foreach (var caseClause in context.exprCaseClause())
        {
            if (Visit(caseClause) is CaseClause cc)
                cases.Add(cc);
        }
        
        return new SwitchStatement(
            init,
            expr,
            cases,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitExprCaseClause(GoParser.ExprCaseClauseContext context)
    {
        var values = new List<IExpression>();
        var exprCase = context.exprSwitchCase();
        var isDefault = exprCase?.DEFAULT() != null;
        
        if (!isDefault && exprCase?.expressionList() != null)
        {
            foreach (var expr in exprCase.expressionList().expression())
            {
                if (Visit(expr) is IExpression value)
                    values.Add(value);
            }
        }
        
        var statements = new List<IStatement>();
        if (context.statementList() != null)
        {
            foreach (var stmt in context.statementList().statement())
            {
                if (Visit(stmt) is IStatement statement)
                    statements.Add(statement);
            }
        }
        
        return new CaseClause(
            isDefault ? null : values,
            statements,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitTypeSwitchStmt(GoParser.TypeSwitchStmtContext context)
    {
        IStatement? init = null;
        IStatement? assign = null;
        
        if (context.simpleStmt() != null)
        {
            var simpleStmt = Visit(context.simpleStmt()) as IStatement;
            
            // Check if this is the type switch guard
            if (context.typeSwitchGuard() != null)
            {
                init = simpleStmt;
                assign = VisitTypeSwitchGuard(context.typeSwitchGuard()) as IStatement;
            }
            else
            {
                assign = simpleStmt;
            }
        }
        else if (context.typeSwitchGuard() != null)
        {
            assign = (IStatement)VisitTypeSwitchGuard(context.typeSwitchGuard());
        }
        
        var cases = new List<TypeCaseClause>();
        foreach (var caseClause in context.typeCaseClause())
        {
            if (Visit(caseClause) is TypeCaseClause tcc)
                cases.Add(tcc);
        }
        
        return new TypeSwitchStatement(
            init,
            assign,
            cases,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitTypeSwitchGuard(GoParser.TypeSwitchGuardContext context)
    {
        // Type switch guard: x.(type) or id := x.(type)
        var primaryExpr = Visit(context.primaryExpr()) as IExpression
            ?? throw new InvalidOperationException("Invalid type switch guard expression");
            
        // Create a type assertion expression with no specific type for type switch
        var typeAssertion = new TypeAssertionExpression(
            primaryExpr,
            null, // No specific type for type switch
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
            
        if (context.IDENTIFIER() != null)
        {
            // id := x.(type)
            return new ShortVariableDeclaration(
                new[] { context.IDENTIFIER().GetText() },
                new[] { typeAssertion },
                GetPosition(context.Start),
                GetPosition(context.Stop ?? context.Start));
        }
        else
        {
            // x.(type)
            return new ExpressionStatement(
                typeAssertion,
                GetPosition(context.Start),
                GetPosition(context.Stop ?? context.Start));
        }
    }
    
    public override IGoNode? VisitTypeCaseClause(GoParser.TypeCaseClauseContext context)
    {
        var types = new List<IType>();
        var typeCase = context.typeSwitchCase();
        var isDefault = typeCase?.DEFAULT() != null;
        
        if (!isDefault && typeCase?.typeList() != null)
        {
            foreach (var type in typeCase.typeList().type_())
            {
                if (Visit(type) is IType t)
                    types.Add(t);
            }
        }
        
        var statements = new List<IStatement>();
        if (context.statementList() != null)
        {
            foreach (var stmt in context.statementList().statement())
            {
                if (Visit(stmt) is IStatement statement)
                    statements.Add(statement);
            }
        }
        
        return new TypeCaseClause(
            types,
            statements,
            isDefault,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitSelectStmt(GoParser.SelectStmtContext context)
    {
        var cases = new List<CommClause>();
        
        foreach (var commClause in context.commClause())
        {
            if (Visit(commClause) is CommClause cc)
                cases.Add(cc);
        }
        
        return new SelectStatement(
            cases,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitCommClause(GoParser.CommClauseContext context)
    {
        IStatement? comm = null;
        var commCase = context.commCase();
        var isDefault = commCase?.DEFAULT() != null;
        
        if (!isDefault && commCase != null)
        {
            comm = Visit(commCase) as IStatement;
        }
        
        var statements = new List<IStatement>();
        if (context.statementList() != null)
        {
            foreach (var stmt in context.statementList().statement())
            {
                if (Visit(stmt) is IStatement statement)
                    statements.Add(statement);
            }
        }
        
        return new CommClause(
            comm,
            statements,
            isDefault,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitCommCase(GoParser.CommCaseContext context)
    {
        if (context.sendStmt() != null)
            return Visit(context.sendStmt());
            
        if (context.recvStmt() != null)
            return Visit(context.recvStmt());
            
        return null;
    }
    
    public override IGoNode? VisitSendStmt(GoParser.SendStmtContext context)
    {
        var channel = Visit(context.expression(0)) as IExpression
            ?? throw new InvalidOperationException("Invalid channel expression");
        var value = Visit(context.expression(1)) as IExpression
            ?? throw new InvalidOperationException("Invalid send value");
            
        return new SendStatement(
            channel,
            value,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitDeferStmt(GoParser.DeferStmtContext context)
    {
        var expr = Visit(context.expression()) as IExpression
            ?? throw new InvalidOperationException("Invalid defer expression");
            
        if (expr is not CallExpression call)
            throw new InvalidOperationException("Defer statement must contain a function call");
            
        return new DeferStatement(
            call,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitGoStmt(GoParser.GoStmtContext context)
    {
        var expr = Visit(context.expression()) as IExpression
            ?? throw new InvalidOperationException("Invalid go expression");
            
        if (expr is not CallExpression call)
            throw new InvalidOperationException("Go statement must contain a function call");
            
        return new GoStatement(
            call,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitAssignment(GoParser.AssignmentContext context)
    {
        var left = new List<IExpression>();
        var right = new List<IExpression>();
        
        foreach (var expr in context.expressionList(0).expression())
        {
            if (Visit(expr) is IExpression e)
                left.Add(e);
        }
        
        foreach (var expr in context.expressionList(1).expression())
        {
            if (Visit(expr) is IExpression e)
                right.Add(e);
        }
        
        var op = context.assign_op().GetText();
        var assignOp = op switch
        {
            "=" => AssignmentOperator.Assign,
            "+=" => AssignmentOperator.AddAssign,
            "-=" => AssignmentOperator.SubAssign,
            "*=" => AssignmentOperator.MulAssign,
            "/=" => AssignmentOperator.DivAssign,
            "%=" => AssignmentOperator.ModAssign,
            "&=" => AssignmentOperator.AndAssign,
            "|=" => AssignmentOperator.OrAssign,
            "^=" => AssignmentOperator.XorAssign,
            "<<=" => AssignmentOperator.ShlAssign,
            ">>=" => AssignmentOperator.ShrAssign,
            "&^=" => AssignmentOperator.AndNotAssign,
            _ => throw new InvalidOperationException($"Unknown assignment operator: {op}")
        };
        
        return new AssignmentStatement(
            left,
            assignOp,
            right,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitShortVarDecl(GoParser.ShortVarDeclContext context)
    {
        var names = context.identifierList().IDENTIFIER()
            .Select(id => id.GetText())
            .ToList();
            
        var values = new List<IExpression>();
        foreach (var expr in context.expressionList().expression())
        {
            if (Visit(expr) is IExpression e)
                values.Add(e);
        }
        
        return new ShortVariableDeclaration(
            names,
            values,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitIncDecStmt(GoParser.IncDecStmtContext context)
    {
        var expr = Visit(context.expression()) as IExpression
            ?? throw new InvalidOperationException("Invalid inc/dec expression");
            
        var isIncrement = context.PLUS_PLUS() != null;
        
        return new IncDecStatement(
            expr,
            isIncrement,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    // Expression visitors
    public override IGoNode? VisitExpression(GoParser.ExpressionContext context)
    {
        if (context.primaryExpr() != null)
            return Visit(context.primaryExpr());
            
        if (context.expression().Length == 2)
        {
            var left = Visit(context.expression(0)) as IExpression
                ?? throw new InvalidOperationException("Invalid left expression");
            var right = Visit(context.expression(1)) as IExpression
                ?? throw new InvalidOperationException("Invalid right expression");
                
            var op = ParseBinaryOperator(context);
            
            return new BinaryExpression(
                left,
                op,
                right,
                GetPosition(context.Start),
                GetPosition(context.Stop ?? context.Start));
        }
        
        // Handle unary expressions
        if (context.expression().Length == 1)
        {
            var operand = Visit(context.expression(0)) as IExpression
                ?? throw new InvalidOperationException("Invalid operand");
                
            var op = ParseUnaryOperator(context);
            if (op != null)
            {
                return new UnaryExpression(
                    op.Value,
                    operand,
                    GetPosition(context.Start),
                    GetPosition(context.Stop ?? context.Start));
            }
        }
        
        return null;
    }
    
    public override IGoNode? VisitPrimaryExpr(GoParser.PrimaryExprContext context)
    {
        // Start with the base expression
        IExpression? result = null;
        
        if (context.operand() != null)
            result = Visit(context.operand()) as IExpression;
        else if (context.conversion() != null)
            result = Visit(context.conversion()) as IExpression;
        else if (context.methodExpr() != null)
            result = Visit(context.methodExpr()) as IExpression;
            
        if (result == null)
            return null;
            
        // Apply postfix operations
        var selectorIndex = 0;
        
        for (int i = 0; i < context.ChildCount; i++)
        {
            var child = context.GetChild(i);
            
            // Skip the base expression
            if (i == 0) continue;
            
            if (child is GoParser.TypeAssertionContext typeAssertion)
            {
                // Type assertion - the grammar rule includes the DOT
                IType? type = null;
                
                if (typeAssertion.type_() != null)
                {
                    type = Visit(typeAssertion.type_()) as IType;
                }
                
                result = new TypeAssertionExpression(
                    result,
                    type,
                    GetPosition(result.Start.Line, result.Start.Column, result.Start.Offset),
                    GetPosition(typeAssertion.Stop ?? typeAssertion.Start));
            }
            else if (child.GetText() == ".")
            {
                // Look ahead for selector expression (DOT IDENTIFIER)
                if (i + 1 < context.ChildCount)
                {
                    var nextChild = context.GetChild(i + 1);
                    if (context.IDENTIFIER() != null && selectorIndex < context.IDENTIFIER().Length)
                    {
                        // Check if this identifier is the next child
                        if (nextChild.GetText() == context.IDENTIFIER(selectorIndex).GetText())
                        {
                            // Selector expression
                            result = new SelectorExpression(
                                result,
                                context.IDENTIFIER(selectorIndex).GetText(),
                                GetPosition(result.Start.Line, result.Start.Column, result.Start.Offset),
                                GetPosition(context.Stop ?? context.Start));
                            selectorIndex++;
                            i++; // Skip the identifier
                        }
                    }
                }
            }
            else if (child is GoParser.IndexContext indexCtx)
            {
                var index = Visit(indexCtx.expression()) as IExpression
                    ?? throw new InvalidOperationException("Invalid index");
                    
                result = new IndexExpression(
                    result,
                    index,
                    GetPosition(result.Start.Line, result.Start.Column, result.Start.Offset),
                    GetPosition(indexCtx.Stop ?? indexCtx.Start));
            }
            else if (child is GoParser.Slice_Context sliceCtx)
            {
                IExpression? low = null;
                IExpression? high = null;
                IExpression? max = null;
                
                var expressions = sliceCtx.expression();
                var colons = sliceCtx.COLON();
                
                if (expressions != null && expressions.Length > 0 && colons != null && colons.Length > 0)
                {
                    // Determine positions based on colon placement
                    // For [:high], the first colon comes before the first expression
                    // For [low:], the first colon comes after the first expression
                    // For [low:high], expressions are on both sides of the colon
                    
                    if (colons.Length == 1)
                    {
                        // Two-part slice [low:high]
                        var colonToken = colons[0];
                        var colonLine = colonToken.Symbol.Line;
                        var colonColumn = colonToken.Symbol.Column;
                        
                        if (expressions.Length == 1)
                        {
                            var expr = expressions[0];
                            // Check if expression comes before or after colon
                            if (expr.Start.Line < colonLine || 
                                (expr.Start.Line == colonLine && expr.Start.Column < colonColumn))
                            {
                                // Expression is before colon: [expr:]
                                low = Visit(expr) as IExpression;
                            }
                            else
                            {
                                // Expression is after colon: [:expr]
                                high = Visit(expr) as IExpression;
                            }
                        }
                        else if (expressions.Length == 2)
                        {
                            // Both sides have expressions: [expr:expr]
                            low = Visit(expressions[0]) as IExpression;
                            high = Visit(expressions[1]) as IExpression;
                        }
                    }
                    else if (colons.Length == 2)
                    {
                        // Three-part slice [low:high:max]
                        if (expressions.Length > 0) low = Visit(expressions[0]) as IExpression;
                        if (expressions.Length > 1) high = Visit(expressions[1]) as IExpression;
                        if (expressions.Length > 2) max = Visit(expressions[2]) as IExpression;
                    }
                }
                
                result = new SliceExpression(
                    result,
                    low,
                    high,
                    max,
                    GetPosition(result.Start.Line, result.Start.Column, result.Start.Offset),
                    GetPosition(sliceCtx.Stop ?? sliceCtx.Start));
            }
            else if (child is GoParser.ArgumentsContext argCtx)
            {
                result = VisitCall(result, argCtx);
            }
        }
        
        return result;
    }
    
    public override IGoNode? VisitOperand(GoParser.OperandContext context)
    {
        if (context.literal() != null)
            return Visit(context.literal());
            
        if (context.operandName() != null)
        {
            var name = context.operandName().GetText();
            
            // Check for boolean literals (predeclared identifiers in Go)
            if (name == "true" || name == "false")
            {
                return new LiteralExpression(
                    LiteralKind.Bool,
                    name,
                    GetPosition(context.Start),
                    GetPosition(context.Stop ?? context.Start));
            }
            
            return new IdentifierExpression(
                name,
                GetPosition(context.Start),
                GetPosition(context.Stop ?? context.Start));
        }
        
        if (context.expression() != null)
            return Visit(context.expression());
            
        return null;
    }
    
    public override IGoNode? VisitLiteral(GoParser.LiteralContext context)
    {
        if (context.basicLit() != null)
        {
            var basicLit = context.basicLit();
            
            if (basicLit.NIL_LIT() != null)
            {
                return new LiteralExpression(
                    LiteralKind.Nil,
                    "nil",
                    GetPosition(context.Start),
                    GetPosition(context.Stop ?? context.Start));
            }
            
            if (basicLit.integer() != null)
            {
                var integer = basicLit.integer();
                // Check if it's an imaginary literal
                if (integer.IMAGINARY_LIT() != null)
                {
                    return new LiteralExpression(
                        LiteralKind.Imaginary,
                        integer.IMAGINARY_LIT().GetText(),
                        GetPosition(context.Start),
                        GetPosition(context.Stop ?? context.Start));
                }
                // Check if it's a rune literal
                if (integer.RUNE_LIT() != null)
                {
                    var runeText = integer.RUNE_LIT().GetText();
                    // Strip quotes from rune literal
                    if (runeText.Length >= 2 && runeText[0] == '\'' && runeText[^1] == '\'')
                    {
                        runeText = runeText[1..^1];
                    }
                    return new LiteralExpression(
                        LiteralKind.Rune,
                        runeText,
                        GetPosition(context.Start),
                        GetPosition(context.Stop ?? context.Start));
                }
                
                return new LiteralExpression(
                    LiteralKind.Int,
                    integer.GetText(),
                    GetPosition(context.Start),
                    GetPosition(context.Stop ?? context.Start));
            }
            
            if (basicLit.string_() != null)
            {
                var stringText = basicLit.string_().GetText();
                // Strip quotes from string literal
                if ((stringText.Length >= 2 && stringText[0] == '"' && stringText[^1] == '"') ||
                    (stringText.Length >= 2 && stringText[0] == '`' && stringText[^1] == '`'))
                {
                    stringText = stringText[1..^1];
                }
                return new LiteralExpression(
                    LiteralKind.String,
                    stringText,
                    GetPosition(context.Start),
                    GetPosition(context.Stop ?? context.Start));
            }
            
            if (basicLit.FLOAT_LIT() != null)
            {
                return new LiteralExpression(
                    LiteralKind.Float,
                    basicLit.FLOAT_LIT().GetText(),
                    GetPosition(context.Start),
                    GetPosition(context.Stop ?? context.Start));
            }
        }
        
        if (context.compositeLit() != null)
            return Visit(context.compositeLit());
            
        if (context.functionLit() != null)
            return Visit(context.functionLit());
        
        return null;
    }
    
    public override IGoNode? VisitLiteralType(GoParser.LiteralTypeContext context)
    {
        if (context.structType() != null)
            return Visit(context.structType());
        if (context.arrayType() != null)
            return Visit(context.arrayType());
        if (context.sliceType() != null)
            return Visit(context.sliceType());
        if (context.mapType() != null)
            return Visit(context.mapType());
        if (context.typeName() != null)
            return Visit(context.typeName());
            
        // Handle [...] array type (ellipsis array)
        if (context.ELLIPSIS() != null && context.elementType() != null)
        {
            var elementType = Visit(context.elementType().type_()) as IType
                ?? throw new InvalidOperationException("Invalid element type");
                
            // Create an ellipsis expression to represent [...]
            var ellipsis = new EllipsisExpression(
                null,
                GetPosition(context.ELLIPSIS().Symbol),
                GetPosition(context.ELLIPSIS().Symbol));
                
            return new ArrayType(
                ellipsis,
                elementType,
                GetPosition(context.Start),
                GetPosition(context.Stop ?? context.Start));
        }
        
        return null;
    }
    
    public override IGoNode? VisitCompositeLit(GoParser.CompositeLitContext context)
    {
        IType? type = null;
        
        if (context.literalType() != null)
        {
            type = Visit(context.literalType()) as IType;
        }
        
        var elements = new List<IExpression>();
        if (context.literalValue() != null)
        {
            elements = ParseLiteralValue(context.literalValue());
        }
        
        return new CompositeLiteral(
            type,
            elements,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    private List<IExpression> ParseLiteralValue(GoParser.LiteralValueContext context)
    {
        var elements = new List<IExpression>();
        
        if (context.elementList() != null)
        {
            foreach (var element in context.elementList().keyedElement())
            {
                if (Visit(element) is IExpression expr)
                    elements.Add(expr);
            }
        }
        
        return elements;
    }
    
    public override IGoNode? VisitKeyedElement(GoParser.KeyedElementContext context)
    {
        IExpression? key = null;
        IExpression value;
        
        if (context.key() != null)
        {
            if (context.key().expression() != null)
                key = Visit(context.key().expression()) as IExpression;
            else if (context.key().literalValue() != null)
            {
                // Handle composite literal as key
                var innerElements = ParseLiteralValue(context.key().literalValue());
                key = new CompositeLiteral(
                    null,
                    innerElements,
                    GetPosition(context.key().Start),
                    GetPosition(context.key().Stop ?? context.key().Start));
            }
        }
        
        if (context.element().expression() != null)
        {
            value = Visit(context.element().expression()) as IExpression
                ?? throw new InvalidOperationException("Invalid element value");
        }
        else if (context.element().literalValue() != null)
        {
            var innerElements = ParseLiteralValue(context.element().literalValue());
            value = new CompositeLiteral(
                null,
                innerElements,
                GetPosition(context.element().Start),
                GetPosition(context.element().Stop ?? context.element().Start));
        }
        else
        {
            throw new InvalidOperationException("Invalid element in composite literal");
        }
        
        return new KeyedElement(
            key,
            value,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitFunctionLit(GoParser.FunctionLitContext context)
    {
        var signature = context.signature();
        var parameters = ParseParameters(signature.parameters());
        var returnParams = signature.result() != null 
            ? ParseResult(signature.result()) 
            : null;
            
        var body = Visit(context.block()) as BlockStatement
            ?? throw new InvalidOperationException("Invalid function literal body");
            
        return new FunctionLiteral(
            null, // No explicit function type in literal
            parameters,
            returnParams,
            body,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitSlice_(GoParser.Slice_Context context)
    {
        IExpression? low = null;
        IExpression? high = null;
        IExpression? max = null;
        
        var expressions = context.expression();
        if (expressions.Length > 0 && expressions[0] != null)
            low = Visit(expressions[0]) as IExpression;
        if (expressions.Length > 1 && expressions[1] != null)
            high = Visit(expressions[1]) as IExpression;
        if (expressions.Length > 2 && expressions[2] != null)
            max = Visit(expressions[2]) as IExpression;
            
        // Note: This returns null as the slice needs to be combined with the expression
        // The actual SliceExpression is created in VisitPrimaryExpr
        return null;
    }
    
    public override IGoNode? VisitTypeAssertion(GoParser.TypeAssertionContext context)
    {
        IType? type = null;
        
        if (context.type_() != null)
        {
            type = Visit(context.type_()) as IType;
        }
        
        // Note: This returns null as the type assertion needs to be combined with the expression
        // The actual TypeAssertionExpression is created in VisitPrimaryExpr
        return null;
    }
    
    public override IGoNode? VisitConversion(GoParser.ConversionContext context)
    {
        var type = Visit(context.type_()) as IType
            ?? throw new InvalidOperationException("Invalid conversion type");
            
        var args = new List<IExpression>();
        if (context.expression() != null)
        {
            if (Visit(context.expression()) is IExpression expr)
                args.Add(expr);
        }
        
        if (args.Count != 1)
            throw new InvalidOperationException("Type conversion must have exactly one argument");
            
        return new ConversionExpression(
            type,
            args[0],
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitDeclaration(GoParser.DeclarationContext context)
    {
        if (context.constDecl() != null)
            return Visit(context.constDecl());
        if (context.typeDecl() != null)
            return Visit(context.typeDecl());
        if (context.varDecl() != null)
            return Visit(context.varDecl());
            
        return null;
    }
    
    public override IGoNode? VisitConstDecl(GoParser.ConstDeclContext context)
    {
        var specs = new List<ConstSpec>();
        
        foreach (var spec in context.constSpec())
        {
            if (Visit(spec) is ConstSpec constSpec)
                specs.Add(constSpec);
        }
        
        return new ConstDeclaration(
            specs,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitConstSpec(GoParser.ConstSpecContext context)
    {
        var names = context.identifierList().IDENTIFIER()
            .Select(id => id.GetText())
            .ToList();
            
        IType? type = null;
        if (context.type_() != null)
            type = Visit(context.type_()) as IType;
            
        var values = new List<IExpression>();
        if (context.expressionList() != null)
        {
            foreach (var expr in context.expressionList().expression())
            {
                if (Visit(expr) is IExpression e)
                    values.Add(e);
            }
        }
        
        return new ConstSpec(
            names,
            type,
            values,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitTypeDecl(GoParser.TypeDeclContext context)
    {
        var specs = new List<TypeSpec>();
        
        foreach (var spec in context.typeSpec())
        {
            if (Visit(spec) is TypeSpec typeSpec)
                specs.Add(typeSpec);
        }
        
        if (specs.Count == 0)
            return null;
            
        return new TypeDeclaration(
            specs,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitTypeSpec(GoParser.TypeSpecContext context)
    {
        if (context.aliasDecl() != null)
        {
            var aliasDecl = context.aliasDecl();
            var name = aliasDecl.IDENTIFIER().GetText();
            var type = Visit(aliasDecl.type_()) as IType
                ?? throw new InvalidOperationException("Invalid type in alias declaration");
            var typeParams = aliasDecl.typeParameters() != null
                ? ParseTypeParameters(aliasDecl.typeParameters())
                : null;
                
            var spec = new TypeSpec(
                name,
                typeParams,
                type,
                GetPosition(aliasDecl.Start),
                GetPosition(aliasDecl.Stop ?? aliasDecl.Start));
                
            return spec;
        }
        else if (context.typeDef() != null)
        {
            var typeDef = context.typeDef();
            var name = typeDef.IDENTIFIER().GetText();
            var type = Visit(typeDef.type_()) as IType
                ?? throw new InvalidOperationException("Invalid type in type definition");
            var typeParams = typeDef.typeParameters() != null
                ? ParseTypeParameters(typeDef.typeParameters())
                : null;
                
            var spec = new TypeSpec(
                name,
                typeParams,
                type,
                GetPosition(typeDef.Start),
                GetPosition(typeDef.Stop ?? typeDef.Start));
                
            return spec;
        }
        
        throw new InvalidOperationException("TypeSpec must have either aliasDecl or typeDef");
    }
    
    public override IGoNode? VisitVarDecl(GoParser.VarDeclContext context)
    {
        var specs = new List<VariableSpec>();
        
        foreach (var spec in context.varSpec())
        {
            if (Visit(spec) is VariableSpec varSpec)
                specs.Add(varSpec);
        }
        
        return new VariableDeclaration(
            false, // isConstant - this is a var declaration, not const
            specs,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    public override IGoNode? VisitVarSpec(GoParser.VarSpecContext context)
    {
        var names = context.identifierList().IDENTIFIER()
            .Select(id => id.GetText())
            .ToList();
            
        IType? type = null;
        if (context.type_() != null)
            type = Visit(context.type_()) as IType;
            
        var values = new List<IExpression>();
        if (context.expressionList() != null)
        {
            foreach (var expr in context.expressionList().expression())
            {
                if (Visit(expr) is IExpression e)
                    values.Add(e);
            }
        }
        
        return new VariableSpec(
            names,
            type,
            values,
            GetPosition(context.Start),
            GetPosition(context.Stop ?? context.Start));
    }
    
    private IExpression VisitCall(IExpression function, GoParser.ArgumentsContext arguments)
    {
        var args = new List<IExpression>();
        var typeArgs = new List<IType>();
        
        // Check if the function expression is an identifier with type arguments
        if (function is IdentifierExpression identifier)
        {
            // Look back to see if there were type arguments in the operand
            // This requires us to track type arguments from the operand context
            // For now, we'll handle type arguments as part of the CallExpression
        }
        
        if (arguments.expressionList() != null)
        {
            foreach (var expr in arguments.expressionList().expression())
            {
                if (Visit(expr) is IExpression arg)
                    args.Add(arg);
            }
        }
        
        var hasEllipsis = arguments.ELLIPSIS() != null;
        
        return new CallExpression(
            function,
            args,
            typeArgs.Count > 0 ? typeArgs : null,
            hasEllipsis,
            GetPosition(arguments.Start),
            GetPosition(arguments.Stop ?? arguments.Start));
    }
    
    private BinaryOperator ParseBinaryOperator(GoParser.ExpressionContext context)
    {
        // Check the operator token between expressions
        var children = context.children;
        foreach (var child in children)
        {
            if (child is ITerminalNode terminal)
            {
                var text = terminal.GetText();
                switch (text)
                {
                    case "+": return BinaryOperator.Add;
                    case "-": return BinaryOperator.Subtract;
                    case "*": return BinaryOperator.Multiply;
                    case "/": return BinaryOperator.Divide;
                    case "%": return BinaryOperator.Modulo;
                    case "&": return BinaryOperator.BitwiseAnd;
                    case "|": return BinaryOperator.BitwiseOr;
                    case "^": return BinaryOperator.BitwiseXor;
                    case "<<": return BinaryOperator.LeftShift;
                    case ">>": return BinaryOperator.RightShift;
                    case "&^": return BinaryOperator.AndNot;
                    case "==": return BinaryOperator.Equal;
                    case "!=": return BinaryOperator.NotEqual;
                    case "<": return BinaryOperator.Less;
                    case "<=": return BinaryOperator.LessOrEqual;
                    case ">": return BinaryOperator.Greater;
                    case ">=": return BinaryOperator.GreaterOrEqual;
                    case "&&": return BinaryOperator.LogicalAnd;
                    case "||": return BinaryOperator.LogicalOr;
                }
            }
        }
        
        throw new InvalidOperationException("Unknown binary operator");
    }
    
    private UnaryOperator? ParseUnaryOperator(GoParser.ExpressionContext context)
    {
        // Check for unary operator at the beginning
        if (context.children.Count > 0 && context.children[0] is ITerminalNode terminal)
        {
            var text = terminal.GetText();
            switch (text)
            {
                case "+": return UnaryOperator.Plus;
                case "-": return UnaryOperator.Minus;
                case "!": return UnaryOperator.Not;
                case "^": return UnaryOperator.Complement;
                case "*": return UnaryOperator.Dereference;
                case "&": return UnaryOperator.Address;
                case "<-": return UnaryOperator.Receive;
            }
        }
        
        return null;
    }
    
    private Position GetPosition(IToken token)
    {
        // ANTLR uses 0-based columns, but we want 1-based for consistency with Go tooling
        return new Position(token.Line, token.Column + 1, token.StartIndex);
    }
    
    private Position GetPosition(int line, int column, int offset)
    {
        return new Position(line, column, offset);
    }
    
    private IReadOnlyList<TypeParameter>? ParseTypeParameters(GoParser.TypeParametersContext context)
    {
        var parameters = new List<TypeParameter>();
        
        foreach (var paramDecl in context.typeParameterDecl())
        {
            foreach (var id in paramDecl.identifierList().IDENTIFIER())
            {
                var name = id.GetText();
                IType? constraint = null;
                
                if (paramDecl.typeElement() != null)
                {
                    constraint = Visit(paramDecl.typeElement()) as IType;
                }
                
                parameters.Add(new TypeParameter(
                    name,
                    constraint,
                    GetPosition(id.Symbol),
                    GetPosition(paramDecl.Stop ?? paramDecl.Start)));
            }
        }
        
        return parameters.Count > 0 ? parameters : null;
    }
}