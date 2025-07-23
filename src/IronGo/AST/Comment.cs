namespace IronGo.AST;

/// <summary>
/// Represents a comment in Go source code
/// </summary>
public class Comment : GoNodeBase
{
    public string Text { get; }
    public bool IsLineComment { get; }
    
    public Comment(string text, bool isLineComment, Position start, Position end) : base(start, end)
    {
        Text = text;
        IsLineComment = isLineComment;
    }
    
    public override void Accept(IGoAstVisitor visitor) => visitor.VisitComment(this);
    public override T Accept<T>(IGoAstVisitor<T> visitor) => visitor.VisitComment(this);
}