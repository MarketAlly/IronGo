using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Antlr4.Runtime;
using IronGo.AST;

namespace IronGo.Diagnostics;

/// <summary>
/// Collects diagnostic information during parsing
/// </summary>
internal class DiagnosticCollector
{
    private readonly List<ParseError> _errors = new();
    private readonly List<DiagnosticWarning> _warnings = new();
    private readonly Stopwatch _stopwatch = new();
    private int _tokenCount;
    private long _fileSizeBytes;
    private int _lineCount;
    
    public bool HasErrors => _errors.Count > 0;
    public IReadOnlyList<ParseError> Errors => _errors;
    
    public void StartParsing()
    {
        _stopwatch.Restart();
    }
    
    public void StopParsing()
    {
        _stopwatch.Stop();
    }
    
    public void AddError(ParseError error)
    {
        _errors.Add(error);
    }
    
    public void AddWarning(DiagnosticWarning warning)
    {
        _warnings.Add(warning);
    }
    
    public void SetTokenCount(int count)
    {
        _tokenCount = count;
    }
    
    public void SetFileInfo(long sizeBytes, int lineCount)
    {
        _fileSizeBytes = sizeBytes;
        _lineCount = lineCount;
    }
    
    public DiagnosticInfo CreateDiagnosticInfo(SourceFile sourceFile)
    {
        return new DiagnosticInfo(
            sourceFile,
            _errors,
            _warnings,
            _stopwatch.Elapsed.TotalMilliseconds,
            _tokenCount,
            _fileSizeBytes,
            _lineCount);
    }
}

/// <summary>
/// Analyzes AST for potential issues and collects warnings
/// </summary>
public class AstAnalyzer : GoAstWalker
{
    private readonly List<DiagnosticWarning> _warnings = new();
    private readonly HashSet<string> _declaredFunctions = new();
    private readonly HashSet<string> _declaredTypes = new();
    private readonly HashSet<string> _importedPackages = new();
    
    public IReadOnlyList<DiagnosticWarning> Warnings => _warnings;
    
    public override void VisitSourceFile(SourceFile node)
    {
        // Collect all top-level declarations first
        foreach (var decl in node.Declarations)
        {
            if (decl is FunctionDeclaration func)
                _declaredFunctions.Add(func.Name);
            else if (decl is TypeDeclaration typeDecl && typeDecl.Name != null)
                _declaredTypes.Add(typeDecl.Name);
        }
        
        base.VisitSourceFile(node);
    }
    
    public override void VisitImportSpec(ImportSpec node)
    {
        if (_importedPackages.Contains(node.Path))
        {
            _warnings.Add(new DiagnosticWarning
            {
                Line = node.Start.Line,
                Column = node.Start.Column,
                Message = $"Duplicate import of package '{node.Path}'"
            });
        }
        else
        {
            _importedPackages.Add(node.Path);
        }
        
        base.VisitImportSpec(node);
    }
    
    public override void VisitFunctionDeclaration(FunctionDeclaration node)
    {
        // Check for functions with too many parameters
        var totalParamCount = node.Parameters.Sum(p => p.Names.Count);
        if (totalParamCount > 7)
        {
            _warnings.Add(new DiagnosticWarning
            {
                Line = node.Start.Line,
                Column = node.Start.Column,
                Message = $"Function '{node.Name}' has {totalParamCount} parameters; consider using a struct"
            });
        }
        
        // Check for empty function bodies (except in interfaces)
        if (node.Body != null && node.Body.Statements.Count == 0)
        {
            _warnings.Add(new DiagnosticWarning
            {
                Line = node.Start.Line,
                Column = node.Start.Column,
                Message = $"Function '{node.Name}' has an empty body"
            });
        }
        
        base.VisitFunctionDeclaration(node);
    }
    
    public override void VisitVariableDeclaration(VariableDeclaration node)
    {
        // Check for unused looking variable names
        foreach (var spec in node.Specs)
        {
            foreach (var name in spec.Names)
            {
                if (name == "_")
                    continue; // Blank identifier is intentionally unused
                    
                if (name.StartsWith("_") && name.Length > 1)
                {
                    _warnings.Add(new DiagnosticWarning
                    {
                        Line = node.Start.Line,
                        Column = node.Start.Column,
                        Message = $"Variable '{name}' starts with underscore but is not a blank identifier; consider using '_' if unused"
                    });
                }
            }
        }
        
        base.VisitVariableDeclaration(node);
    }
    
    public override void VisitShortVariableDeclaration(ShortVariableDeclaration node)
    {
        // Check for unused looking variable names
        foreach (var name in node.Names)
        {
            if (name == "_")
                continue; // Blank identifier is intentionally unused
                
            if (name.StartsWith("_") && name.Length > 1)
            {
                _warnings.Add(new DiagnosticWarning
                {
                    Line = node.Start.Line,
                    Column = node.Start.Column,
                    Message = $"Variable '{name}' starts with underscore but is not a blank identifier; consider using '_' if unused"
                });
            }
        }
        
        base.VisitShortVariableDeclaration(node);
    }
    
    public override void VisitIfStatement(IfStatement node)
    {
        // Check for if statements with empty then clause
        if (node.Then is BlockStatement block && block.Statements.Count == 0)
        {
            _warnings.Add(new DiagnosticWarning
            {
                Line = node.Start.Line,
                Column = node.Start.Column,
                Message = "If statement has empty then clause"
            });
        }
        
        base.VisitIfStatement(node);
    }
    
    public override void VisitForStatement(ForStatement node)
    {
        // Check for infinite loops without break
        if (node.Condition == null && node.Init == null && node.Post == null)
        {
            var hasBreak = CheckForBreakStatement(node.Body);
            if (!hasBreak)
            {
                _warnings.Add(new DiagnosticWarning
                {
                    Line = node.Start.Line,
                    Column = node.Start.Column,
                    Message = "Infinite loop detected without break statement"
                });
            }
        }
        
        base.VisitForStatement(node);
    }
    
    private bool CheckForBreakStatement(IStatement statement)
    {
        if (statement is BranchStatement branch && branch.Kind == BranchKind.Break)
            return true;
            
        if (statement is BlockStatement block)
            return block.Statements.Any(CheckForBreakStatement);
            
        if (statement is IfStatement ifStmt)
        {
            if (CheckForBreakStatement(ifStmt.Then))
                return true;
            if (ifStmt.Else != null && CheckForBreakStatement(ifStmt.Else))
                return true;
        }
        
        if (statement is ExpressionStatement)
            return false;
            
        return false;
    }
}