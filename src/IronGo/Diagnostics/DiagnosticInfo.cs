using System;
using System.Collections.Generic;
using IronGo.AST;

namespace IronGo.Diagnostics;

/// <summary>
/// Represents diagnostic information about the parsed source
/// </summary>
public class DiagnosticInfo
{
    /// <summary>
    /// Source file being analyzed
    /// </summary>
    public SourceFile SourceFile { get; }
    
    /// <summary>
    /// Parse errors encountered
    /// </summary>
    public IReadOnlyList<ParseError> Errors { get; }
    
    /// <summary>
    /// Parse warnings
    /// </summary>
    public IReadOnlyList<DiagnosticWarning> Warnings { get; }
    
    /// <summary>
    /// Parse time in milliseconds
    /// </summary>
    public double ParseTimeMs { get; }
    
    /// <summary>
    /// Number of tokens processed
    /// </summary>
    public int TokenCount { get; }
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; }
    
    /// <summary>
    /// Number of lines in the source
    /// </summary>
    public int LineCount { get; }
    
    public DiagnosticInfo(
        SourceFile sourceFile,
        IReadOnlyList<ParseError> errors,
        IReadOnlyList<DiagnosticWarning> warnings,
        double parseTimeMs,
        int tokenCount,
        long fileSizeBytes,
        int lineCount)
    {
        SourceFile = sourceFile;
        Errors = errors;
        Warnings = warnings;
        ParseTimeMs = parseTimeMs;
        TokenCount = tokenCount;
        FileSizeBytes = fileSizeBytes;
        LineCount = lineCount;
    }
}

/// <summary>
/// Represents a diagnostic warning
/// </summary>
public class DiagnosticWarning
{
    public int Line { get; set; }
    public int Column { get; set; }
    public string Message { get; set; } = "";
}

/// <summary>
/// Represents a parse warning
/// </summary>
public class ParseWarning
{
    public Position Position { get; }
    public string Message { get; }
    public WarningLevel Level { get; }
    
    public ParseWarning(Position position, string message, WarningLevel level = WarningLevel.Warning)
    {
        Position = position;
        Message = message;
        Level = level;
    }
    
    public override string ToString() => $"{Position}: {Level}: {Message}";
}

/// <summary>
/// Warning severity levels
/// </summary>
public enum WarningLevel
{
    Info,
    Warning,
    Suggestion
}

/// <summary>
/// Source location range
/// </summary>
public readonly struct SourceRange
{
    public Position Start { get; }
    public Position End { get; }
    
    public SourceRange(Position start, Position end)
    {
        Start = start;
        End = end;
    }
    
    public static SourceRange FromNode(IGoNode node) => new(node.Start, node.End);
    
    public bool Contains(Position position)
    {
        return position.Offset >= Start.Offset && position.Offset <= End.Offset;
    }
    
    public override string ToString() => $"{Start}-{End}";
}