using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using IronGo.AST;

namespace IronGo.Serialization;

/// <summary>
/// Custom JSON converter for Go AST nodes
/// </summary>
public class AstJsonConverter : JsonConverter<IGoNode>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(IGoNode).IsAssignableFrom(typeToConvert);
    }

    public override IGoNode? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Deserialization of AST nodes is not supported");
    }

    public override void Write(Utf8JsonWriter writer, IGoNode value, JsonSerializerOptions options)
    {
        var nodeWriter = new AstJsonWriter(writer, options);
        value.Accept(nodeWriter);
    }
}

/// <summary>
/// Visitor that writes AST nodes as JSON
/// </summary>
internal class AstJsonWriter : GoAstVisitorBase
{
    private readonly Utf8JsonWriter _writer;
    private readonly JsonSerializerOptions _options;

    public AstJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        _writer = writer;
        _options = options;
    }

    private void WriteNodeStart(string nodeType, IGoNode node)
    {
        _writer.WriteStartObject();
        _writer.WriteString("Type", nodeType);
        WritePosition("Start", node.Start);
        WritePosition("End", node.End);
    }

    private void WriteNodeEnd()
    {
        _writer.WriteEndObject();
    }

    private void WritePosition(string name, Position position)
    {
        _writer.WritePropertyName(name);
        _writer.WriteStartObject();
        _writer.WriteNumber("Line", position.Line);
        _writer.WriteNumber("Column", position.Column);
        _writer.WriteNumber("Offset", position.Offset);
        _writer.WriteEndObject();
    }

    private void WriteArray<T>(string name, System.Collections.Generic.IReadOnlyList<T> items) where T : IGoNode
    {
        _writer.WritePropertyName(name);
        _writer.WriteStartArray();
        foreach (var item in items)
        {
            item.Accept(this);
        }
        _writer.WriteEndArray();
    }

    public override void VisitSourceFile(SourceFile node)
    {
        WriteNodeStart("SourceFile", node);
        
        _writer.WritePropertyName("Package");
        if (node.Package != null)
            node.Package.Accept(this);
        else
            _writer.WriteNullValue();
        
        WriteArray("Imports", node.Imports);
        WriteArray("Declarations", node.Declarations);
        WriteArray("Comments", node.Comments);
        
        WriteNodeEnd();
    }

    public override void VisitPackageDeclaration(PackageDeclaration node)
    {
        WriteNodeStart("PackageDeclaration", node);
        _writer.WriteString("Name", node.Name);
        WriteNodeEnd();
    }

    public override void VisitImportDeclaration(ImportDeclaration node)
    {
        WriteNodeStart("ImportDeclaration", node);
        WriteArray("Specs", node.Specs);
        WriteNodeEnd();
    }

    public override void VisitImportSpec(ImportSpec node)
    {
        WriteNodeStart("ImportSpec", node);
        if (node.Alias != null)
            _writer.WriteString("Alias", node.Alias);
        _writer.WriteString("Path", node.Path);
        WriteNodeEnd();
    }

    public override void VisitFunctionDeclaration(FunctionDeclaration node)
    {
        WriteNodeStart("FunctionDeclaration", node);
        _writer.WriteString("Name", node.Name);
        
        if (node.TypeParameters != null)
            WriteArray("TypeParameters", node.TypeParameters);
            
        WriteArray("Parameters", node.Parameters);
        
        if (node.ReturnParameters != null)
            WriteArray("ReturnParameters", node.ReturnParameters);
            
        if (node.Body != null)
        {
            _writer.WritePropertyName("Body");
            node.Body.Accept(this);
        }
        
        WriteNodeEnd();
    }

    public override void VisitMethodDeclaration(MethodDeclaration node)
    {
        WriteNodeStart("MethodDeclaration", node);
        
        _writer.WritePropertyName("Receiver");
        node.Receiver.Accept(this);
        
        _writer.WriteString("Name", node.Name);
        
        if (node.TypeParameters != null)
            WriteArray("TypeParameters", node.TypeParameters);
            
        WriteArray("Parameters", node.Parameters);
        
        if (node.ReturnParameters != null)
            WriteArray("ReturnParameters", node.ReturnParameters);
            
        if (node.Body != null)
        {
            _writer.WritePropertyName("Body");
            node.Body.Accept(this);
        }
        
        WriteNodeEnd();
    }

    public override void VisitParameter(Parameter node)
    {
        WriteNodeStart("Parameter", node);
        
        _writer.WritePropertyName("Names");
        _writer.WriteStartArray();
        foreach (var name in node.Names)
            _writer.WriteStringValue(name);
        _writer.WriteEndArray();
        
        _writer.WritePropertyName("Type");
        node.Type.Accept(this);
        
        _writer.WriteBoolean("IsVariadic", node.IsVariadic);
        
        WriteNodeEnd();
    }

    public override void VisitTypeParameter(TypeParameter node)
    {
        WriteNodeStart("TypeParameter", node);
        _writer.WriteString("Name", node.Name);
        
        if (node.Constraint != null)
        {
            _writer.WritePropertyName("Constraint");
            node.Constraint.Accept(this);
        }
        
        WriteNodeEnd();
    }

    public override void VisitIdentifierType(IdentifierType node)
    {
        WriteNodeStart("IdentifierType", node);
        _writer.WriteString("Name", node.Name);
        if (node.Package != null)
            _writer.WriteString("Package", node.Package);
        if (node.TypeArguments != null && node.TypeArguments.Count > 0)
        {
            _writer.WritePropertyName("TypeArguments");
            _writer.WriteStartArray();
            foreach (var arg in node.TypeArguments)
                arg.Accept(this);
            _writer.WriteEndArray();
        }
        WriteNodeEnd();
    }

    public override void VisitPointerType(PointerType node)
    {
        WriteNodeStart("PointerType", node);
        _writer.WritePropertyName("ElementType");
        node.ElementType.Accept(this);
        WriteNodeEnd();
    }

    public override void VisitArrayType(ArrayType node)
    {
        WriteNodeStart("ArrayType", node);
        
        if (node.Length != null)
        {
            _writer.WritePropertyName("Length");
            node.Length.Accept(this);
        }
        
        _writer.WritePropertyName("ElementType");
        node.ElementType.Accept(this);
        
        WriteNodeEnd();
    }

    public override void VisitSliceType(SliceType node)
    {
        WriteNodeStart("SliceType", node);
        _writer.WritePropertyName("ElementType");
        node.ElementType.Accept(this);
        WriteNodeEnd();
    }

    public override void VisitMapType(MapType node)
    {
        WriteNodeStart("MapType", node);
        
        _writer.WritePropertyName("KeyType");
        node.KeyType.Accept(this);
        
        _writer.WritePropertyName("ValueType");
        node.ValueType.Accept(this);
        
        WriteNodeEnd();
    }

    public override void VisitChannelType(ChannelType node)
    {
        WriteNodeStart("ChannelType", node);
        _writer.WriteString("Direction", node.Direction.ToString());
        
        _writer.WritePropertyName("ElementType");
        node.ElementType.Accept(this);
        
        WriteNodeEnd();
    }

    public override void VisitFunctionType(FunctionType node)
    {
        WriteNodeStart("FunctionType", node);
        
        if (node.TypeParameters != null)
            WriteArray("TypeParameters", node.TypeParameters);
            
        WriteArray("Parameters", node.Parameters);
        
        if (node.ReturnParameters != null)
            WriteArray("ReturnParameters", node.ReturnParameters);
            
        WriteNodeEnd();
    }

    public override void VisitInterfaceType(InterfaceType node)
    {
        WriteNodeStart("InterfaceType", node);
        WriteArray("Methods", node.Methods);
        if (node.TypeElements.Count > 0)
            WriteArray("TypeElements", node.TypeElements);
        WriteNodeEnd();
    }

    public override void VisitStructType(StructType node)
    {
        WriteNodeStart("StructType", node);
        WriteArray("Fields", node.Fields);
        WriteNodeEnd();
    }
    
    public override void VisitTypeDeclaration(TypeDeclaration node)
    {
        WriteNodeStart("TypeDeclaration", node);
        WriteArray("Specs", node.Specs);
        WriteNodeEnd();
    }
    
    public override void VisitTypeSpec(TypeSpec node)
    {
        WriteNodeStart("TypeSpec", node);
        _writer.WriteString("Name", node.Name);
        
        if (node.TypeParameters != null)
            WriteArray("TypeParameters", node.TypeParameters);
            
        _writer.WritePropertyName("Type");
        node.Type.Accept(this);
        
        WriteNodeEnd();
    }
    
    public override void VisitVariableDeclaration(VariableDeclaration node)
    {
        WriteNodeStart("VariableDeclaration", node);
        WriteArray("Specs", node.Specs);
        WriteNodeEnd();
    }
    
    public override void VisitVariableSpec(VariableSpec node)
    {
        WriteNodeStart("VariableSpec", node);
        
        _writer.WritePropertyName("Names");
        _writer.WriteStartArray();
        foreach (var name in node.Names)
            _writer.WriteStringValue(name);
        _writer.WriteEndArray();
        
        if (node.Type != null)
        {
            _writer.WritePropertyName("Type");
            node.Type.Accept(this);
        }
        
        if (node.Values != null)
            WriteArray("Values", node.Values);
            
        WriteNodeEnd();
    }
    
    public override void VisitConstDeclaration(ConstDeclaration node)
    {
        WriteNodeStart("ConstDeclaration", node);
        WriteArray("Specs", node.Specs);
        WriteNodeEnd();
    }
    
    public override void VisitConstSpec(ConstSpec node)
    {
        WriteNodeStart("ConstSpec", node);
        
        _writer.WritePropertyName("Names");
        _writer.WriteStartArray();
        foreach (var name in node.Names)
            _writer.WriteStringValue(name);
        _writer.WriteEndArray();
        
        if (node.Type != null)
        {
            _writer.WritePropertyName("Type");
            node.Type.Accept(this);
        }
        
        if (node.Values != null)
            WriteArray("Values", node.Values);
            
        WriteNodeEnd();
    }

    public override void VisitFieldDeclaration(FieldDeclaration node)
    {
        WriteNodeStart("FieldDeclaration", node);
        
        _writer.WritePropertyName("Names");
        _writer.WriteStartArray();
        foreach (var name in node.Names)
            _writer.WriteStringValue(name);
        _writer.WriteEndArray();
        
        _writer.WritePropertyName("Type");
        node.Type.Accept(this);
        
        if (node.Tag != null)
            _writer.WriteString("Tag", node.Tag);
            
        _writer.WriteBoolean("IsEmbedded", node.IsEmbedded);
        
        WriteNodeEnd();
    }

    // Statements
    public override void VisitBlockStatement(BlockStatement node)
    {
        WriteNodeStart("BlockStatement", node);
        WriteArray("Statements", node.Statements);
        WriteNodeEnd();
    }

    public override void VisitExpressionStatement(ExpressionStatement node)
    {
        WriteNodeStart("ExpressionStatement", node);
        _writer.WritePropertyName("Expression");
        node.Expression.Accept(this);
        WriteNodeEnd();
    }

    public override void VisitAssignmentStatement(AssignmentStatement node)
    {
        WriteNodeStart("AssignmentStatement", node);
        
        WriteArray("Left", node.Left);
        _writer.WriteString("Operator", node.Operator.ToString());
        WriteArray("Right", node.Right);
        
        WriteNodeEnd();
    }

    public override void VisitIfStatement(IfStatement node)
    {
        WriteNodeStart("IfStatement", node);
        
        if (node.Init != null)
        {
            _writer.WritePropertyName("Init");
            node.Init.Accept(this);
        }
        
        _writer.WritePropertyName("Condition");
        node.Condition.Accept(this);
        
        _writer.WritePropertyName("Then");
        node.Then.Accept(this);
        
        if (node.Else != null)
        {
            _writer.WritePropertyName("Else");
            node.Else.Accept(this);
        }
        
        WriteNodeEnd();
    }

    public override void VisitForStatement(ForStatement node)
    {
        WriteNodeStart("ForStatement", node);
        
        if (node.Init != null)
        {
            _writer.WritePropertyName("Init");
            node.Init.Accept(this);
        }
        
        if (node.Condition != null)
        {
            _writer.WritePropertyName("Condition");
            node.Condition.Accept(this);
        }
        
        if (node.Post != null)
        {
            _writer.WritePropertyName("Post");
            node.Post.Accept(this);
        }
        
        _writer.WritePropertyName("Body");
        node.Body.Accept(this);
        
        WriteNodeEnd();
    }

    public override void VisitReturnStatement(ReturnStatement node)
    {
        WriteNodeStart("ReturnStatement", node);
        WriteArray("Results", node.Results);
        WriteNodeEnd();
    }

    public override void VisitBranchStatement(BranchStatement node)
    {
        WriteNodeStart("BranchStatement", node);
        _writer.WriteString("Kind", node.Kind.ToString());
        
        if (node.Label != null)
            _writer.WriteString("Label", node.Label);
            
        WriteNodeEnd();
    }
    
    public override void VisitShortVariableDeclaration(ShortVariableDeclaration node)
    {
        WriteNodeStart("ShortVariableDeclaration", node);
        
        _writer.WritePropertyName("Names");
        _writer.WriteStartArray();
        foreach (var name in node.Names)
            _writer.WriteStringValue(name);
        _writer.WriteEndArray();
        
        WriteArray("Values", node.Values);
        
        WriteNodeEnd();
    }

    // Expressions
    public override void VisitIdentifierExpression(IdentifierExpression node)
    {
        WriteNodeStart("IdentifierExpression", node);
        _writer.WriteString("Name", node.Name);
        WriteNodeEnd();
    }

    public override void VisitLiteralExpression(LiteralExpression node)
    {
        WriteNodeStart("LiteralExpression", node);
        _writer.WriteString("Kind", node.Kind.ToString());
        _writer.WriteString("Value", node.Value);
        WriteNodeEnd();
    }

    public override void VisitBinaryExpression(BinaryExpression node)
    {
        WriteNodeStart("BinaryExpression", node);
        
        _writer.WritePropertyName("Left");
        node.Left.Accept(this);
        
        _writer.WriteString("Operator", node.Operator.ToString());
        
        _writer.WritePropertyName("Right");
        node.Right.Accept(this);
        
        WriteNodeEnd();
    }

    public override void VisitUnaryExpression(UnaryExpression node)
    {
        WriteNodeStart("UnaryExpression", node);
        
        _writer.WriteString("Operator", node.Operator.ToString());
        
        _writer.WritePropertyName("Operand");
        node.Operand.Accept(this);
        
        WriteNodeEnd();
    }

    public override void VisitCallExpression(CallExpression node)
    {
        WriteNodeStart("CallExpression", node);
        
        _writer.WritePropertyName("Function");
        node.Function.Accept(this);
        
        if (node.TypeArguments != null && node.TypeArguments.Count > 0)
            WriteArray("TypeArguments", node.TypeArguments);
        
        WriteArray("Arguments", node.Arguments);
        
        _writer.WriteBoolean("HasEllipsis", node.HasEllipsis);
        
        WriteNodeEnd();
    }

    public override void VisitSelectorExpression(SelectorExpression node)
    {
        WriteNodeStart("SelectorExpression", node);
        
        _writer.WritePropertyName("X");
        node.X.Accept(this);
        
        _writer.WriteString("Selector", node.Selector);
        
        WriteNodeEnd();
    }

    public override void VisitIndexExpression(IndexExpression node)
    {
        WriteNodeStart("IndexExpression", node);
        
        _writer.WritePropertyName("X");
        node.X.Accept(this);
        
        _writer.WritePropertyName("Index");
        node.Index.Accept(this);
        
        WriteNodeEnd();
    }

    public override void VisitComment(Comment node)
    {
        WriteNodeStart("Comment", node);
        _writer.WriteString("Text", node.Text);
        _writer.WriteBoolean("IsLineComment", node.IsLineComment);
        WriteNodeEnd();
    }
    
    public override void VisitSliceExpression(SliceExpression node)
    {
        WriteNodeStart("SliceExpression", node);
        
        _writer.WritePropertyName("X");
        node.X.Accept(this);
        
        if (node.Low != null)
        {
            _writer.WritePropertyName("Low");
            node.Low.Accept(this);
        }
        
        if (node.High != null)
        {
            _writer.WritePropertyName("High");
            node.High.Accept(this);
        }
        
        if (node.Max != null)
        {
            _writer.WritePropertyName("Max");
            node.Max.Accept(this);
        }
        
        WriteNodeEnd();
    }
    
    public override void VisitTypeAssertionExpression(TypeAssertionExpression node)
    {
        WriteNodeStart("TypeAssertionExpression", node);
        
        _writer.WritePropertyName("X");
        node.X.Accept(this);
        
        if (node.Type != null)
        {
            _writer.WritePropertyName("Type");
            node.Type.Accept(this);
        }
        
        WriteNodeEnd();
    }
    
    public override void VisitCompositeLiteral(CompositeLiteral node)
    {
        WriteNodeStart("CompositeLiteral", node);
        
        _writer.WritePropertyName("Type");
        node.Type.Accept(this);
        
        WriteArray("Elements", node.Elements);
        
        WriteNodeEnd();
    }
    
    public override void VisitKeyedElement(KeyedElement node)
    {
        WriteNodeStart("KeyedElement", node);
        
        if (node.Key != null)
        {
            _writer.WritePropertyName("Key");
            node.Key.Accept(this);
        }
        
        _writer.WritePropertyName("Value");
        node.Value.Accept(this);
        
        WriteNodeEnd();
    }
    
    public override void VisitFunctionLiteral(FunctionLiteral node)
    {
        WriteNodeStart("FunctionLiteral", node);
        
        if (node.Type != null)
        {
            _writer.WritePropertyName("Type");
            node.Type.Accept(this);
        }
            
        WriteArray("Parameters", node.Parameters);
        
        if (node.ReturnParameters != null)
            WriteArray("ReturnParameters", node.ReturnParameters);
            
        _writer.WritePropertyName("Body");
        node.Body.Accept(this);
        
        WriteNodeEnd();
    }
    
    public override void VisitDeclarationStatement(DeclarationStatement node)
    {
        WriteNodeStart("DeclarationStatement", node);
        
        _writer.WritePropertyName("Declaration");
        node.Declaration.Accept(this);
        
        WriteNodeEnd();
    }
    
    public override void VisitGoStatement(GoStatement node)
    {
        WriteNodeStart("GoStatement", node);
        
        _writer.WritePropertyName("Call");
        node.Call.Accept(this);
        
        WriteNodeEnd();
    }
    
    public override void VisitDeferStatement(DeferStatement node)
    {
        WriteNodeStart("DeferStatement", node);
        
        _writer.WritePropertyName("Call");
        node.Call.Accept(this);
        
        WriteNodeEnd();
    }
    
    public override void VisitSwitchStatement(SwitchStatement node)
    {
        WriteNodeStart("SwitchStatement", node);
        
        if (node.Init != null)
        {
            _writer.WritePropertyName("Init");
            node.Init.Accept(this);
        }
        
        if (node.Tag != null)
        {
            _writer.WritePropertyName("Tag");
            node.Tag.Accept(this);
        }
        
        _writer.WritePropertyName("Cases");
        _writer.WriteStartArray();
        foreach (var caseClause in node.Cases)
        {
            _writer.WriteStartObject();
            
            if (caseClause.Expressions != null)
            {
                WriteArray("Expressions", caseClause.Expressions);
            }
            
            WriteArray("Body", caseClause.Body);
            
            _writer.WriteEndObject();
        }
        _writer.WriteEndArray();
        
        WriteNodeEnd();
    }
    
    public override void VisitIncDecStatement(IncDecStatement node)
    {
        WriteNodeStart("IncDecStatement", node);
        
        _writer.WritePropertyName("Expression");
        node.Expression.Accept(this);
        
        _writer.WriteString("IsIncrement", node.IsIncrement.ToString());
        
        WriteNodeEnd();
    }
    
    public override void VisitLabeledStatement(LabeledStatement node)
    {
        WriteNodeStart("LabeledStatement", node);
        
        _writer.WriteString("Label", node.Label);
        
        _writer.WritePropertyName("Statement");
        node.Statement.Accept(this);
        
        WriteNodeEnd();
    }
    
    public override void VisitSendStatement(SendStatement node)
    {
        WriteNodeStart("SendStatement", node);
        
        _writer.WritePropertyName("Channel");
        node.Channel.Accept(this);
        
        _writer.WritePropertyName("Value");
        node.Value.Accept(this);
        
        WriteNodeEnd();
    }
    
    public override void VisitEmptyStatement(EmptyStatement node)
    {
        WriteNodeStart("EmptyStatement", node);
        WriteNodeEnd();
    }
    
    public override void VisitFallthroughStatement(FallthroughStatement node)
    {
        WriteNodeStart("FallthroughStatement", node);
        WriteNodeEnd();
    }
    
    public override void VisitForRangeStatement(ForRangeStatement node)
    {
        WriteNodeStart("ForRangeStatement", node);
        
        if (node.Key != null)
            _writer.WriteString("Key", node.Key);
            
        if (node.Value != null)
            _writer.WriteString("Value", node.Value);
            
        _writer.WritePropertyName("Range");
        node.Range.Accept(this);
        
        _writer.WritePropertyName("Body");
        node.Body.Accept(this);
        
        WriteNodeEnd();
    }
    
    public override void VisitTypeSwitchStatement(TypeSwitchStatement node)
    {
        WriteNodeStart("TypeSwitchStatement", node);
        
        if (node.Init != null)
        {
            _writer.WritePropertyName("Init");
            node.Init.Accept(this);
        }
        
        _writer.WritePropertyName("Assign");
        node.Assign.Accept(this);
        
        _writer.WritePropertyName("Cases");
        _writer.WriteStartArray();
        foreach (var caseClause in node.Cases)
        {
            _writer.WriteStartObject();
            
            if (caseClause.Types != null)
            {
                WriteArray("Types", caseClause.Types);
            }
            
            WriteArray("Statements", caseClause.Statements);
            
            _writer.WriteEndObject();
        }
        _writer.WriteEndArray();
        
        WriteNodeEnd();
    }
    
    public override void VisitSelectStatement(SelectStatement node)
    {
        WriteNodeStart("SelectStatement", node);
        
        _writer.WritePropertyName("Cases");
        _writer.WriteStartArray();
        foreach (var commClause in node.Cases)
        {
            _writer.WriteStartObject();
            
            if (commClause.Comm != null)
            {
                _writer.WritePropertyName("Comm");
                commClause.Comm.Accept(this);
            }
            
            WriteArray("Statements", commClause.Statements);
            
            _writer.WriteEndObject();
        }
        _writer.WriteEndArray();
        
        WriteNodeEnd();
    }
    
    public override void VisitTypeInstantiation(TypeInstantiation node)
    {
        WriteNodeStart("TypeInstantiation", node);
        
        _writer.WritePropertyName("BaseType");
        node.BaseType.Accept(this);
        
        WriteArray("TypeArguments", node.TypeArguments);
        
        WriteNodeEnd();
    }
    
    public override void VisitTypeUnion(TypeUnion node)
    {
        WriteNodeStart("TypeUnion", node);
        
        WriteArray("Terms", node.Terms);
        
        WriteNodeEnd();
    }
    
    public override void VisitTypeTerm(TypeTerm node)
    {
        WriteNodeStart("TypeTerm", node);
        
        _writer.WriteBoolean("IsUnderlying", node.IsUnderlying);
        
        _writer.WritePropertyName("Type");
        node.Type.Accept(this);
        
        WriteNodeEnd();
    }
    
    public override void VisitTypeElement(TypeElement node)
    {
        WriteNodeStart("TypeElement", node);
        
        _writer.WritePropertyName("Type");
        node.Type.Accept(this);
        
        WriteNodeEnd();
    }
}