using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace MarketAlly.IronGo.Parser;

public abstract class GoParserBase : Antlr4.Runtime.Parser
{
    protected GoParserBase(ITokenStream input) : base(input)
    {
    }

    protected GoParserBase(ITokenStream input, TextWriter output, TextWriter errorOutput) : base(input, output, errorOutput)
    {
    }

    protected bool lineTerminatorAhead()
    {
        var possibleIndexEosToken = CurrentToken.TokenIndex - 1;
        if (possibleIndexEosToken < 0)
            return false;

        var prevToken = TokenStream.Get(possibleIndexEosToken);
        if (prevToken.Channel != Lexer.Hidden)
            return false;

        if (prevToken.Type == GoLexer.TERMINATOR)
            return true;

        if (prevToken.Type == GoLexer.WS)
            return prevToken.Text.Contains('\r') || prevToken.Text.Contains('\n');

        if (prevToken.Type == GoLexer.COMMENT || prevToken.Type == GoLexer.LINE_COMMENT)
            return true;

        return false;
    }

    protected bool noTerminatorBetween(int tokenOffset)
    {
        var stream = TokenStream as BufferedTokenStream;
        if (stream == null)
            return true;
            
        var start = stream.Index + tokenOffset;
        var stop = CurrentToken.TokenIndex - 1;
        
        if (start < 0 || start > stop)
            return true;

        for (int i = start; i <= stop; i++)
        {
            var token = stream.Get(i);
            if (token.Channel == Lexer.Hidden && token.Type == GoLexer.TERMINATOR)
                return false;
        }
        
        return true;
    }

    protected bool noTerminatorAfterParams(int tokenOffset)
    {
        var stream = TokenStream;
        var leftParams = 1;
        var rightParams = 0;
        var tokenIndex = stream.Index + tokenOffset;

        while (leftParams != rightParams && tokenIndex < stream.Size)
        {
            var token = stream.Get(tokenIndex);
            
            if (token.Type == GoLexer.L_PAREN)
                leftParams++;
            else if (token.Type == GoLexer.R_PAREN)
            {
                rightParams++;
                tokenIndex++;
                break;
            }
            
            tokenIndex++;
        }

        while (tokenIndex < stream.Size)
        {
            var token = stream.Get(tokenIndex);
            
            if (token.Channel == Lexer.Hidden)
            {
                if (token.Type == GoLexer.TERMINATOR)
                    return false;
            }
            else
            {
                return true;
            }
            
            tokenIndex++;
        }
        
        return true;
    }

    protected bool checkPreviousTokenText(string text)
    {
        var tokenStream = TokenStream;
        var previousTokenIndex = tokenStream.Index - 1;
        
        if (previousTokenIndex < 0)
            return false;

        var previousToken = tokenStream.Get(previousTokenIndex);
        return previousToken.Text?.Equals(text, StringComparison.Ordinal) ?? false;
    }

    protected void myreset()
    {
        // This method can be used for any necessary parser state reset
        // Currently empty as the original Go implementation doesn't require state tracking in C#
    }

    protected void addImportSpec()
    {
        // Implementation for import spec tracking
    }

    protected bool isNotReceive()
    {
        // Check if the current expression is not a receive operation
        return !isReceiveOp();
    }

    protected bool isOperand()
    {
        // Check if the current token can be an operand
        var token = CurrentToken;
        switch (token.Type)
        {
            case GoLexer.IDENTIFIER:
            case GoLexer.DECIMAL_LIT:
            case GoLexer.BINARY_LIT:
            case GoLexer.OCTAL_LIT:
            case GoLexer.HEX_LIT:
            case GoLexer.FLOAT_LIT:
            case GoLexer.IMAGINARY_LIT:
            case GoLexer.RUNE_LIT:
            case GoLexer.RAW_STRING_LIT:
            case GoLexer.INTERPRETED_STRING_LIT:
            case GoLexer.L_PAREN:
            case GoLexer.L_BRACKET:
            case GoLexer.L_CURLY:
            case GoLexer.FUNC:
            case GoLexer.INTERFACE:
            case GoLexer.MAP:
            case GoLexer.STRUCT:
            case GoLexer.CHAN:
            case GoLexer.NIL_LIT:
                return true;
            default:
                return false;
        }
    }

    protected bool isConversion()
    {
        // Check if this is a type conversion
        // Look ahead to see if we have a type followed by '('
        var lt1 = TokenStream.LT(1);
        var lt2 = TokenStream.LT(2);
        
        if (lt1 == null || lt2 == null)
            return false;
            
        // Simple check: identifier followed by '('
        return lt1.Type == GoLexer.IDENTIFIER && lt2.Type == GoLexer.L_PAREN;
    }

    protected bool isMethodExpr()
    {
        // Check if this is a method expression
        // Look for pattern: Type.Method
        var lt1 = TokenStream.LT(1);
        var lt2 = TokenStream.LT(2);
        var lt3 = TokenStream.LT(3);
        
        if (lt1 == null || lt2 == null || lt3 == null)
            return false;
            
        return lt1.Type == GoLexer.IDENTIFIER && 
               lt2.Type == GoLexer.DOT && 
               lt3.Type == GoLexer.IDENTIFIER;
    }

    protected bool closingBracket()
    {
        // Handle implicit semicolon insertion before closing brackets
        // Check if next token is a closing bracket
        var nextToken = TokenStream.LT(1);
        if (nextToken != null)
        {
            var tokenType = nextToken.Type;
            if (tokenType == GoLexer.R_CURLY || tokenType == GoLexer.R_PAREN || tokenType == GoLexer.R_BRACKET)
            {
                return true;
            }
        }
        
        // Also check for line terminator (original behavior)
        return lineTerminatorAhead();
    }

    private bool isReceiveOp()
    {
        // Check if we have a receive operation '<-chan'
        var lt1 = TokenStream.LT(1);
        var lt2 = TokenStream.LT(2);
        
        if (lt1 == null || lt2 == null)
            return false;
            
        return lt1.Type == GoLexer.RECEIVE && lt2.Type == GoLexer.CHAN;
    }
}